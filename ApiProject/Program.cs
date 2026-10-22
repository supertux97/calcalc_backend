using System.Globalization;
using System.Text.Json.Serialization;
using calcalc.Repositories;
using calcalc.Services;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using Serilog;
using Serilog.Exceptions;


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>().Build();

builder.Services.AddDbContext<CalCalcContext>(options =>
    options.UseNpgsql(config["Database:ConnectionString"]));

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    var enumConverter = new JsonStringEnumConverter();
    options.JsonSerializerOptions.Converters.Add(enumConverter);
});

builder.Services.AddHttpClient();
// builder.Services.AddHttpLogging(o => { });

var logger = new LoggerConfiguration()
    .Enrich.WithExceptionDetails()
    .ReadFrom.Configuration(config).CreateLogger();


builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);


// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IFooditemsService, FoodItemsServiceMock>();
}
else
{
    Console.WriteLine("PROD!!!!");
    builder.Services.AddScoped<IFooditemsService, MattilsynetAPIService>();
    builder.Services.AddScoped<IFoodItemRepository, EFCorePsqlFoodItemRepository>();
    builder.Services.AddScoped<IFoodUnitRepository, EFCoreFoodUnitRepository>();
    builder.Services.AddScoped<IFoodSynonymRepository, EFCoreFoodSynonymRepository>();
    builder.Services.AddScoped<IIngredientLanguageSpecificParserStrategy, IngredientLanguageSpecificParserStrategyNorwegian>();
    builder.WebHost.UseStaticWebAssets();
}

var app = builder.Build();
// app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();


app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

var defaultCulture = new CultureInfo("no");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = new List<CultureInfo> { defaultCulture },
    SupportedUICultures = new List<CultureInfo> { defaultCulture }
};
app.UseRequestLocalization(localizationOptions);

Console.WriteLine("Started!");
app.Run();
