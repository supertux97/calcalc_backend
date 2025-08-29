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
using Microsoft.IdentityModel.Tokens;

namespace calcalc.Controllers
{
    public class FoodUnitController : Controller
    {
        private readonly CalCalcContext _context;

        public FoodUnitController(CalCalcContext context)
        {
            _context = context;
        }

        // GET: FoodUnit
        public async Task<IActionResult> Index()
        {
            return View(await _context.FoodUnit.Include(fu => fu.Synonyms).ToListAsync());
        }

        // GET: FoodUnit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodUnit = await _context.FoodUnit.FirstOrDefaultAsync(m => m.Id == id);
            if (foodUnit == null)
            {
                return NotFound();
            }

            return View(foodUnit);
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
                _context.Add(foodUnit);
                await _context.SaveChangesAsync();
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

            var foodUnit = await _context.FoodUnit.Include(fu => fu.Synonyms).Where(fu => fu.Id == id).FirstOrDefaultAsync();
            if (foodUnit == null)
            {
                return NotFound();
            }
            return View( new FoodUnitEditViewmodel {AmountDeciliters = foodUnit.AmountDeciliters, Id = foodUnit.Id, Name = foodUnit.Name , SynonymsCommaSeparated = String.Join(", ", foodUnit.Synonyms.Select(fu => fu.Name))}  );
        }

        // POST: FoodUnit/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodUnitEditViewmodel foodUnit)
        {
            var toSave = await _context.FoodUnit.Include(fu => fu.Synonyms).FirstOrDefaultAsync(fu => fu.Id == id); 
            if (toSave is null)
            {
                return NotFound();
            }

            _context.RemoveRange(toSave.Synonyms);

            if (!foodUnit.SynonymsCommaSeparated.IsNullOrEmpty())
            {
                toSave.Synonyms = foodUnit.SynonymsCommaSeparated.Replace(", ", ",").Split(",")
                    .Select(u => new FoodUnitSynonym { FoodUnitId = id, Name = u }).ToList();
            }

            toSave.Name = foodUnit.Name;
            toSave.AmountDeciliters = foodUnit.AmountDeciliters;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(toSave);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodUnitExists(toSave.Id))
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
            
            return View( new FoodUnitEditViewmodel {Id = foodUnit.Id, Name = foodUnit.Name , SynonymsCommaSeparated = String.Join(", ", toSave.Synonyms.Select(fu => fu.Name))}  );
        }

        // GET: FoodUnit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodUnit = await _context.FoodUnit
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var foodUnit = await _context.FoodUnit.FindAsync(id);
            if (foodUnit != null)
            {
                _context.FoodUnit.Remove(foodUnit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FoodUnitExists(int id)
        {
            return _context.FoodUnit.Any(e => e.Id == id);
        }
    }
}
