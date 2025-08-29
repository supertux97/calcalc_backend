using System.ComponentModel.DataAnnotations;
using calcalc.Models;
using calcalc.Validators;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Name), IsUnique = true)]
public class FoodItem
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    public decimal Calories { get; set; }
    [FoodSynonymIsUniqueAcrossFoodItems]
    public List<FoodSynonym> Synonyms { get; set; } = new List<FoodSynonym>();
    public List<FoodHasUnit> Units { get; set; } = new List<FoodHasUnit>();
    public bool UserAdded { get; set; } = true;

    public string GetSynonymsNamesList()
    {
        return String.Join(",", this.Synonyms.Select(s => s.Name));
    }

    public string GetFoodUnitNamesList()
    {
        return String.Join(",", this.Units.Select(u => u.FoodUnit.Name));
    }
}