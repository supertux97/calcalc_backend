namespace calcalc.Models;

public enum IngredientsError
{
    OPTION = 0, INACCURATE_UNIT = 1, OPTIONAL = 2,INVALID_FORMAT = 3,INVALID_FORMAT_AMOUNT = 4,AMOUNT_RANGE = 5, INVALID_FORMAT_INGREDIENT = 6,
    INGREDIENT_NOT_FOUND = 7, UNIT_NOT_FOUND = 8
}

public class CalculateCaloriesResult
{
    public string ErrorMessage { get; set; }
    public IngredientsError? ErrorCode { get; set; } = null;
    public List<string> IngredientsFullNames { get; set; } = new List<string>();
    public double Calories { get; set; }
}