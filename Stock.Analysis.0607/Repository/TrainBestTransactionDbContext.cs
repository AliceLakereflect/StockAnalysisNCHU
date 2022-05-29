using Microsoft.EntityFrameworkCore;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class TrainBestTransactionDbContext : DbContext
    {
        public DbSet<TrainBestTransaction> TrainBestTransaction { get; set; }

        public TrainBestTransactionDbContext(DbContextOptions<TrainBestTransactionDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainBestTransaction>()
                .HasKey(t => t.Id);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            return base.SaveChanges();
        }
    }
}

