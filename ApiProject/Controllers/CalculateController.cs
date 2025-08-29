using System.Text.RegularExpressions;
using calcalc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using NuGet.Versioning;

namespace calcalc.Controllers;

public class CalculateController:Controller
{
   private readonly CalCalcContext _dbContext;
   private readonly ILogger<CalculateController> _log;

   public CalculateController(CalCalcContext dbContext, ILogger<CalculateController> log)
   {
      _dbContext = dbContext;
      _log = log;
   }
   
   [HttpPost]
   public async Task<ActionResult<CalculateCaloriesResult>>  CalculateCalories([FromBody] CalculationItems items)
   {
       if (!ModelState.IsValid)
       {
           return BadRequest(ModelState);
       }
       var unitsInDatabase = await _dbContext.FoodUnit.Include(u => u.Synonyms).ToListAsync();
       var unitsWithSynonymsInDatabaseNames = new List<string>();
       foreach (var unit in unitsInDatabase)
       {
          unitsWithSynonymsInDatabaseNames.Add(unit.Name);
          foreach (var synonym in unit.Synonyms)
          {
              unitsWithSynonymsInDatabaseNames.Add(synonym.Name);
          }
       }
       
       var ingredients = Ingredients.ParseIngredients(items.Items, unitsWithSynonymsInDatabaseNames  );
       if (ingredients.ErrorCode != null)
       { 
          return new CalculateCaloriesResult {ErrorCode = ingredients.ErrorCode, ErrorMessage = ingredients.ErrorMessage};
       }
       
       var foodItemsInDatabase = _dbContext.FoodItem.Include(fi => fi.Synonyms).Include(fi => fi.Units);
       // find corresponding items
       var foundFoodItems = new List<FoodItem>();

       bool namesMatches(string dbName, string searchName)
       {
           return Regex.Replace(dbName.ToLower(), ", .*$", "").Equals(searchName.ToLower()) ||
                  Regex.Replace(dbName.ToLower(), "$.*, ", "").Equals(searchName.ToLower()) ||
                  Regex.Replace(dbName.ToLower(), "^([^,]+), ([^,]+)$", "$2 $1").Equals(searchName.ToLower());
       }

       int idxToFind = -1;
       foreach(var toFind in ingredients.Ingredients)
       {
           // first with synonyms, they work as kind of an override
           idxToFind++; 
           bool foundFoodItem = false;
           foreach (var synonym in _dbContext.FoodSynonym.ToList())
           {
               if (namesMatches(synonym.Name, toFind.Name))
               {
                   foundFoodItems.Add( foodItemsInDatabase.First(fi => fi.Id == synonym.FoodItemId) );
                   foundFoodItem = true;
                   break;
               }
           }

           if (foundFoodItem)
           {
               continue;
           }
           foreach (var inDatabase in foodItemsInDatabase)
           {
               if (namesMatches(inDatabase.Name, toFind.Name))
               {
                   foundFoodItems.Add(inDatabase);
                   foundFoodItem = true;
                   break;
               }
           }

           if (!foundFoodItem)
           {
               return new CalculateCaloriesResult { ErrorCode = IngredientsError.INGREDIENT_NOT_FOUND, ErrorMessage = $"Linje {idxToFind + 1}: Fant ikke '{toFind.Name}' "};
           }
       }

       double calories = 0d;
       
       for (var idx = 0; idx < ingredients.Ingredients.Count; idx++)
       {
           var foodItem = foundFoodItems[idx];
           var ingredient = ingredients.Ingredients[idx];
            
           FoodHasUnit foundUnit = null;
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
                   ErrorCode =  IngredientsError.UNIT_NOT_FOUND,
                   ErrorMessage = $"Linje {idx+ 1}: Enheten {ingredient.Unit} for matvaren {ingredient.Name} er ikke støttet. Forsøk med en annen enhet."
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
           calories += (double)(foodItem.Calories * (grams / 100m));

       }
       
       return new CalculateCaloriesResult()
       {
           IngredientsFullNames = foundFoodItems.Select(x => x.Name).ToList(),
           Calories = calories
       }; 
   }
}

public enum Test
{
    A,B,C
}