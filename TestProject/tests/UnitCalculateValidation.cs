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
    public async Task TestCorrectFormatReturnsNoError()
    {
        
        // with and without decimal point
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g poteter","1.5 g poteter"} } );
        Assert.Null(result1.Value.ErrorCode);
        
        // with parenteces
        var result2 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g poteter(beate)","2 g gulrøtter (i biter)"} } );
        Assert.Null(result2.Value.ErrorCode);
        
        // with description word before and after ingredient
        var result3 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g poteter, strimlet","200 g strimlede gulrøtter"} } );
        Assert.Null(result3.Value.ErrorCode);
       
        // multi-word ingredient
        var result4 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g cottage cheese","500 g blå amerikansk ost"} } );
        Assert.Null(result4.Value.ErrorCode);
        
        // multi-word ingredient with description word before and after ingredient
        var result5 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g amerikansk ost, revet","100 g revet amerikansk ost"} } );
        Assert.Null(result5.Value.ErrorCode);
    }
    
    [Fact]
    public async Task TestIngredientOptionReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"2 kg gultøtter eller poteter"} } );
        
        Assert.Equivalent(IngredientsError.OPTION, result.Value.ErrorCode);
    }
    
    [Fact]
    public async Task TestIngredientAmountRangeReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"2-5 stk poteter"} } );
        
        Assert.Equivalent(IngredientsError.AMOUNT_RANGE, result.Value.ErrorCode);
    }
    
     
    [Fact]
    public async Task TestIngredientInvalidAmountFormatReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"..8 gram poteter"} } );
        
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_AMOUNT, result.Value.ErrorCode);
    }
    
    [Fact]
    public async Task TestIngredientInvalidFormatReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"1 stk stor og god gulrot!"} } );
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result1.Value.ErrorCode);
        
        var result2 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"1 stk gulrot, rød og stor"} } );
        
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result2.Value.ErrorCode);
        
        var result3 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"1 stk gulrot, rød og stor"} } );
        
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result3.Value.ErrorCode);
        
        var result4 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"1 stk gulrot men bruk potet hvis du ikke har"} } );
        
        Assert.Equivalent(IngredientsError.INVALID_FORMAT_INGREDIENT, result4.Value.ErrorCode);
    }
    
    [Fact]
    public async Task TestOptionalIngredientReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"2 kg gulrøtter(kan sløyfes)"} } );
        
        Assert.Equivalent(IngredientsError.OPTIONAL, result1.Value.ErrorCode);
        
        var result2 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"2 kg gulrøtter (valgfritt)"} } );
        
        Assert.Equivalent(IngredientsError.OPTIONAL, result2.Value.ErrorCode);
    }

    [Fact]
    public async Task TestNonexistingIngredientReturnsError()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result1 = await controller.CalculateCalories(new CalculationItems
            { Items = new List<string> { "2 kg thisdoesnotexist" } });
        Assert.Equivalent(IngredientsError.INGREDIENT_NOT_FOUND, result1.Value.ErrorCode);
    }

    //    OPTION, INACCURATE_UNIT, OPTIONAL,INVALID_FORMAT,INVALID_FORMAT_UNIT,AMOUNT_RANGE

    [Fact]
    public async Task TestIngredientInaccurateUnit()
    {
        var controller = new CalculateController(MockData.getFakeDbContext(), _log);
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "en del egg" } });
        CalculateCaloriesResult resultModel1 = result1.Value;
        Assert.Equivalent(IngredientsError.INACCURATE_UNIT, resultModel1.ErrorCode);
        
        var result2 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "nok" } });
        CalculateCaloriesResult resultModel2 = result2.Value;
        Assert.Equivalent(IngredientsError.INACCURATE_UNIT, resultModel2.ErrorCode);
    }
}