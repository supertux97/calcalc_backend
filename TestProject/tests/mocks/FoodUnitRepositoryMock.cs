using calcalc.Models;
using calcalc.Repositories;

public class FoodUnitRepositoryMock : IFoodUnitRepository
{
    public Task AddFoodUnitAsync(FoodUnit foodUnit)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFoodUnitAsync(FoodUnit foodUnit)
    {
        throw new NotImplementedException();
    }

    public async Task<List<FoodUnit>> GetAllFoodUnitsAsync(bool includeSynonyms)
    {
        var units = new List<FoodUnit>
        {
            new FoodUnit
            {
                Id = 1, Name = "gram",
                Synonyms = new List<FoodUnitSynonym> { new FoodUnitSynonym { Id = 1, FoodUnitId = 1, Name = "g" } }
            },
            new FoodUnit
            {
                Id = 2, Name = "desiliter",
                Synonyms = new List<FoodUnitSynonym>
                {
                    new FoodUnitSynonym { Id = 2, FoodUnitId = 2, Name = "dl" },
                    new FoodUnitSynonym { Id = 3, FoodUnitId = 2, Name = "dl." },
                }
            },
            new FoodUnit
            {
                Id = 3, Name = "stykk",
                Synonyms = new List<FoodUnitSynonym>
                {
                    new FoodUnitSynonym { Id = 4, FoodUnitId = 3, Name = "stk" },
                    new FoodUnitSynonym { Id = 5, FoodUnitId = 3, Name = "stk." },
                }
            }
        };
        return units;
    }

    public Task<FoodUnit?> GetFoodUnitByIdAsync(bool includeSynonyms, int id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateFoodUnitAsync(FoodUnit foodUnit)
    {
        throw new NotImplementedException();
    }
}