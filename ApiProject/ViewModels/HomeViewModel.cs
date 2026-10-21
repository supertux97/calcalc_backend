using calcalc.Models;

namespace calcalc.ViewModels;

public class HomeViewModel
{
    public int CntFoodItemsTotal { get; set; }
    public int CntFoodItemsExternal { get; set; }
    public int CntFoodItemsUserAdded { get; set; }
    public List<FoodItem> FoodItemsUserLatestUpdated { get; set; }
    public List<FoodItem> FoodItemsUserLatestCreated { get; set; }
    public FoodItemsSyncEvent? LatestSyncEvent { get; set; }
    public FoodItemsSyncEvent? LatestSuccessfullSyncEvent { get; set; }
}