using Microsoft.EntityFrameworkCore;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class StockModelDbContext : DbContext
    {
        public DbSet<StockModel> StockModel { get; set; }

        public StockModelDbContext(DbContextOptions<StockModelDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockModel>()
                .HasKey(t => t.Id);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            return base.SaveChanges();
        }
    }
}

