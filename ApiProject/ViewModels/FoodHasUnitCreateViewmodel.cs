using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using calcalc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace calcalc.ViewModels;

public class FoodHasUnitCreateViewmodel
{
    public int UnitIdx { get; set; }
    public List<SelectListItem> FoodUnitsListItems { get; set; }
    public FoodHasUnitCreateFoodItem FoodItem { get; set; }
    public string ChooseUnitUIStr = "Velg enhet";
}

public class FoodHasUnitCreateFoodHasUnit
{
    [DisplayName("Gram per enhet")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Du må velge en")] 
    public decimal GramsPerUnit { get; set; }
    
    [Required(AllowEmptyStrings = false, ErrorMessage = "Du må velge en")]
    public int FoodItemId { get; set; }
    public int FoodUnitId { get; set; }
}



public class FoodHasUnitCreateFoodItem
{
    public List<FoodHasUnitCreateFoodHasUnit> Units { get; set; } = new List<FoodHasUnitCreateFoodHasUnit>();
}