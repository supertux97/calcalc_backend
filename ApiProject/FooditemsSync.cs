using calcalc.Models;
using calcalc.Services;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

public class FoodItemsSync
{

    public static ServiceCollection ConfigureServices(string[] args)
    {
        var services = new ServiceCollection();

        var builder = WebApplication.CreateBuilder(args);
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>().Build();

        services.AddDbContext<CalCalcContext>(options =>
        options.UseNpgsql(config["Database:ConnectionString"]));

        var logger = new LoggerConfiguration().WriteTo.File("logs/fodo_items.log", rollingInterval: RollingInterval.Day).WriteTo.Console(theme: AnsiConsoleTheme.Code).CreateLogger();
        builder.Logging.AddSerilog(logger);
        return services;
    }

    public async static void Main(string[] args)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        var services = ConfigureServices(args);
        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetRequiredService<CalCalcContext>();
        try
        {

            var logger = provider.GetRequiredService<ILogger<FoodItemsSync>>();
            var mattilsynetApiService = provider.GetRequiredService<MattilsynetAPIService>();

            await DoSync(dbContext, logger, mattilsynetApiService);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            logger.LogInformation($"FoodItemsSync: Took {elapsedMs / 1000} seconds");
            dbContext.Add(new FoodItemsSyncEvent { EventName = "RetrieveAndStoreItems", Successfull = true, Time = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
        }
        catch
        {
            dbContext.Add(new FoodItemsSyncEvent { EventName = "RetrieveAndStoreItems", Successfull = false, Time = DateTime.UtcNow });
            await dbContext.SaveChangesAsync();
        }
    }

    public static async Task DoSync(CalCalcContext dbContext, ILogger<FoodItemsSync> logger, IFooditemsService mattilsynetAPIService)
    {
        logger.LogInformation("FoodItemsSync: Start");
        logger.LogInformation("FoodItemsSync: Get food items from API");
        var result = await mattilsynetAPIService.GetFoodItems();
        if (result is null || result.Error != null)
        {
            logger.LogInformation("FoodItemsSync: Failed to get food items from API. Details: {@Error}", result.Error);
            return;
        }
        logger.LogInformation("FoodItemsSync: Got food items from API");

        var foodUnits = await (from fu in dbContext.FoodUnit select fu).ToListAsync();

        var toAdd = new List<FoodItem>();

        var dbUnitGrams = await dbContext.FoodUnit.FirstOrDefaultAsync(u => u.Name == "gram");

        logger.LogInformation("FoodItemsSync: Adding food items to database");
        // first add the actual item
        int skippedItems = 0;
        foreach (calcalc.Services.FoodItem apiItem in result.Foods)
        {
            // food item with this name already added
            if ((from f in dbContext.FoodItem where f.Name == apiItem.FoodName select f).Any())
            {
                skippedItems++;
                continue;
            }

            toAdd.Add(new FoodItem
            {
                UserAdded = false,
                Name = apiItem.FoodName,
                Calories = apiItem.Calories.Quantity ?? 0m,
                Created = DateTime.UtcNow,
            });
            logger.LogInformation("FooditemsSync: Added new item {@item}", apiItem);
        }

        await dbContext.AddRangeAsync(toAdd);

        logger.LogInformation($"FoodItemsSync: Got {toAdd.Count} new items to add to database. Skipped {skippedItems} items.");

        logger.LogInformation("FoodItemsSync: Adding units to food items to database");

        // then add the units for the items
        int addedUnits = 0;
        foreach (calcalc.Services.FoodItem apiItem in result.Foods)
        {
            var foodItem = await (from f in dbContext.FoodItem where f.Name == apiItem.FoodName select f).Include(f => f.Units).FirstOrDefaultAsync();
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
            }
            // always add the grams unit
            foodItem.Units.Add(new FoodHasUnit
            {
                FoodUnitId = dbUnitGrams.Id,
                GramsPerUnit = 1,
                FoodItemId = foodItem.Id,
            });
            addedUnits++;

        }

        logger.LogInformation("FoodItemsSync: Ready to save changes to database");
        await dbContext.SaveChangesAsync();
        logger.LogInformation($"FoodItemsSync: Done saving changes to database");

        logger.LogInformation("FoodItemsSync: End");
    }
}