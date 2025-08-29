using System.ComponentModel.DataAnnotations;
using calcalc.Models;
using calcalc.ViewModels;
using Microsoft.IdentityModel.Tokens;
using MvcMovie.Data;

namespace calcalc.Validators;

public class FoodSynonymIsUniqueAcrossFoodItems : ValidationAttribute
{

    protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
    {
        var dbContext = validationContext.GetService<CalCalcContext>();
        var thisModel = validationContext.ObjectInstance as FoodItemEditViewmodel;
        if ((value as string).IsNullOrEmpty())
        {
            return ValidationResult.Success;
        }
        var thisSynonyms = (value as string).Split(",");

        var existingSynonyms = dbContext.FoodSynonym.Where(fs => fs.FoodItemId != thisModel.Id).ToList();
        foreach (var existingSynonym in existingSynonyms)
        {
            foreach (var synonymThisModel in thisSynonyms)
            {
                if (existingSynonym.Name == synonymThisModel)
                {
                    var otherFoodItemThatAlsoHasSynonym = dbContext.FoodItem.First(fi => fi.Id == existingSynonym.FoodItemId);
                    return new ValidationResult(
                        $"The synonym {synonymThisModel} already exists in item with id {otherFoodItemThatAlsoHasSynonym.Id} and name {otherFoodItemThatAlsoHasSynonym.Name}");
                }
            }
        }

        return ValidationResult.Success;
    }
}