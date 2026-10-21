using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using calcalc.Models;
using calcalc.Repositories;
using calcalc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using Serilog;

namespace calcalc.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly IFoodItemRepository _foodItemRepository;
        private readonly IFoodUnitRepository _foodUnitRepository;

        public FoodItemController(IFoodItemRepository foodItemRepository, IFoodUnitRepository foodUnitRepository)
        {
            _foodItemRepository = foodItemRepository;
            _foodUnitRepository = foodUnitRepository;
        }

        // GET: FoodItem
        public async Task<IActionResult> Index(string nameFilterString, bool isAddedByUserFilter, bool hasSynonymFilter, string notification, int paginationPageNo = 1)
        {
            var PAGINATION_ITEMS_PR_PAGE = 10;

            var searchResults = await _foodItemRepository.SearchForFooditems(
                includeSynonyms: true,
                includeUnits: true,
                searchStr: nameFilterString,
                onlyAddedByUser: isAddedByUserFilter,
                onlyHasSynonyms: hasSynonymFilter,
                paginationResultsPrPage: PAGINATION_ITEMS_PR_PAGE,
                paginationPageNo: paginationPageNo
            );

            var viewmodel = new FoodItemIndexViewmodel();

            viewmodel.FoodItems = searchResults.Items;
            viewmodel.HasNextPage = searchResults.HasNextPage;
            viewmodel.HasPreviousPage = searchResults.HasPreviousPage;
            viewmodel.NameFilterString = nameFilterString;
            viewmodel.Notification = notification;
            viewmodel.PaginationPageNo = paginationPageNo;

            return View(viewmodel);
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
            var foodUnits = await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: true);
            var viewModel = new FoodHasUnitCreateViewmodel { UnitIdx = idx, FoodUnitsListItems = foodUnits.Select(fu => new SelectListItem { Text = fu.Name, Value = fu.Id.ToString() }).ToList() };

            ViewBag.idx = idx;
            ViewBag.FoodUnitsListItems = foodUnits
                .Select(fu => new SelectListItem { Text = fu.Name, Value = fu.Id.ToString() }).ToList();
            ViewBag.ChooseUnitUIStr = "Velg enhet";

            return PartialView("FoodHasUnit/Create", viewModel);
        }

        [Route("/FoodItem/FoodHasUnit/Get")]
        public async Task<IActionResult> FoodHasUnitGet(int idx, int foodItemId)
        {
            var foodUnits = await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: true);
            var viewModel = new FoodHasUnitCreateViewmodel
            {
                UnitIdx = idx,
                FoodUnitsListItems = foodUnits.Select(fu => new SelectListItem { Text = fu.Name, Value = fu.Id.ToString() }).ToList()
            };

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
                var newItem = new FoodItem { NameWithoutDetails = foodItem.Name, Name = foodItem.Name, Calories = foodItem.Calories, Created = DateTime.UtcNow };
                await _foodItemRepository.AddFoodItemAsync(newItem);
                return RedirectToAction(nameof(Index));
            }
            return View(new FoodItemCreateViewModel { FoodItem = foodItem });
        }

        // GET: FoodItem/Edit/5
        public async Task<IActionResult> Edit(int? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(new FoodItemEditViewmodel { Id = foodItem.Id, Name = foodItem.Name, ReturnUrl = returnUrl, UserAdded = foodItem.UserAdded, Calories = foodItem.Calories, SynonymsCommaseparated = String.Join(",", foodItem.Synonyms.Select(s => s.Name)) });
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

            var foodItemDb = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)id);
            if (foodItemDb is null)
            {
                return NotFound();
            }

            if (!String.IsNullOrEmpty(foodItem.SynonymsCommaseparated))
            {
                var synonymsToAdd = foodItem.SynonymsCommaseparated.Trim().Split(",");
                foodItemDb.Synonyms = synonymsToAdd
                    .Select(s => new FoodSynonym { Name = s, FoodItemId = id }).ToList();
            }
            else if (foodItemDb.Synonyms.Count > 0)
            {
                foodItemDb.Synonyms = new List<FoodSynonym>();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (foodItemDb.UserAdded)
                    {
                        foodItemDb.Name = foodItem.Name;
                        foodItemDb.Calories = foodItem.Calories;
                    }
                    foodItemDb.Updated = DateTime.UtcNow;
                    await _foodItemRepository.UpdateFoodItemAsync(foodItemDb);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await FoodItemExists(foodItemDb.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (foodItem.ReturnUrl != null)
                {
                    return Redirect(foodItem.ReturnUrl);
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

            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)id);
            if (foodItem == null)
            {
                return NotFound();
            }

            return View(foodItem);
        }

        // GET: FoodItem/ManageUnits/5
        public async Task<IActionResult> ManageUnits(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)id);
            if (foodItem == null)
            {
                return NotFound();
            }
            var units = await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: false);

            return View(new FoodManageUnitsViewModel { FoodItem = foodItem, Units = units });
        }

        // POST: FoodItem/ManageUnits/5
        public async Task<IActionResult> DeleteFoodUnit(int foodItemId, int foodUnitId)
        {
            if (foodItemId == null)
            {
                return NotFound();
            }

            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)foodItemId);
            if (foodItem == null)
            {
                return NotFound();
            }
            foodItem.Units = foodItem.Units.Where(u => u.FoodUnitId != foodUnitId).ToList();
            await _foodItemRepository.UpdateFoodItemAsync(foodItem);

            return RedirectToAction(nameof(Index));
        }

        // POST: FoodItem/ManageUnits/5
        public async Task AddFoodUnit(int foodItemId, int foodUnitId, int amountGramsPrUnit)
        {
            if (foodItemId == null)
            {
                Response.StatusCode = 404;
                return;
            }

            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: (int)foodItemId);
            if (foodItem == null)
            {
                Response.StatusCode = 404;
                return;
            }
            foodItem.Units.Add(new FoodHasUnit { FoodItemId = foodItemId, FoodUnitId = foodUnitId, GramsPerUnit = amountGramsPrUnit });
            await _foodItemRepository.UpdateFoodItemAsync(foodItem);
        }


        // POST: FoodItem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodItem = await _foodItemRepository.GetFoodItemByIdAsync(includeSynonyms: true, includeUnits: true, id: id);
            if (foodItem != null && foodItem.UserAdded)
            {
                await _foodItemRepository.DeleteFoodItemAsync(foodItem);
            }

            return RedirectToAction(nameof(Index), new { notification = "The food item was deleted" });
        }

        private async Task<bool> FoodItemExists(int id)
        {
            return (await _foodItemRepository.GetAllFoodItemsAsync(includeSynonyms: false, includeUnits: false)).Any(e => e.Id == id);
        }
    }
}
