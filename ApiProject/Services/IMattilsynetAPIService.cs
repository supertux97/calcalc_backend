namespace calcalc.Services;

public interface IFooditemsService : IDisposable, IAsyncDisposable
{
    Task<FoodItemsResult?> GetFoodItems();
}