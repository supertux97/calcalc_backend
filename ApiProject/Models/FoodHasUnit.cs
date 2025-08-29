using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace calcalc.Models;

[PrimaryKey("FoodItemId", "FoodUnitId")]
public class FoodHasUnit
{
    
    [ForeignKey("FoodUnit")]
    public int FoodUnitId { get; set; }

    public FoodUnit? FoodUnit { get; set; } = null;
    
    public int FoodItemId { get; set; }
    
    public decimal GramsPerUnit { get; set; }
    
}

