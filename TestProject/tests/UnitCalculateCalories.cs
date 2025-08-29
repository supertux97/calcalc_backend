using calcalc.Controllers;
using calcalc.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace tests;

public class UnitCalculateCalories
{
        private readonly ILogger<CalculateController> _log;
        public UnitCalculateCalories()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .BuildServiceProvider();

            _log = serviceProvider.GetService<ILogger<CalculateController>>();
        }

        [Fact]
        public async Task TestCalculateCalories()
        {
            
            var controller = new CalculateController(MockData.getFakeDbContext(), _log);
            var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 gram havregryn","1,5 desiliter havregryn"} } );
            // multiply so we comnvert to grams first, not directly use desoliter
            // this prevents rounding errors
            Assert.Equivalent( (5 * 369) + (0.4 * 1.5 *369)  , result1.Value.Calories);
        }
        
        [Fact]
        public async Task TestCalculateCaloriesSynonymsUnits()
        {
            
            var controller = new CalculateController(MockData.getFakeDbContext(), _log);
            // also test units that ends with a dot, eg dl.
            var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"500 g havregryn","1,5 dl. havregryn", "2.5 dl havregryn"} } );
            Assert.Equivalent( (5 * 369) + (0.4 * 2.5 *369) + (0.4 * 1.5 *369)  , result1.Value.Calories);
        }
        
        [Fact]
        public async Task TestCalculateCaloriesSynonymsFoods()
        {
            
            var controller = new CalculateController(MockData.getFakeDbContext(), _log);
            // testing different units, synonym units (dl is syunony, to desiliter) as well as both "." and "," for decimals
            // also test units that ends with a dot, eg dl.
            var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"5 desiliter melk","5 desiliter vanlig melk ", "2.5 desiliter melk a"} } );
            // multiply so we comnvert to grams first, not directly use desoliter
            // this prevents rounding errors
            Assert.Equivalent( (5 * 100) + (5 *100) + (2.5 *100)  , result1.Value.Calories);
        }
        
        // should implicitly converted to "stk"
        [Fact]
        public async Task TestCalculateCaloriesNoUnit()
        {
            var controller = new CalculateController(MockData.getFakeDbContext(), _log);
            var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"5 egg"} } );
            Assert.Equivalent( (5 * 0.55 * 100), result1.Value.Calories);
        }
        
        // should implicitly converted to "stk"
        [Fact]
        public async Task TestCalculateCaloriesNoSpaceAfterUnit()
        {
            var controller = new CalculateController(MockData.getFakeDbContext(), _log);
            var result1 = await controller.CalculateCalories(new CalculationItems {Items = new List<string> {"10.5gram egg"} } );
            Assert.Equivalent( (0.105 * 100), result1.Value.Calories);
        }
        
}