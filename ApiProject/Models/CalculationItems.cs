using System.ComponentModel.DataAnnotations;

namespace calcalc.Models;

public class CalculationItems
{
    [Required, MinLength(1)] public List<string> Items { get; set; } 
}