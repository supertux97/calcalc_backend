using calcalc.Models;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

namespace tests;

public class MockData:IDisposable,IAsyncDisposable
{
    private static CalCalcContext _db;
    static MockData()
    {
        _db = getFakeDbContext();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    public static CalCalcContext getFakeDbContext()
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
       
        // 
        var foodItems = new List<FoodItem>
        {
            new FoodItem { Id = 1, Name = "Hvetemel", Calories = 100m },
            new FoodItem { Id = 2, Name = "Sukker", Calories = 100m },
            // 29 different milks 
            new FoodItem { Id = 3, Name = "Melk a", Calories = 100m,
                Units = new List<FoodHasUnit>
                {
                    new FoodHasUnit {FoodItemId = 3, FoodUnitId = 2, GramsPerUnit = 100}, 
                    new FoodHasUnit {FoodItemId = 3, FoodUnitId = 1, GramsPerUnit = 1}
                },
                Synonyms = new List<FoodSynonym>
            {
                new FoodSynonym {FoodItemId = 3, Id= 1, Name = "Melk"},
                new FoodSynonym {FoodItemId = 3, Id= 2, Name = "Vanlig melk"},
            }},
            new FoodItem { Id = 4, Name = "Melk b", Calories = 100m },
            new FoodItem { Id = 5, Name = "Melk c", Calories = 100m },
            new FoodItem { Id = 6, Name = "Melk d", Calories = 100m },
            new FoodItem { Id = 7, Name = "Melk e", Calories = 100m },
            new FoodItem { Id = 8, Name = "Melk f", Calories = 100m },
            new FoodItem { Id = 9, Name = "Melk g", Calories = 100m },
            new FoodItem { Id = 10, Name = "Melk h", Calories = 100m },
            new FoodItem { Id = 11, Name = "Melk i", Calories = 100m },
            new FoodItem { Id = 12, Name = "Melk j", Calories = 100m },
            new FoodItem { Id = 13, Name = "Melk k", Calories = 100m },
            new FoodItem { Id = 14, Name = "Melk l", Calories = 100m },
            new FoodItem { Id = 15, Name = "Melk m", Calories = 100m },
            new FoodItem { Id = 16, Name = "Melk n", Calories = 100m },
            new FoodItem { Id = 17, Name = "Melk o", Calories = 100m },
            new FoodItem { Id = 18, Name = "Melk p", Calories = 100m },
            new FoodItem { Id = 19, Name = "Melk q", Calories = 100m },
            new FoodItem { Id = 20, Name = "Melk r", Calories = 100m },
            new FoodItem { Id = 21, Name = "Melk s", Calories = 100m },
            new FoodItem { Id = 22, Name = "Melk t", Calories = 100m },
            new FoodItem { Id = 23, Name = "Melk u", Calories = 100m },
            new FoodItem { Id = 24, Name = "Melk v", Calories = 100m },
            new FoodItem { Id = 25, Name = "Melk w", Calories = 100m },
            new FoodItem { Id = 26, Name = "Melk x", Calories = 100m },
            new FoodItem { Id = 27, Name = "Melk y", Calories = 100m },
            new FoodItem { Id = 28, Name = "Melk z", Calories = 100m },
            new FoodItem { Id = 29, Name = "Melk æ", Calories = 100m },
            new FoodItem { Id = 30, Name = "Melk ø", Calories = 100m },
            new FoodItem { Id = 31, Name = "Melk å", Calories = 100m },
            new FoodItem { Id = 32, Name = "Poteter", Calories = 100m, Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 32, FoodUnitId = 1, GramsPerUnit = 100}} },
            new FoodItem { Id = 33, Name = "Gulrøtter", Calories = 100m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 33, FoodUnitId = 1, GramsPerUnit = 100}} },
            new FoodItem { Id = 34, Name = "Cottage Cheese", Calories = 100m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 34, FoodUnitId = 1, GramsPerUnit = 100}}  },
            new FoodItem { Id = 35, Name = "Amerikansk ost", Calories = 100m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 35, FoodUnitId = 1, GramsPerUnit = 100}}  },
            new FoodItem { Id = 36, Name = "Blå Amerikansk ost", Calories = 100m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 36, FoodUnitId = 1, GramsPerUnit = 100}}  },
            new FoodItem { Id = 37, Name = "Havregryn", Calories = 369m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 37, FoodUnitId = 2, GramsPerUnit = 40}, new FoodHasUnit {FoodItemId = 37, FoodUnitId = 1, GramsPerUnit = 100}}  },
            new FoodItem { Id = 38, Name = "Egg", Calories = 100m,  Units = new List<FoodHasUnit> {new FoodHasUnit {FoodItemId = 38, FoodUnitId = 3, GramsPerUnit = 55}, new FoodHasUnit {FoodItemId = 38, FoodUnitId = 1, GramsPerUnit = 100}}  },
        };

      

    DbContextOptions< CalCalcContext > options = new DbContextOptionsBuilder< CalCalcContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        var _db = new CalCalcContext(options);
        foreach (var u in units)
        {
            _db.Add(u);
        }
        foreach (var d in foodItems)
        {
            _db.Add(d);
        }
        _db.SaveChanges();
        return _db;
    }

    public async ValueTask DisposeAsync()
    {
        await _db.DisposeAsync();
    }
}