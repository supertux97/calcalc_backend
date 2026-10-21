using System.Text.RegularExpressions;

public class IngredientLanguageSpecificParserStrategyNorwegian : IIngredientLanguageSpecificParserStrategy
{
    public List<string> GetFillerWords()
    {
        return new List<string> { "ca", "ca.", "til pensling", "til pynt", "til steking", "til servering", "til dekorering", "til anretning" };
    }

    public List<string> GetPreparationWords()
    {
        return new List<string>
            {"malt","malte","hel","hele","i biter","rompemperert","romtempererte",
                "revet", "revede", "strimlet", "strimlede", "ternet", "i terninger", "oppskårne", "oppskåret",
                "finhakket", "finhakkede", "grovhakket", "grovhakkede", "skivet", "skivede","pisket","piskede","stekt","stekte","oppkuttet","oppkuttede","vasket","vaskede","kuttet i staver","kuttet i ringer",
                "rørt","rørte","lett pisket","lett piskede","sammenrørt","sammenrørte","delt i to","delt i tre","delt i båter","i båter","i halve","delt i halve", "most","moste","banket","bankede",
                "varmet","varmede","lunkne","romtempererte","i romtemperatur","avkjølt","avkjølte","frossen","frossne","hakket","hakkede","kuttet","kuttede"
            };
    }

    public bool GetLineHasOptionalIngredient(string line)
    {
        return line.Contains(" kan sløyfes") || line.Contains("valgfritt") || Regex.IsMatch(line, "^evt. ") || Regex.IsMatch(line, "^evt ") || line.Contains(" evt ") || line.Contains(" eventuelt ");
    }

    public bool GetLineHasAlternativesIngredients(string line)
    {
        return line.Contains(" eller ");
    }

    public bool GetLineHasInacurateUnits(string line)
    {
        var ambigiousWords = new List<string> { "noen", "mye", "passe", "litt", "rikelig", "tilstrekelig", "en del", "et par", "en god del", "massevis", "en neve", "en håndfull", "en god håndfull", "en god neve", "så mye du vil", "nok" };
        return ambigiousWords.Any(w => line.Contains(w));
    }
}