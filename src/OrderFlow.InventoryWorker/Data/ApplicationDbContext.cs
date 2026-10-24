using Microsoft.EntityFrameworkCore;
using OrderFlow.InventoryWorker.Domain;

namespace OrderFlow.InventoryWorker.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Stock> Stocks => Set<Stock>();
        public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Stock>(e =>
            {
                e.HasKey(s => s.Sku);
                e.Property(s => s.Sku).HasMaxLength(50);
            });

            b.Entity<ProcessedEvent>(e =>
            {
                e.HasKey(p => p.EventId);
                e.Property(p => p.EventId).ValueGeneratedNever();
                e.Property(p => p.Outcome).HasConversion<string>().HasMaxLength(20);
                e.Property(p => p.Motivo).HasMaxLength(300);
            });
        }
    }
}