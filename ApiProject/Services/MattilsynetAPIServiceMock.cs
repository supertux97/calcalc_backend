namespace calcalc.Services;

public class MattilsynetAPIServiceMock:IMattilsynetAPIService
{
    private ILogger _logger;
    public MattilsynetAPIServiceMock(ILogger<MattilsynetAPIServiceMock> logger)
    {
        _logger = logger;
    }

    public async Task<MattilsynetAPIFoodItemsResult?> GetFoodItems()
    {
        _logger.LogInformation("Getting items from mock!");
        return new MattilsynetAPIFoodItemsResult{Foods = new List<MattilsynetAPIFoodItem> {new MattilsynetAPIFoodItem
        {
            FoodName = "Popcorntest",
            
            Calories = new Calories {Quantity = 999},
            Portions = new List<Portion>()
        } } };
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