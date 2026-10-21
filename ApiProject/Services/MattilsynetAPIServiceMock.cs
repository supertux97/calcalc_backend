namespace calcalc.Services;

public class FoodItemsServiceMock : IFooditemsService
{
    private ILogger _logger;
    public FoodItemsServiceMock(ILogger<FoodItemsServiceMock> logger)
    {
        _logger = logger;
    }

    public async Task<FoodItemsResult?> GetFoodItems()
    {
        _logger.LogInformation("Getting items from mock!");
        return new FoodItemsResult
        {
            Foods = new List<FoodItem> {new FoodItem
        {
            FoodName = "Popcorntest",

            Calories = new Calories {Quantity = 999},
            Portions = new List<Portion>()
        } }
        };
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}