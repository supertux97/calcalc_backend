using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using calcalc.Models;
using calcalc.ViewModels;
using calcalc.Repositories;

namespace calcalc.Controllers
{
    public class FoodUnitController : Controller
    {
        private readonly IFoodUnitRepository _foodUnitRepository;

        public FoodUnitController(IFoodUnitRepository foodUnitRepository)
        {
            _foodUnitRepository = foodUnitRepository;
        }

        // GET: FoodUnit
        public async Task<IActionResult> Index()
        {
            return View(await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: true));
        }

        // GET: FoodUnit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FoodUnit/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] FoodUnit foodUnit)
        {
            if (ModelState.IsValid)
            {
                await _foodUnitRepository.AddFoodUnitAsync(foodUnit);
                return RedirectToAction(nameof(Index));
            }
            return View(foodUnit);
        }

        // GET: FoodUnit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodUnit = await _foodUnitRepository.GetFoodUnitByIdAsync(includeSynonyms: true, id: (int)id);
            if (foodUnit == null)
            {
                return NotFound();
            }
            return View(new FoodUnitEditViewmodel { AmountDeciliters = foodUnit.AmountDeciliters, Id = foodUnit.Id, Name = foodUnit.Name, SynonymsCommaSeparated = String.Join(", ", foodUnit.Synonyms.Select(fu => fu.Name)) });
        }

        // POST: FoodUnit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodUnitEditViewmodel foodUnit)
        {
            var toSave = await _foodUnitRepository.GetFoodUnitByIdAsync(includeSynonyms: true, id: (int)id);
            if (toSave is null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(new FoodUnitEditViewmodel { Id = foodUnit.Id, Name = foodUnit.Name, SynonymsCommaSeparated = String.Join(", ", toSave.Synonyms.Select(fu => fu.Name)) });
            }

            if (!String.IsNullOrEmpty(foodUnit.SynonymsCommaSeparated))
            {
                toSave.Synonyms = foodUnit.SynonymsCommaSeparated.Replace(", ", ",").Split(",")
                    .Select(u => new FoodUnitSynonym { FoodUnitId = id, Name = u }).ToList();
            }

            toSave.Name = foodUnit.Name;
            toSave.AmountDeciliters = foodUnit.AmountDeciliters;
            try
            {
                await _foodUnitRepository.UpdateFoodUnitAsync(toSave);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await FoodUnitExists(toSave.Id))
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

        // GET: FoodUnit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodUnit = await _foodUnitRepository.GetFoodUnitByIdAsync(includeSynonyms: true, id: (int)id);
            if (foodUnit == null)
            {
                return NotFound();
            }

            return View(foodUnit);
        }

        // POST: FoodUnit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodUnit = await _foodUnitRepository.GetFoodUnitByIdAsync(includeSynonyms: true, id: (int)id);
            if (foodUnit != null)
            {
                await _foodUnitRepository.DeleteFoodUnitAsync(foodUnit);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> FoodUnitExists(int id)
        {
            return (await _foodUnitRepository.GetAllFoodUnitsAsync(includeSynonyms: false)).Any(e => e.Id == id);
        }
    }
}
