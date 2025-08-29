using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace calcalc.ViewModels;

public class FoodItemIndexViewmodel
{
    public string FilterNameUiStr = "Filtrer på navn";
    public List<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    [DisplayName("Filtrer på navn")]
    public string NameFilterString { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string PrevStartAtName { get; set; }
    public string CurrPageStartAtName { get; set; }
    public string NextPageStartAtName { get; set; }
    public string? Notification { get; set; }
}
