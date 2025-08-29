using System.ComponentModel.DataAnnotations;

namespace calcalc.ViewModels;

public class FoodUnitEditViewmodel
{
    public string Name { get; set; }
    public int Id {get; set;}

    [Display(Name = "Synonymer, separert med komma")]
    public string? SynonymsCommaSeparated { get; set; }
    
    [Display(Name = "Antall desiliter(brukes hvis matvaren har ant kalorier pr desiliter registrert)")]
    public decimal? AmountDeciliters { get; set; }
}