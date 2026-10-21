using System.ComponentModel.DataAnnotations;

namespace calcalc.ViewModels;

public class FoodUnitEditViewmodel
{
    public string Name { get; set; }
    public int Id { get; set; }

    [Display(Name = "Synonyms, comma separated")]
    public string? SynonymsCommaSeparated { get; set; }

    [Display(Name = "Number of deciliters, used if the unit is for fluids")]
    public decimal? AmountDeciliters { get; set; }
}