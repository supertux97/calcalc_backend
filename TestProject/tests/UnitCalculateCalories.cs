using calcalc.Controllers;
using calcalc.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace tests;

public class UnitCalculateCalories
{
    private readonly ILogger<CalculateController> _log;
    public UnitCalculateCalories()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .BuildServiceProvider();

        _log = serviceProvider.GetService<ILogger<CalculateController>>();
    }

    [Fact]
    public async Task TestCalculateCalories()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nb-NO");

        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "500 gram havregryn", "1,5 desiliter havregryn" } });
        // multiply so we comnvert to grams first, not directly use desoliter
        // this prevents rounding errors
        Assert.Equivalent((5 * 369) + (0.4 * 1.5 * 369), result1.Value.Calories);
    }

    [Fact]
    public async Task TestCalculateCaloriesSynonymsUnits()
    {
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nb-NO");
        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        // also test units that ends with a dot, eg dl.
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "500 g havregryn", "1,5 dl. havregryn", "2.5 dl havregryn" } });
        Assert.Equivalent((5 * 369) + (0.4 * 2.5 * 369) + (0.4 * 1.5 * 369), result1.Value.Calories);
    }

    [Fact]
    public async Task TestCalculateCaloriesSynonymsFoods()
    {

        var controller = CommonUtils.GetCalculateControllerWithInjectedServices(_log);
        // testing different units, synonym units (dl is syunony, to desiliter) as well as both "." and "," for decimals
        // also test units that ends with a dot, eg dl.
        var result1 = await controller.CalculateCalories(new CalculationItems { Items = new List<string> { "5 desiliter melk", "5 desiliter vanlig melk " } });
        // multiply so we comnvert to grams first, not directly use desoliter
        // this prevents rounding errors
        Assert.Equivalent((5 * 100) + (5 * 100), result1.Value.Calories);
    }



}