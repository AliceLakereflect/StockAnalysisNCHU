using Microsoft.EntityFrameworkCore;
using Stock.Analysis._0607.Models;

namespace Stock.Analysis._0607.Repository
{
    public class TrainResultDbContext : DbContext
    {
        public DbSet<TrainResult> TrainResult { get; set; }

        public TrainResultDbContext(DbContextOptions<TrainResultDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TrainResult>()
                .HasKey(t => t.Id);
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            return base.SaveChanges();
        }
    }
}

