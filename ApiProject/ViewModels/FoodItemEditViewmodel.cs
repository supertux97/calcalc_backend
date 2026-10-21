using System.ComponentModel.DataAnnotations;
using calcalc.Models;
using calcalc.Repositories;

namespace calcalc.ViewModels;

public class FoodItemEditViewmodel
{
    public string Name { get; set; }
    public int Id { get; set; }
    [Display(Name = "Calories pr 100 g")]
    public decimal Calories { get; set; }
    [Display(Name = "Synonyms, comma-separated")]
    public string? SynonymsCommaseparated { get; set; }
    public bool UserAdded { get; set; }

    public string? ReturnUrl { get; set; }
    public FoodItemUpdateResult? UpdateResult { get; set; }

    public List<FoodSynonym> Synonyms { get; set; } = new List<FoodSynonym>();
    public List<FoodHasUnit> Units { get; set; } = new List<FoodHasUnit>();
}