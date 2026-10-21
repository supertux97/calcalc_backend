using calcalc.Models;
using calcalc.Repositories;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

public class EFCoreFoodSynonymRepository : IFoodSynonymRepository
{
    private readonly CalCalcContext _context;

    public EFCoreFoodSynonymRepository(CalCalcContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FoodSynonym>> GetAllSynonymsItemsAsync()
    {
        return await _context.FoodSynonym.ToListAsync();
    }

}