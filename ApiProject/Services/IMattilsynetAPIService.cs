namespace calcalc.Services;

public interface IMattilsynetAPIService: IDisposable, IAsyncDisposable
{
    Task<MattilsynetAPIFoodItemsResult?> GetFoodItems();
}