using calcalc.Models;
using calcalc.Repositories;

public class FoodItemRepositoryMock : IFoodItemRepository
{
    public Task<FoodItemAddUpdateResult> AddFoodItemAsync(FoodItem foodItem)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFoodItemAsync(FoodItem foodItem)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<FoodItem>> GetAllFoodItemsAsync(bool includeUnits, bool includeSynonyms)
    {
        var foodItems = new List<FoodItem> {
        new FoodItem {
            Id = 3, Name = "Melk a", Calories = 100m,
            Units = new List < FoodHasUnit > {
                new FoodHasUnit {
                FoodUnit = new FoodUnit {
                    Id = 2, Name = "desiliter",
                    Synonyms = new List < FoodUnitSynonym > {
                        new FoodUnitSynonym {
                        Id = 2, FoodUnitId = 2, Name = "dl"
                        },
                        new FoodUnitSynonym {
                        Id = 3, FoodUnitId = 2, Name = "dl."
                        },
                    }
                }, FoodItemId = 3, FoodUnitId = 2, GramsPerUnit = 100
                },
                new FoodHasUnit {
                FoodItemId = 3, FoodUnitId = 1, GramsPerUnit = 1, FoodUnit = new FoodUnit {
                    Id = 1, Name = "gram",
                    Synonyms = new List < FoodUnitSynonym > {
                        new FoodUnitSynonym {
                        Id = 1, FoodUnitId = 1, Name = "g"
                        }
                    }
                }
                }
            },
            Synonyms = new List < FoodSynonym > {
                new FoodSynonym {
                FoodItemId = 3, Id = 1, Name = "Melk"
                },
                new FoodSynonym {
                FoodItemId = 3, Id = 2, Name = "Vanlig melk"
                },
            }
        },
        new FoodItem {
            Id = 37, Name = "Havregryn", Calories = 369m, Units = new List < FoodHasUnit > {
            new FoodHasUnit {
                FoodItemId = 37, FoodUnitId = 2, GramsPerUnit = 40, FoodUnit = new FoodUnit {
                Id = 2, Name = "desiliter",
                    Synonyms = new List < FoodUnitSynonym > {
                    new FoodUnitSynonym {
                        Id = 2, FoodUnitId = 2, Name = "dl"
                    },
                    new FoodUnitSynonym {
                        Id = 3, FoodUnitId = 2, Name = "dl."
                    },
                    }
                }
            },

            new FoodHasUnit {
                FoodItemId = 37, FoodUnitId = 1, GramsPerUnit = 100, FoodUnit = new FoodUnit {
                Id = 1, Name = "gram",
                    Synonyms = new List < FoodUnitSynonym > {
                    new FoodUnitSynonym {
                        Id = 1, FoodUnitId = 1, Name = "g"
                    }
                    }
                }
            }
            }
        },
            };
        return foodItems;
    }

    public Task<FoodItem?> GetFoodItemByIdAsync(bool includeUnits, bool includeSynonyms, int id)
    {
        throw new NotImplementedException();
    }

    public Task<SearchWithPaginationResult> SearchForFooditems(bool includeUnits, bool includeSynonyms, string searchStr, bool onlyHasSynonyms, bool onlyAddedByUser, int paginationPageNo, int paginationResultsPrPage)
    {
        throw new NotImplementedException();
    }

    public Task<FoodItemAddUpdateResult> UpdateFoodItemAsync(FoodItem foodItem)
    {
        throw new NotImplementedException();
    }
}