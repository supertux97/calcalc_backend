using calcalc.Models;
using calcalc.Repositories;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

public class EFCoreFoodUnitRepository : IFoodUnitRepository
{
    private readonly CalCalcContext _context;

    public EFCoreFoodUnitRepository(CalCalcContext context)
    {
        _context = context;
    }

    public async Task AddFoodUnitAsync(FoodUnit foodUnit)
    {
        _context.Add(foodUnit);
        await _context.SaveChangesAsync();
    }

    public Task DeleteFoodUnitAsync(FoodUnit foodUnit)
    {
        throw new NotImplementedException();
    }

    public async Task<List<FoodUnit>> GetAllFoodUnitsAsync(bool includeSynonyms)
    {
        IQueryable<FoodUnit> items = _context.FoodUnit;
        if (includeSynonyms)
        {
            items = items.Include(i => i.Synonyms);
        }
        return await items.ToListAsync();
    }

    public async Task<FoodUnit?> GetFoodUnitByIdAsync(bool includeSynonyms, int id)
    {
        var item = _context.FoodUnit.Where(fu => fu.Id == id);
        if (includeSynonyms)
        {
            item = item.Include(i => i.Synonyms);
        }
        return await item.FirstOrDefaultAsync();
    }

    public async Task UpdateFoodUnitAsync(FoodUnit foodUnit)
    {
        _context.FoodUnit.Update(foodUnit);
        await _context.SaveChangesAsync();
    }
}