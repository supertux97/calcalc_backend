using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace calcalc.ViewModels;

public class FoodItemIndexViewmodel
{
    public string FilterNameUiStr = "Filtrer på navn";
    public List<FoodItem> FoodItems { get; set; } = new List<FoodItem>();

    [DisplayName("Matching name")]
    public string NameFilterString { get; set; }

    public bool HasSynonymFilter { get; set; }
    public bool IsAddedByUserFilter { get; set; }

    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string? Notification { get; set; }
    public int PaginationPageNo { get; set; }
}
