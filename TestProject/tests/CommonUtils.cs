using calcalc.Controllers;
using Microsoft.Extensions.Logging;

public static class CommonUtils
{
    public static CalculateController GetCalculateControllerWithInjectedServices(ILogger<CalculateController> log)
    {
        return new CalculateController(new FoodItemRepositoryMock(), new FoodUnitRepositoryMock(), new FoodSynonymRepositoryMock(), log, new IngredientLanguageSpecificParserStrategyNorwegian());
    }
}