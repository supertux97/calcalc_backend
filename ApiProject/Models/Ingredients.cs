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

    public static IngredientsParseResult ParseIngredients(IIngredientLanguageSpecificParserStrategy languageSpecificParser, List<string> ingredientsLines, List<string> availibleFoodUnitsNames)
    {
        var result = new List<Ingredient>();
        var idx = 0;
        foreach (var ingredientLine in ingredientsLines)
        {
            string createErrorForCurrLine(string msg)
            {
                return $"Linje {idx + 1}('{ingredientLine}') {msg}";
            }
            var lineCleaned = Regex.Replace(Regex.Replace(ingredientLine.Trim(), "  ", " "), "^([0-9]+)\\.([0-9])+", "$1,$2").ToLower();
            // replace 100g with 100 g
            // also same with 10.5g
            lineCleaned = Regex.Replace(lineCleaned, "^([0-9,]+)([^0-9]+) ", "$1 $2 ");


            var fillerwords = languageSpecificParser.GetFillerWords();
            fillerwords.ForEach(fw => lineCleaned = lineCleaned.Replace($", {fw}", "").Replace(fw + " ", ""));

            // are to be removed
            var preparationWords = languageSpecificParser.GetPreparationWords();
            preparationWords.ForEach(pw => lineCleaned = lineCleaned.Replace($", {pw}", "").Replace(pw, ""));

            if (languageSpecificParser.GetLineHasOptionalIngredient(lineCleaned))
            {
                return new IngredientsParseResult
                {
                    ErrorCode = IngredientsError.OPTIONAL,
                    ErrorMessage =
                        createErrorForCurrLine("har valgfrie ingredienser. Enten fjern denne linjen eller" +
                                               "fjern tekst om valgfrihet og prøv på nytt")
                };
            }

            // remove parenteces and stuff within
            lineCleaned = Regex.Replace(lineCleaned, "\\(.*\\)$", "").Trim();

            if (languageSpecificParser.GetLineHasAlternativesIngredients(lineCleaned))
            {
                return new IngredientsParseResult
                {
                    ErrorCode = IngredientsError.OPTION,
                    ErrorMessage = createErrorForCurrLine("inneholder flere matvarer man kan velge blant. " +
                                                                   "Fjern det ene valget og forsøk på nytt")
                };
            }

            if (languageSpecificParser.GetLineHasInacurateUnits(lineCleaned))
            {
                return new IngredientsParseResult
                { ErrorCode = IngredientsError.INACCURATE_UNIT, ErrorMessage = createErrorForCurrLine("inneholder et unøyaktig mål, feks 'en neve'. Endre til noe mer presist, feks '100g'") };
            }

            var lineHasIngredientAmountRange = Regex.IsMatch(lineCleaned, "[0-9]+\\s?-\\s?[0-9]+");
            if (lineHasIngredientAmountRange)
            {
                return new IngredientsParseResult
                {
                    ErrorCode = IngredientsError.AMOUNT_RANGE,
                    ErrorMessage = createErrorForCurrLine("inneholder et antall ingrdienser som kan variere, feks 1-2 tomater. " +
                                                                   $"Velg et antall (feks 2 tomater) og prøv på nytt")
                };
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
                return new IngredientsParseResult { ErrorCode = IngredientsError.INVALID_FORMAT, ErrorMessage = createErrorForCurrLine("må ha formatet [ANTALL] [ENHET] [INGREDIENS] feks 1 stk egg") };
            }

            decimal amount = 0m;
            if (!Decimal.TryParse(parts.Groups[1].Value, out amount))
            {
                return new IngredientsParseResult { ErrorCode = IngredientsError.INVALID_FORMAT_AMOUNT, ErrorMessage = createErrorForCurrLine(",kunne ikke lese antall. Linjen må ha formatet [ANTALL] [ENHET] [INGREDIENS] feks 1 stk egg") };
            }

            var unit = parts.Groups[2].Value;
            var name = parts.Groups[3].Value;

            if (
                !Regex.IsMatch(name, $"^[{unicodeLetter} ]+$") ||
                name.Split(" ").Length > 3) // more than tree likely means sentence not word
            {
                return new IngredientsParseResult
                {
                    ErrorCode = IngredientsError.INVALID_FORMAT_INGREDIENT,
                    ErrorMessage = createErrorForCurrLine("har feil format på ingrediensnavn. Gyldig er a) 1 kg mel b) 1 kg grovt mel c) 1 kg mel, grovt")
                };
            }
            idx++;
            result.Add(new Ingredient { Unit = unit, Amount = amount, Name = name });
        }

        return new IngredientsParseResult { Ingredients = result };
    }
}


public class Ingredient
{
    public decimal Amount { get; set; }
    public string Unit { get; set; }
    public string Name { get; set; }
}