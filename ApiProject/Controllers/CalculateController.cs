using System.Text.RegularExpressions;
using calcalc.Models;
using calcalc.Repositories;
using Humanizer;
using Microsoft.AspNetCore.Mvc;

namespace calcalc.Controllers;

public class CalculateController : Controller
{
    private readonly ILogger<CalculateController> _log;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodUnitRepository _foodUnitRepository;
    private readonly IFoodSynonymRepository _foodSynonymRepository;
    private readonly IIngredientLanguageSpecificParserStrategy _languageSpecificParserStrategy;

    public CalculateController(IFoodItemRepository foodItemRepository, IFoodUnitRepository foodUnitRepository, IFoodSynonymRepository foodSynonymRepository, ILogger<CalculateController> log, IIngredientLanguageSpecificParserStrategy ingredientLanguageSpecificParserStrategy)
    {
        _foodItemRepository = foodItemRepository;
        _foodUnitRepository = foodUnitRepository;
        _foodSynonymRepository = foodSynonymRepository;
        _log = log;
        _languageSpecificParserStrategy = ingredientLanguageSpecificParserStrategy;
    }

    [HttpPost]
    public async Task<ActionResult<CalculateCaloriesResult>> CalculateCalories([FromBody] CalculationItems items)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var unitsInDatabase = await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: true);
        var unitsAndSynonymsInDatabaseNames = new List<string>();
        foreach (var unit in unitsInDatabase)
        {
            unitsAndSynonymsInDatabaseNames.Add(unit.Name);
            foreach (var synonym in unit.Synonyms)
            {
                unitsAndSynonymsInDatabaseNames.Add(synonym.Name);
            }
        }

        var ingredients = Ingredients.ParseIngredients(_languageSpecificParserStrategy, items.Items, unitsAndSynonymsInDatabaseNames);
        if (ingredients.ErrorCode != null)
        {
            return new CalculateCaloriesResult { ErrorCode = ingredients.ErrorCode, ErrorMessage = ingredients.ErrorMessage };
        }

        var foodItemsInDatabase = await _foodItemRepository.GetAllFoodItemsAsync(includeSynonyms: true, includeUnits: true);
        var synonymsInDatabase = await _foodSynonymRepository.GetAllSynonymsItemsAsync();

        // ====
        // match ingredient to food item
        // ====
        var foundFoodItemForIngredientLines = new List<FoodItem>();

        // cleans out unneeded information
        bool namesMatches(string dbName, string searchName)
        {
            return Regex.Replace(dbName.ToLower(), ", .*$", "").Equals(searchName.ToLower()) ||
                   Regex.Replace(dbName.ToLower(), "$.*, ", "").Equals(searchName.ToLower()) ||
                   Regex.Replace(dbName.ToLower(), "^([^,]+), ([^,]+)$", "$2 $1").Equals(searchName.ToLower());
        }

        var ingredientInformation = new List<IngredientInfo>();

        int idxToFind = -1;
        foreach (var toFind in ingredients.Ingredients)
        {
            // first check with synonyms, they have priority over normal name
            idxToFind++;
            bool foundFoodItem = false;
            foreach (var synonym in synonymsInDatabase)
            {
                if (namesMatches(synonym.Name, toFind.Name))
                {
                    foundFoodItemForIngredientLines.Add(foodItemsInDatabase.First(fi => fi.Id == synonym.FoodItemId));
                    foundFoodItem = true;
                    break;
                }
            }

            if (foundFoodItem)
            {
                continue;
            }

            // then check with normal name
            foreach (var inDatabase in foodItemsInDatabase)
            {
                if (namesMatches(inDatabase.Name, toFind.Name))
                {
                    foundFoodItemForIngredientLines.Add(inDatabase);
                    foundFoodItem = true;
                    break;
                }
            }

            if (!foundFoodItem)
            {
                return new CalculateCaloriesResult { ErrorCode = IngredientsError.INGREDIENT_NOT_FOUND, ErrorMessage = $"Linje {idxToFind + 1}: Fant ikke '{toFind.Name}' " };
            }
        }

        double caloriesTotal = 0d;

        for (var idx = 0; idx < ingredients.Ingredients.Count; idx++)
        {
            var foodItem = foundFoodItemForIngredientLines[idx];
            var ingredient = ingredients.Ingredients[idx];

            // find the unit for the ingredient line
            FoodHasUnit? foundUnit = null;
            foreach (var foodHasUnit in foodItem.Units)
            {
                if (foundUnit is not null)
                {
                    break;
                }
                var unit = unitsInDatabase.First(u => u.Id == foodHasUnit.FoodUnitId);
                var unitNames = unit.Synonyms.Select(u => u.Name).Append(unit.Name);
                foreach (var unitName in unitNames)
                {
                    if (unitName.ToLower().Equals(ingredient.Unit.ToLower()))
                    {
                        foundUnit = foodHasUnit;
                        break;
                    }
                }
            }

            if (foundUnit is null)
            {
                return new CalculateCaloriesResult()
                {
                    ErrorCode = IngredientsError.UNIT_NOT_FOUND,
                    ErrorMessage = $"Ingredient {idx + 1}: Enheten {ingredient.Unit} for matvaren {ingredient.Name} er ikke støttet. Forsøk med en annen enhet."
                };
            }

            decimal grams = 0;
            if (foundUnit.FoodUnit.Name.ToLower().Equals("gram"))
            {
                grams = ingredient.Amount;
            }
            else
            {
                grams = (ingredient.Amount * foundUnit.GramsPerUnit);
            }
            double calories = (double)(foodItem.Calories * (grams / 100m));
            caloriesTotal += calories;

            // add total calories after when we have gone trough all items
            ingredientInformation.Add(new IngredientInfo
            {
                IngredientName = ingredient.Name,
                Unit = ingredient.Unit,
                Amount = ingredient.Amount,
                Calories = calories,
                ReadMoreUrl = foodItem.InformationUrl,
            });

        }

        foreach (var ingredientInfo in ingredientInformation)
        {
            ingredientInfo.PercentageOfCalories = (ingredientInfo.Calories / caloriesTotal) * 100d;
        }

        return new CalculateCaloriesResult()
        {
            IngredientsFullNames = foundFoodItemForIngredientLines.Select(x => x.Name).ToList(),
            IngredientsInformation = ingredientInformation,
            Calories = caloriesTotal
        };
    }
}
