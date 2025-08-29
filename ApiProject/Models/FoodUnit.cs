using Microsoft.AspNetCore.Mvc.Rendering;

namespace calcalc.Models;

public class FoodUnit
{
   public int Id { get; set; } 
   public string Name { get; set; }
   public List<FoodUnitSynonym> Synonyms { get; set; } = new List<FoodUnitSynonym>();
   // if set, will be used to convert to deciliters for food items that have that
   public decimal? AmountDeciliters { get; set; }

   public string GetSynonymsNames()
   {
      return String.Join(",", Synonyms.Select(synonym => synonym.Name));
   }
}