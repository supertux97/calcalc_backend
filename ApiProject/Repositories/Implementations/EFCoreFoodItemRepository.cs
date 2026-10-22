using calcalc.Models;
using calcalc.Repositories;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

public class EFCorePsqlFoodItemRepository : IFoodItemRepository
{
    private readonly CalCalcContext _context;

    public EFCorePsqlFoodItemRepository(CalCalcContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FoodItem>> GetAllFoodItemsAsync(bool includeUnits, bool includeSynonyms)
    {
        IQueryable<FoodItem> items = _context.FoodItem;
        if (includeSynonyms)
        {
            items = items.Include(i => i.Synonyms);
        }
        if (includeUnits)
        {
            items = items.Include(i => i.Units);
        }
        return await items.ToListAsync();
    }

    public async Task<FoodItem?> GetFoodItemByIdAsync(bool includeUnits, bool includeSynonyms, int id)
    {
        var item = _context.FoodItem.Where(fi => fi.Id == id);
        if (includeSynonyms)
        {
            item = item.Include(i => i.Synonyms);
        }
        if (includeUnits)
        {
            item = item.Include(i => i.Units);
        }
        return await item.FirstOrDefaultAsync();
    }

    public async Task<FoodItemAddUpdateResult> AddFoodItemAsync(FoodItem foodItem)
    {
        // every new food item by default has grams unit
        if (!foodItem.Units.Any(u => u.FoodUnit.Name == "gram"))
        {
            var gramUnit = await _context.FoodUnit.Where(u => u.Name == "gram").FirstOrDefaultAsync();
            foodItem.Units.Add(new FoodHasUnit { FoodUnitId = gramUnit.Id, FoodUnit = gramUnit, GramsPerUnit = 1 });
        }

        var res = new FoodItemAddUpdateResult();
        var validateError = await ValidateAddEditFoodItem(foodItem);
        res.Error = validateError;
        if (res.Error != null)
        {
            return res;
        }

        _context.FoodItem.Add(foodItem);
        await _context.SaveChangesAsync();
        return res;
    }

    private async Task<FoodItemUpdateResultErrorCause?> ValidateAddEditFoodItem(FoodItem foodItem)
    {
        var isEdit = foodItem.Id > 0;

        var existingFoodItemWithSameName = await _context.FoodItem.Where(fi => (!isEdit || fi.Id != foodItem.Id) && fi.Name.ToLower() == foodItem.Name.ToLower()).FirstOrDefaultAsync() != null;
        if (existingFoodItemWithSameName)
        {
            return FoodItemUpdateResultErrorCause.FOOD_ITEM_NAME_ALREADY_EXISTS;
        }
        var existingFoodSynonymItemWithSameName = _context.FoodSynonym.Any(fs => fs.Name.ToLower() == foodItem.Name.ToLower());
        if (existingFoodSynonymItemWithSameName)
        {
            return FoodItemUpdateResultErrorCause.FOOD_ITEM_NAME_ALREADY_EXISTS_IN_SYNONYM;
        }

        var synonymsNames = foodItem.Synonyms.Select(s => s.Name);
        if (synonymsNames.Distinct().Count() != synonymsNames.Count())
        {
            return FoodItemUpdateResultErrorCause.DUPLICATE_SYNONYMS_TO_ADD;
        }

        foreach (var newSynonym in foodItem.Synonyms)
        {
            var synonymAlreadyExists = _context.FoodItem.Include(fi => fi.Synonyms).Any(existingFoodItem => existingFoodItem.Id != foodItem.Id && existingFoodItem.Synonyms.Any(existingSynonym => existingSynonym.Name.ToLower() == newSynonym.Name.ToLower()));
            if (synonymAlreadyExists)
            {
                return FoodItemUpdateResultErrorCause.SYNONYM_ALREADY_EXISTS;
            }
            var synonymExistsAsFoodName = await _context.FoodItem.Where(existingFoodItem => foodItem.Id != existingFoodItem.Id && existingFoodItem.Name.ToLower() == newSynonym.Name.ToLower()).FirstOrDefaultAsync() != null;
            if (synonymExistsAsFoodName)
            {
                return FoodItemUpdateResultErrorCause.SYNONYM_ALREADY_EXISTS_AS_FOOD_ITEM_NAME;
            }
        }
        return null;
    }

    public async Task<FoodItemAddUpdateResult> UpdateFoodItemAsync(FoodItem foodItem)
    {
        var res = new FoodItemAddUpdateResult();
        var validateError = await ValidateAddEditFoodItem(foodItem);
        res.Error = validateError;
        if (res.Error != null)
        {
            return res;
        }

        _context.FoodItem.Update(foodItem);
        await _context.SaveChangesAsync();
        return res;
    }

    public async Task DeleteFoodItemAsync(FoodItem foodItem)
    {
        _context.FoodItem.Remove(foodItem);
        await _context.SaveChangesAsync();
    }

    public async Task<SearchWithPaginationResult> SearchForFooditems(bool includeUnits, bool includeSynonyms, string searchStr, bool onlyHasSynonyms, bool onlyAddedByUser, int paginationPageNo, int paginationResultsPrPage)
    {
        var FUZZY_SEARCH_DIFFERENCES_THRESHOLD = 2;
        IQueryable<FoodItem> foodItemsQueryWithSearch = _context.FoodItem
            .Include(fi => fi.Synonyms)
            .Include(fi => fi.Units).ThenInclude(u => u.FoodUnit)
        .OrderBy(fi => fi.Name);
        if (!String.IsNullOrEmpty(searchStr))
        {
            foodItemsQueryWithSearch = foodItemsQueryWithSearch
                .Where(fi => _context.Fuzzysearch(fi.NameWithoutDetails.ToLower(), searchStr.ToLower()) < FUZZY_SEARCH_DIFFERENCES_THRESHOLD
                            || fi.NameWithoutDetails.ToLower().Contains(searchStr.ToLower())
            );
            foodItemsQueryWithSearch = foodItemsQueryWithSearch.OrderByDescending(i => i.NameWithoutDetails.ToLower().StartsWith(searchStr.ToLower()))
                .ThenByDescending(i => i.NameWithoutDetails.Contains(searchStr.ToLower()) ? 1 : -1)
                .ThenByDescending(i => -_context.Fuzzysearch(i.NameWithoutDetails.ToLower(), searchStr.ToLower()));
        }
        if (onlyHasSynonyms)
        {
            foodItemsQueryWithSearch = foodItemsQueryWithSearch.Where(fi => fi.Synonyms.Count() > 0);
        }
        if (onlyAddedByUser)
        {
            foodItemsQueryWithSearch = foodItemsQueryWithSearch.Where(fi => fi.UserAdded == true);
        }
        var cntAllItems = await foodItemsQueryWithSearch.CountAsync();
        foodItemsQueryWithSearch = foodItemsQueryWithSearch.Skip(Math.Max(paginationPageNo - 1, 0) * paginationResultsPrPage).Take(paginationResultsPrPage);
        var res = await foodItemsQueryWithSearch.ToListAsync();

        return new SearchWithPaginationResult
        {
            Items = await foodItemsQueryWithSearch.ToListAsync(),
            HasNextPage = cntAllItems > paginationPageNo * paginationResultsPrPage,
            HasPreviousPage = paginationPageNo > 1
        };

    }
}