using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using calcalc.Models;
using calcalc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MvcMovie.Data;
using Serilog;

namespace calcalc.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly CalCalcContext _context;

        public FoodItemController(CalCalcContext context)
        {
            _context = context;
        }

        // GET: FoodItem
        public async Task<IActionResult> Index(string nameFilterString, string paginationStartAtName, string paginationPrevStartAt, string notification)
        {
            var paginationItemsPrPage = 10;
            Console.WriteLine("WritelineTest!");

            IQueryable<FoodItem> foodItemsQueryWithSearch = _context.FoodItem.Include(fi => fi.Synonyms).Include(fi => fi.Units).ThenInclude(u => u.FoodUnit). OrderBy(fi => fi.Name);
            if (!String.IsNullOrEmpty(nameFilterString))
            {
                foodItemsQueryWithSearch = foodItemsQueryWithSearch.Where(fi => fi.Name.ToLower().Contains(nameFilterString.ToLower()));
                foodItemsQueryWithSearch =
                    foodItemsQueryWithSearch.OrderByDescending(i => i.Name.ToLower().StartsWith(nameFilterString));
            }

            IQueryable<FoodItem> foodItemsQueryWithPagination = foodItemsQueryWithSearch;
            
            var viewmodel = new FoodItemIndexViewmodel();  
           
            // when press next page, get next n elements with name > latest name
            if (!paginationStartAtName.IsNullOrEmpty())
            {
                foodItemsQueryWithPagination = foodItemsQueryWithPagination.Where(fi => String.Compare(fi.Name, paginationStartAtName) >= 0);
            }
            
            var foodItemsForCurrPage = await foodItemsQueryWithPagination.Take<FoodItem>(paginationItemsPrPage + 1).ToListAsync();
            
            if(foodItemsForCurrPage.Count > 0)
            {
                
                // try to take an extra item, to check if there are more items to display on the next page
                // the extra item is removed later
                var existsMorePages = foodItemsForCurrPage.Count == paginationItemsPrPage + 1;
                if (existsMorePages)
                {
                    viewmodel.NextPageStartAtName =  foodItemsForCurrPage.Last().Name;
                    foodItemsForCurrPage = foodItemsForCurrPage.SkipLast(1).ToList();
                    viewmodel.HasNextPage = true;
                }
            
                // if first item in result set ist not first item in set of all, we have previous items
                viewmodel.HasPreviousPage = foodItemsQueryWithSearch.Any(fi => String.Compare(foodItemsForCurrPage.First().Name, fi.Name) > 0);
                viewmodel.CurrPageStartAtName =  foodItemsForCurrPage.First().Name;
            }

            viewmodel.FoodItems = foodItemsForCurrPage;
            viewmodel.PrevStartAtName = paginationPrevStartAt; 
            viewmodel.NameFilterString = nameFilterString;
            viewmodel.Notification = notification;
            
            return View(viewmodel);
        }

        public async Task<string> Test()
        {
            throw new Exception("ErlendException");
            return "test";
        }
        
        // GET: FoodItem/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItem.Include(fi => fi.Synonyms).Include(fi => fi.Units).ThenInclude(fhu => fhu.FoodUnit)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // GET: FoodItem/Create
        public IActionResult Create()
        {
            return View();
        }
        
        // GET: FoodItem/FoodSynonym/Create
        [Route("FoodItem/FoodSynonym/Create")]
        public IActionResult FoodSynonymCreate(int idx)
        {
            ViewBag.idx = idx;
            return PartialView("FoodSynonym/Create");
        }

        [Route("/FoodItem/FoodHasUnit/Create")]
        public async Task<IActionResult> FoodHasUnitCreate(int idx)
        {
            var foodUnits = await _context.FoodUnit.ToListAsync();
            var viewModel = new FoodHasUnitCreateViewmodel{UnitIdx = idx, FoodUnitsListItems = foodUnits.Select(fu => new SelectListItem { Text=fu.Name, Value=fu.Id.ToString() } ).ToList()};

            ViewBag.idx = idx;
            ViewBag.FoodUnitsListItems = foodUnits
                .Select(fu => new SelectListItem { Text = fu.Name, Value = fu.Id.ToString() }).ToList();
            ViewBag.ChooseUnitUIStr = "Velg enhet";
            
            return PartialView("FoodHasUnit/Create", viewModel);
        }
        

        // POST: FoodItem/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Calories,Synonyms,Units")] FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                foodItem.UserAdded = true;
                _context.Add(foodItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View( new FoodItemCreateViewModel{FoodItem =  foodItem});
        }

        // GET: FoodItem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItem.Include(fi => fi.Synonyms).Where(fi => fi.Id == id).FirstOrDefaultAsync();
            if (foodItem == null)
            {
                return NotFound();
            }

            return View( new FoodItemEditViewmodel {Name = foodItem.Name, Calories = foodItem.Calories, SynonymsCommaseparated = String.Join(",", foodItem.Synonyms.Select(s => s.Name)) } );
        }

        // POST: FoodItem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItemEditViewmodel foodItem)
        {
            if (id != foodItem.Id)
            {
                return NotFound();
            }

            var foodItemDb = await _context.FoodItem.Include(fi => fi.Synonyms).Where(fi => fi.Id == id).FirstOrDefaultAsync();
            if (foodItemDb is null)
            {
                return NotFound();
            }

            if (!foodItem.SynonymsCommaseparated.IsNullOrEmpty())
            {
                var synonymsToAdd = foodItem.SynonymsCommaseparated.Trim().Split(",");
                foodItemDb.Synonyms = synonymsToAdd
                    .Select(s => new FoodSynonym { Name = s, FoodItemId = id }).ToList();
            }
            else if(foodItemDb.Synonyms.Count > 0)
            {
                foodItemDb.Synonyms = new List<FoodSynonym>();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foodItemDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodItemExists(foodItemDb.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(foodItem);
        }

        // GET: FoodItem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItem.Include(fi => fi.Synonyms)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // POST: FoodItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _context.FoodItem.FindAsync(id);
            if (foodItem != null)
            {
                _context.FoodItem.Remove(foodItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new {notification = "Matvaren ble slettet"});
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItem.Any(e => e.Id == id);
        }
    }
}
