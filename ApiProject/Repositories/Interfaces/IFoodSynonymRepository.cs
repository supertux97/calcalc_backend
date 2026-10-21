using calcalc.Models;

namespace calcalc.Repositories;

public interface IFoodSynonymRepository
{
    Task<IEnumerable<FoodSynonym>> GetAllFoodItemsAsync();
}
