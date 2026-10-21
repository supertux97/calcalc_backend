public interface IIngredientLanguageSpecificParserStrategy
{
    public List<string> GetFillerWords();
    public List<string> GetPreparationWords();
    public bool GetLineHasOptionalIngredient(string line);
    public bool GetLineHasAlternativesIngredients(string line);
    public bool GetLineHasInacurateUnits(string line);
}