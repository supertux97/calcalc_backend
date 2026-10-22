using calcalc.Models;
using calcalc.Repositories;

public class FoodSynonymRepositoryMock : IFoodSynonymRepository
{
    public async Task<IEnumerable<FoodSynonym>> GetAllSynonymsItemsAsync()
    {
        return new List<FoodSynonym> {
                new FoodSynonym {
                FoodItemId = 3, Id = 1, Name = "Melk"
                },
                new FoodSynonym {
                FoodItemId = 3, Id = 2, Name = "Vanlig melk"
                },
            };
    }
}