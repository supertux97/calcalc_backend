using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json.Converters;

namespace calcalc.Models;



public class IngredientsParseResult
{
    public string ErrorMessage { get; set; }
    public List<Ingredient> Ingredients { get; set; }
    public IngredientsError? ErrorCode { get; set; } = null;
}

public class Ingredients
{
    public static IngredientsParseResult ParseIngredients(List<string> ingredientsLines, List<string> availibleFoodUnitsNames) {
        var result = new List<Ingredient>();
        var idx = 0;
        foreach (var ingredientLine in ingredientsLines)
        {
            string createErrorForCurrLine(string msg)
            {
                return $"Linje {idx + 1}('{ingredientLine}') {msg}";
            }
            var lineCleaned = Regex.Replace(Regex.Replace(ingredientLine.Trim(), "  ", " "), "^([0-9]+)\\.([0-9])+","$1,$2" ).ToLower();
            // replace 100g with 100 g
            // also same with 10.5g
            lineCleaned = Regex.Replace(lineCleaned, "^([0-9,]+)([^0-9]+) ", "$1 $2 ");

            
            var fillerwords = new List<string> { "til pensling", "til pynt", "til steking", "til servering","til dekorering","til anretning" };
            fillerwords.ForEach(fw => lineCleaned = lineCleaned.Replace($", {fw}","").Replace(fw, ""));
            
            // are to be removed
            var preparationWords = new List<string>
            {"malt","malte","hel","hele","i biter","rompemperert","romtempererte",
                "revet", "revede", "strimlet", "strimlede", "ternet", "i terninger", "oppskårne", "oppskåret",
                "finhakket", "finhakkede", "grovhakket", "grovhakkede", "skivet", "skivede","pisket","piskede","stekt","stekte","oppkuttet","oppkuttede","vasket","vaskede","kuttet i staver","kuttet i ringer",
                "rørt","rørte","lett pisket","lett piskede","sammenrørt","sammenrørte","delt i to","delt i tre","delt i båter","i båter","i halve","delt i halve", "most","moste","banket","bankede",
                "varmet","varmede","lunkne","romtempererte","i romtemperatur","avkjølt","avkjølte","frossen","frossne","hakket","hakkede","kuttet","kuttede"
            };
            preparationWords.ForEach(pw => lineCleaned = lineCleaned.Replace($", {pw}","").Replace(pw, ""));
            
            var lineHasOptionalIngredient =
                lineCleaned.Contains("kan sløyfes") || lineCleaned.Contains("valgfritt");
            if (lineHasOptionalIngredient)
            {
                return new IngredientsParseResult
                {
                    ErrorCode =  IngredientsError.OPTIONAL,
                    ErrorMessage =
                        createErrorForCurrLine("har valgfrie ingredienser. Enten fjern denne linjen eller" +
                                               "fjern tekst om valgfrihet og prøv på nytt")
                };
            }
            
            // remove parenteces and stuff within
            lineCleaned = Regex.Replace(lineCleaned, "\\(.*\\)$", "").Trim();
            
            if (lineCleaned.Contains(" eller "))
            {
                return new IngredientsParseResult { ErrorCode = IngredientsError.OPTION,  ErrorMessage = createErrorForCurrLine( "inneholder flere matvarer man kan velge blant. " +
                                                                   "Fjern det ene valget og forsøk på nytt") };
            }

            var ambigiousWords = new List<string> {"noen","mye","passe","litt","rikelig","tilstrekelig", "en del", "et par","en god del","massevis","en neve","en håndfull","en god håndfull","en god neve", "så mye du vil","nok" };
            if (ambigiousWords.Any(w => lineCleaned.Contains(w)))
            {
                return new IngredientsParseResult
                    {  ErrorCode = IngredientsError.INACCURATE_UNIT, ErrorMessage = createErrorForCurrLine("inneholder et unøyaktig mål, feks 'en neve'. Endre til noe mer presist, feks '100g'") };
            }

            var lineHasIngredientAmountRange = Regex.IsMatch(lineCleaned, "[0-9]+\\s?-\\s?[0-9]+");
            if (lineHasIngredientAmountRange)
            {
                return new IngredientsParseResult { ErrorCode = IngredientsError.AMOUNT_RANGE,  ErrorMessage = createErrorForCurrLine("inneholder et antall ingrdienser som kan variere, feks 1-2 tomater." +
                                                                   $"Velg et antall (feks 2 tomater) og prøv på nytt")};
            }
            
            // if no unit is given, insert "stk"
            bool hasFoodUnit = availibleFoodUnitsNames.Any(fu => Regex.IsMatch(lineCleaned, $"^[0-9,]+\\s*{fu} "));
            if (!hasFoodUnit)
            {
                lineCleaned = Regex.Replace(lineCleaned, "^([0-9,]+\\s?)(.*)", "$1 stk $2");
            }

            lineCleaned = lineCleaned.Replace("  ", " ");

            var unicodeLetter = "\\p{L}";
            /*
             *format: AMOUNT UNIT INGREDIENT
             * AMOUNT: 1 | 1.5
             * UNIT: kg | kilo
             * INGREDIENT: egg | egg, lite | lite egg
             * first extract then validate
             *
             * en del egg
             */
            var parts = Regex.Match(lineCleaned, $"^([^ ]+)\\s+([^ ]+)\\s+(.+)$");
            if (!parts.Groups[1].Success || !parts.Groups[2].Success || !parts.Groups[3].Success)
            {
                return new IngredientsParseResult { ErrorCode  = IngredientsError.INVALID_FORMAT, ErrorMessage = createErrorForCurrLine("må ha formatet ANTALL ENHET INGREDIENS") };
            }

            decimal amount = 0m;
            if (!Decimal.TryParse(parts.Groups[1].Value, out amount))
            {
                return new IngredientsParseResult {  ErrorCode = IngredientsError.INVALID_FORMAT_AMOUNT,  ErrorMessage = createErrorForCurrLine(",kunne ikke lese antall. Linjen må ha formatet ANTALL ENHET INGREDIENS") };
            }
            Console.WriteLine("Antall ble!");
            Console.WriteLine(amount);
            
            var unit = parts.Groups[2].Value; 
            var name = parts.Groups[3].Value;
            
            if (
                !Regex.IsMatch(name, $"^[{unicodeLetter} ]+$" ) ||
                name.Split(" ").Length > 3) // more than tree likely means sentence not word
            {
                return new IngredientsParseResult
                {
                    ErrorCode = IngredientsError.INVALID_FORMAT_INGREDIENT,
                    ErrorMessage = createErrorForCurrLine("har feil format på ingrediensnavn. Gyldig er a) 1 kg mel b) 1 kg grovt mel c) 1 kg mel, grovt")
                };
            }
            idx++;
            result.Add(new Ingredient {Unit = unit, Amount = amount, Name = name});
        }

        return new IngredientsParseResult {Ingredients = result};
    }
}


public class Ingredient
{
    public decimal Amount { get; set; }
    public string Unit { get; set; }
    public string Name { get; set; }
}