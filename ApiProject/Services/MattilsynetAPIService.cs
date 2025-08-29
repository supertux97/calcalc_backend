using System.Text.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

namespace calcalc.Services;

public class Portion
{
    public string PortionName { get; set; }
    public string PortionUnit { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
}

public class Calories
{
    // null means 0 calories
    public decimal? Quantity { get; set; }
}

public class MattilsynetAPIFoodItem
{
    public string FoodName { get; set; }
    public Calories Calories { get; set; } 
    public List<Portion> Portions { get; set; }
}

// todo searchKeywords
    public class MattilsynetAPIFoodItemsResult
    {
        public List<MattilsynetAPIFoodItem> Foods { get; set; }
    }

    public class MattilsynetAPIService : IMattilsynetAPIService
{
    private readonly HttpClient _httpClient;

    public MattilsynetAPIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.matvaretabellen.no/api/");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<MattilsynetAPIFoodItemsResult?> GetFoodItems()
    {
        var resp = await _httpClient.GetAsync("/api/nb/foods.json");
        var json = await resp.Content.ReadAsStringAsync();
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        // todo logging og sjekk resultat
        return JsonSerializer.Deserialize<MattilsynetAPIFoodItemsResult>(json, serializerOptions);
    }

    public void Dispose()
    {
        // not nececarry to dispose of anything, httpclient manages itself
    }

    public async ValueTask DisposeAsync()
    {
        // not nececarry to dispose of anything, httpclient manages itself
    }
}