using Microsoft.EntityFrameworkCore;

namespace Stock.Analysis._0607.Repository
{
    public class MaContext : DbContext
    {
        public DbSet<Ma> Ma { get; set; }

        public string DbPath { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql("Host=localhost:5432;Database=StockResearch;Username=postgres;Password=13");

        public MaContext()
        {
        }
    }

    public class Ma
    {
        public int StockName { get; set; }
        public string Period { get; set; }
        public string Value { get; set; }
        public string Timestamp { get; set; }
    }
}

