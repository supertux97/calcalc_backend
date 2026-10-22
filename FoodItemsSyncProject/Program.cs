using System.Security.Authentication.ExtendedProtection;
using calcalc.Models;
using calcalc.Services;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;

public class FoodItemsSync
{

    public static IServiceProvider ConfigureServices(string[] args)
    {

        IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        services.AddDbContext<CalCalcContext>(options =>
        options.UseNpgsql(config["Database:ConnectionString"]));
        var logger = new LoggerConfiguration().ReadFrom.Configuration(context.Configuration).CreateLogger();
        services.AddSerilog(logger);

        services.AddHttpClient();
        services.AddScoped<IFooditemsService, MattilsynetAPIService>();
    })
    .Build();

        return host.Services;
    }

    public async static Task Main(string[] args)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        var services = ConfigureServices(args);
        var dbContext = services.GetRequiredService<CalCalcContext>();
        var logger = services.GetRequiredService<ILogger<FoodItemsSync>>();

        try
        {
            var foodItemsService = services.GetRequiredService<IFooditemsService>();
            await DoSync(dbContext, logger, foodItemsService);
            dbContext.Add(new FoodItemsSyncEvent { EventName = "RetrieveAndStoreItems", Successfull = true, Time = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            dbContext.Add(new FoodItemsSyncEvent { EventName = "RetrieveAndStoreItems", Successfull = false, Time = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
            logger.LogError("FoodItemsSync: Errored {ex}", ex);
        }
        finally
        {
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            logger.LogInformation($"FoodItemsSync: Took {elapsedMs / 1000} seconds");
        }
    }

    public static async Task DoSync(CalCalcContext dbContext, ILogger<FoodItemsSync> logger, IFooditemsService foodItemsService)
    {
        logger.LogInformation("FoodItemsSync: Start");
        logger.LogInformation("FoodItemsSync: Get food items from API");
        var result = await foodItemsService.GetFoodItems();
        if (result is null || result.Error != null)
        {
            logger.LogInformation("FoodItemsSync: Failed to get food items from API. Details: {@Error}", result.Error);
            return;
        }
        logger.LogInformation("FoodItemsSync: Got food items from API");

        var foodUnits = await dbContext.FoodUnit.Include(fu => fu.Synonyms).ToListAsync();

        var toAdd = new List<FoodItem>();

        var dbUnitGrams = await dbContext.FoodUnit.FirstOrDefaultAsync(u => u.Name == "gram");

        logger.LogInformation("FoodItemsSync: Adding food items to database");
        // first add the actual item
        int skippedItems = 0;
        var foodNamesInDb = await dbContext.FoodItem.AsNoTracking().Select(fi => fi.Name).ToListAsync();
        foreach (calcalc.Services.FoodItem apiItem in result.Foods)
        {
            // food item with this name already added
            if (foodNamesInDb.Any(fn => fn == apiItem.FoodName))
            {
                skippedItems++;
                continue;
            }

            toAdd.Add(new FoodItem
            {
                UserAdded = false,
                Name = apiItem.FoodName,
                NameWithoutDetails = apiItem.FoodName.Split(",").First(),
                InformationUrl = apiItem.Uri,
                Calories = apiItem.Calories.Quantity ?? 0m,
            });
            logger.LogInformation("FooditemsSync: Added new item {@item}", apiItem);
        }

        await dbContext.AddRangeAsync(toAdd);

        logger.LogInformation($"FoodItemsSync: Got {toAdd.Count} new items to add to database. Skipped {skippedItems} items.");
        await dbContext.SaveChangesAsync();
        logger.LogInformation($"FoodItemsSync: Saved new items to database");

        logger.LogInformation("FoodItemsSync: Adding units to food items to database");

        // then add the units for the items
        int unitsToAdd = 0;
        foreach (calcalc.Services.FoodItem apiItem in result.Foods)
        {
            var foodItem = await (from f in dbContext.FoodItem where f.Name == apiItem.FoodName select f).Include(f => f.Units).FirstOrDefaultAsync();
            if (foodItem is null)
            {
                continue;
            }

            // always add the grams unit
            if (dbUnitGrams != null && foodItem.Units.Find(u => u.FoodUnit.Name.ToLower() == "gram") == null)
            {
                foodItem.Units.Add(new FoodHasUnit
                {
                    FoodUnitId = dbUnitGrams.Id,
                    GramsPerUnit = 1,
                    FoodItemId = foodItem.Id,
                });
                unitsToAdd++;
            }

            foreach (var apiUnit in apiItem.Portions)
            {
                var dbUnit = foodUnits.Find(u => u.Name.ToLower() == apiUnit.PortionName.ToLower() || u.Synonyms.Find(s => s.Name.ToLower() == apiUnit.PortionName.ToLower()) != null);
                // if unit does not exist in the system
                if (dbUnit is null)
                {
                    continue;
                }



                if (apiUnit.PortionName == "desiliter")
                {
                    var foodUnitsThatConvertsToDl = foodUnits.Where(fu => fu.AmountDeciliters != null);
                    foreach (var foodUnit in foodUnitsThatConvertsToDl)
                    {
                        var existingDesiliterFoodUnit = foodItem.Units.Find(fu => fu.FoodUnit?.Id == foodUnit.Id);
                        if (existingDesiliterFoodUnit != null)
                        {
                            continue;
                        }
                        foodItem.Units.Add(new FoodHasUnit
                        {
                            FoodUnitId = foodUnit.Id,
                            GramsPerUnit = (decimal)foodUnit.AmountDeciliters! * apiUnit.Quantity,
                            FoodItemId = foodItem.Id,
                        });
                        unitsToAdd++;
                    }
                }
                else
                {
                    // if unit is already added for this food item we update it if needed
                    var existingFoodUnit = foodItem.Units.Find(u => u.FoodUnitId == dbUnit.Id);
                    if (existingFoodUnit != null)
                    {
                        /*
                        if (existingFoodUnit.GramsPerUnit != apiUnit.Quantity)
                        {
                            existingFoodUnit.GramsPerUnit = apiUnit.Quantity;
                        } */
                        continue;
                    }

                    foodItem.Units.Add(new FoodHasUnit
                    {
                        FoodUnitId = dbUnit.Id,
                        GramsPerUnit = apiUnit.Quantity,
                        FoodItemId = foodItem.Id,
                    });
                    unitsToAdd++;
                }

            }


        }
        logger.LogInformation($"FoodItemsSync: Got {unitsToAdd} units to add to database");

        logger.LogInformation("FoodItemsSync: Ready to save changes to database");
        await dbContext.SaveChangesAsync();
        logger.LogInformation($"FoodItemsSync: Done saving changes to database");

        logger.LogInformation("FoodItemsSync: End");
    }
}