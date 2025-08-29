using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using calcalc.Models;

namespace MvcMovie.Data
{
    public class CalCalcContext : DbContext
    {
        public CalCalcContext (DbContextOptions<CalCalcContext> options)
            : base(options)
        {
        }
        
        public CalCalcContext ()
            : base()
        {
        }

        public virtual DbSet<FoodItem> FoodItem { get; set; } = default!;
        public DbSet<calcalc.Models.FoodUnit> FoodUnit { get; set; } = default!;
        public DbSet<calcalc.Models.FoodUnitSynonym> FoodUnitSynonyms { get; set; } = default!;
        public DbSet<calcalc.Models.FoodSynonym> FoodSynonym  { get; set; } = default!;
    }
}
