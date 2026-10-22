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
        public CalCalcContext(DbContextOptions<CalCalcContext> options)
            : base(options)
        {
        }

        public CalCalcContext()
            : base()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDbFunction(() => Fuzzysearch(default, default))
            .HasName("fuzzysearch")
            .HasSchema("public");
        }
        public int Fuzzysearch(string input1, string input2) => throw new NotSupportedException();

        public virtual DbSet<FoodItem> FoodItem { get; set; } = default!;
        public DbSet<calcalc.Models.FoodUnit> FoodUnit { get; set; } = default!;
        public DbSet<calcalc.Models.FoodUnitSynonym> FoodUnitSynonyms { get; set; } = default!;
        public DbSet<calcalc.Models.FoodSynonym> FoodSynonym { get; set; } = default!;
        public DbSet<calcalc.Models.FoodItemsSyncEvent> FoodItemsSyncEvent { get; set; } = default!;
    }
}
