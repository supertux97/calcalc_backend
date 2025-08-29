using calcalc.Models;
using calcalc.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace calcalc.BackgroundJobs;

public class MattilsynetSync
{
    IServiceProvider _serviceProvider;
    private readonly ILogger<MattilsynetSync> _logger;

    public MattilsynetSync(IServiceProvider serviceProvider, ILogger<MattilsynetSync> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)] // Adjust timeout as needed
    public async Task DoSync()
    {
        _logger.LogInformation("MattilsynetSync: Start");
        using (IServiceScope scope = _serviceProvider.CreateScope())
        using (CalCalcContext db_ctx = scope.ServiceProvider.GetRequiredService<CalCalcContext>())
        await using (IMattilsynetAPIService mattilsynetApiService =
                     scope.ServiceProvider.GetRequiredService<IMattilsynetAPIService>())
        {
            _logger.LogInformation("MattilsynetSync: Get food items from API");
            var result = await mattilsynetApiService.GetFoodItems();
            if (result is null)
            {
                _logger.LogInformation("MattilsynetSync: Failed to get food items from API");
                return;
            }
            _logger.LogInformation("MattilsynetSync: Got food items from API");

            var foodUnits = await (from fu in db_ctx.FoodUnit select fu).ToListAsync();

            var toAdd = new List<FoodItem>();

            var dbUnitGrams = await db_ctx.FoodUnit.FirstOrDefaultAsync(u => u.Name == "gram");

            _logger.LogInformation("MattilsynetSync: Adding food items to database");
            // first add the actual item
            /*
            foreach (MattilsynetAPIFoodItem apiItem in result.Foods)
            {
                // food item with this name already added
                if ((from f in db_ctx.FoodItem where f.Name == apiItem.FoodName select f).Any())
                {
                    Console.WriteLine("Skipped");
                    continue;
                }
                // unknown units are skipped

                toAdd.Add(new FoodItem
                {
                    UserAdded = false, Name = apiItem.FoodName, Calories = apiItem.Calories.Quantity ?? 0m,
                });
                Console.WriteLine("Added");
                
            } */

           // await db_ctx.AddRangeAsync(toAdd);
            //await db_ctx.SaveChangesAsync();
            
            
            _logger.LogInformation($"MattilsynetSync: Done adding food items to database, added {toAdd.Count}");
        
            _logger.LogInformation("MattilsynetSync: Adding units to food items to database");
            // then add the units for the items
            var addedUnits = 0;
            foreach (MattilsynetAPIFoodItem apiItem in result.Foods)
            {
                var foodItem = await (from f in db_ctx.FoodItem where f.Name == apiItem.FoodName select f).Include(f => f.Units).FirstOrDefaultAsync();
                if (foodItem is null)
                {
                    continue;
                }

                foreach (var apiUnit in apiItem.Portions)
                {
                    var dbUnit = foodUnits.Find(u => u.Name == apiUnit.PortionName);
                    // if unit does not exist in the system
                    if (dbUnit is null)
                    {
                        continue;
                    }

                    // if unit is already added for this food item
                    if (foodItem.Units.Any(u => u.FoodUnitId == dbUnit.Id))
                    {
                        continue;
                    }

                    if (apiUnit.PortionName == "desiliter")
                    {
                        var foodUnitsThatConvertsToDl = foodUnits.Where(fu => fu.AmountDeciliters != null);
                        foreach (var foodUnit in foodUnitsThatConvertsToDl)
                        {
                            foodItem.Units.Add(new FoodHasUnit
                            {
                                FoodUnitId = foodUnit.Id,
                                GramsPerUnit = (decimal)foodUnit.AmountDeciliters! * apiUnit.Quantity,
                                FoodItemId = foodItem.Id,
                            });
                            addedUnits++; 
                        }
                    }

                    foodItem.Units.Add(new FoodHasUnit
                    {
                        FoodUnitId = dbUnit.Id,
                        GramsPerUnit = apiUnit.Quantity,
                        FoodItemId = foodItem.Id,
                    });
                    addedUnits++;
                    Console.WriteLine("Added unit!");
                }
                // always add the grams unit
                foodItem.Units.Add(new FoodHasUnit
                {
                    FoodUnitId = dbUnitGrams.Id,
                    GramsPerUnit = 1,
                    FoodItemId = foodItem.Id,
                });
                addedUnits++;

                await db_ctx.SaveChangesAsync();
            }
            
            _logger.LogInformation($"MattilsynetSync: Done adding units to food items to database, added {addedUnits} units");
        }
        
        _logger.LogInformation("MattilsynetSync: End");
    }
}