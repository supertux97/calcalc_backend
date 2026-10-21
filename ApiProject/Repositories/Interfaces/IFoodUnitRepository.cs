using calcalc.Models;

namespace calcalc.Repositories;

public interface IFoodUnitRepository
{
    public Task<List<FoodUnit>> GetAllFoodUnitsAsync(bool includeSynonyms);
    public Task<FoodUnit?> GetFoodUnitByIdAsync(bool includeSynonyms, int id);
    public Task AddFoodUnitAsync(FoodUnit foodUnit);
    public Task UpdateFoodUnitAsync(FoodUnit foodUnit);
    public Task DeleteFoodUnitAsync(FoodUnit foodUnit);
}
