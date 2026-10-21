using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using calcalc.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Name), IsUnique = true)]
[Index(nameof(NameWithoutDetails), IsUnique = false)]
public class FoodItem
{

    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }

    // This is for example Egg, boiled
    public int Id { get; set; }
    [Required]
    [MaxLength(250)]
    public string Name { get; set; }

    public string? InformationUrl { get; set; }

    // This is for example Egg
    [MaxLength(250)]
    public string? NameWithoutDetails { get; set; }

    [DisplayName("Calories pr 100g")]
    public decimal Calories { get; set; }

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