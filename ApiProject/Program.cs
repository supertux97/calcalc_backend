using System.Configuration;
using System.Globalization;
using System.Text.Json.Serialization;
using calcalc.BackgroundJobs;
using calcalc.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.DependencyInjection;
using MvcMovie.Data;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var config = new ConfigurationBuilder().AddEnvironmentVariables().
    AddUserSecrets<Program>().Build();

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Db is {config["CalcalcSqldb"]}");
builder.Services.AddDbContext<CalCalcContext>(options =>
    options.UseSqlServer(config["CalcalcSqldb"]));

/*
builder.Services.AddHangfire(configuration => configuration
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage("hangfire_taskqueue")); // Replace with your desired database file path

*/

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    var enumConverter = new JsonStringEnumConverter();
    options.JsonSerializerOptions.Converters.Add(enumConverter);
});

// builder.Services.AddHangfireServer();
builder.Services.AddHttpClient();
// builder.Services.AddHttpLogging(o => { });

var logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Logging.AddSerilog(logger);

builder.Host.UseSerilog( (ctx, conf) => conf.ReadFrom.Configuration(ctx.Configuration) );

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IMattilsynetAPIService, MattilsynetAPIServiceMock>();
}
else
{
    Console.WriteLine("PROD!!!!");
    builder.Services.AddScoped<IMattilsynetAPIService, MattilsynetAPIService>();
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
}

app.UseHttpsRedirection();
app.UseRouting();
//app.UseHangfireDashboard();


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


/*
BackgroundJob.Enqueue<MattilsynetSync>(
    (mtc) => mtc.DoSync());  
    */

Console.WriteLine("Started!");
app.Run();
