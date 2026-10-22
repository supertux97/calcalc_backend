using calcalc.Controllers;
using calcalc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace tests;

public class UnitCalculateValidation
{
    private readonly ILogger<CalculateController> _log;
    public UnitCalculateValidation()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _log = serviceProvider.GetService<ILogger<CalculateController>>();
    }


    [Fact]
    public async Task TestIngredientOptionReturnsError()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "2 kg gultøtter eller poteter" } });

        Assert.Equivalent(IngredientsError.OPTION, result.Value.ErrorCode);
    }

    [Fact]
    public async Task TestIngredientAmountRangeReturnsError()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "2-5 stk poteter" } });

        Assert.Equivalent(IngredientsError.AMOUNT_RANGE, result.Value.ErrorCode);
    }


    [Fact]
    public async Task TestIngredientInvalidAmountFormatReturnsError()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "..8 gram poteter" } });

        Assert.Equivalent(IngredientsError.INVALID_FORMAT_AMOUNT, result.Value.ErrorCode);
    }

    [Fact]
    public async Task TestIngredientInvalidFormatReturnsError()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "1 stk stor og god gulrot!" } });
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result1.Value.ErrorCode);

        var result2 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "1 stk gulrot, rød og stor" } });

        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result2.Value.ErrorCode);

        var result3 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "1 stk gulrot, rød og stor" } });

        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result3.Value.ErrorCode);

        var result4 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "1 stk gulrot men bruk potet hvis du ikke har" } });

        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result4.Value.ErrorCode);
    }


    [Fact]
    public async Task TestNonexistingIngredientReturnsError()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result1 = await controller.CalculateCalories(new CalculationItems
        { Items = new List<string> { "2 kg thisdoesnotexist" } });
        Assert.Equivalent(IngredientsError.INGREDIENT_NOT_FOUND, result1.Value.ErrorCode);
    }

    //    OPTION, INACCURATE_UNIT, OPTIONAL,INVALID_FORMAT,INVALID_FORMAT_UNIT,AMOUNT_RANGE

    [Fact]
    public async Task TestIngredientInaccurateUnit()
    {
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "en del egg" } });
        CalculateCaloriesResult resultModel1 = result1.Value;
        Assert.Equivalent(IngredientsError.INACCURATE_UNIT, resultModel1.ErrorCode);

        var result2 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "nok" } });
        CalculateCaloriesResult resultModel2 = result2.Value;
        Assert.Equivalent(IngredientsError.INACCURATE_UNIT, resultModel2.ErrorCode);
    }
}