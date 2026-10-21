using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using calcalc.Models;
using MvcMovie.Data;
using Microsoft.EntityFrameworkCore;
using calcalc.ViewModels;

namespace calcalc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CalCalcContext _context;

    public HomeController(ILogger<HomeController> logger, CalCalcContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewmodel = new HomeViewModel();
        viewmodel.CntFoodItemsExternal = await _context.FoodItem.Where(fi => fi.UserAdded == false).CountAsync();
        viewmodel.CntFoodItemsUserAdded = await _context.FoodItem.Where(fi => fi.UserAdded == true).CountAsync();
        viewmodel.CntFoodItemsTotal = viewmodel.CntFoodItemsExternal + viewmodel.CntFoodItemsUserAdded;
        viewmodel.LatestSyncEvent = await _context.FoodItemsSyncEvent.OrderByDescending(se => se.Time).FirstOrDefaultAsync();
        viewmodel.LatestSuccessfullSyncEvent = await _context.FoodItemsSyncEvent.Where(se => se.Successfull == true).OrderByDescending(se => se.Time).FirstOrDefaultAsync();
        viewmodel.FoodItemsUserLatestCreated = await _context.FoodItem.Where(fi => fi.UserAdded == true).OrderByDescending(fi => fi.Created).Take(10).ToListAsync();
        viewmodel.FoodItemsUserLatestUpdated = await _context.FoodItem.Where(fi => fi.UserAdded == true).OrderByDescending(fi => fi.Updated).Take(10).ToListAsync();

        return View(viewmodel);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
