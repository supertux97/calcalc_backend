namespace calcalc.Repositories;

public interface IFoodItemRepository
{
    Task<IEnumerable<FoodItem>> GetAllFoodItemsAsync(bool includeUnits, bool includeSynonyms);
    Task<FoodItem?> GetFoodItemByIdAsync(bool includeUnits, bool includeSynonyms, int id);
    Task AddFoodItemAsync(FoodItem foodItem);
    Task<FoodItemUpdateResult> UpdateFoodItemAsync(FoodItem foodItem);
    Task DeleteFoodItemAsync(FoodItem foodItem);
    Task<SearchWithPaginationResult> SearchForFooditems(bool includeUnits, bool includeSynonyms, string searchStr, bool onlyHasSynonyms, bool onlyAddedByUser, int paginationPageNo, int paginationResultsPrPage);
}

public class SearchWithPaginationResult
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public List<FoodItem> Items { get; set; }
}

public class FoodItemUpdateResult
{
    public FoodItemUpdateResultErrorCause? Error { get; set; }
}
public enum FoodItemUpdateResultErrorCause
{
    FOOD_ITEM_NAME_ALREADY_EXISTS, FOOD_ITEM_NAME_ALREADY_EXISTS_IN_SYNONYM, DUPLICATE_SYNONYMS_TO_ADD, SYNONYM_ALREADY_EXISTS, SYNONYM_ALREADY_EXISTS_AS_FOOD_ITEM_NAME
}