using calcalc.Models;

namespace calcalc.ViewModels;

public class FoodManageUnitsViewModel
{
    public FoodItem FoodItem { get; set; }
    public int unitIdToDelete { get; set; }
    public List<FoodUnit> Units { get; set; }
}