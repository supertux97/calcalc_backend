using System.ComponentModel.DataAnnotations;
using calcalc.Validators;

namespace calcalc.ViewModels;

public class FoodItemEditViewmodel
{
    public string Name { get; set; }
    public int Id { get; set; }
    public decimal Calories { get; set; }
    [Display(Name = "Synonymer, adskill med komma")] 
    [FoodSynonymIsUniqueAcrossFoodItems]
    public string? SynonymsCommaseparated { get; set; }
}