
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrdersApi.Domain;

namespace OrderFlow.OrdersApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Order> Orders => Set<Order>();

        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder b)
        {

            b.Entity<Order>(e =>
            {
                e.HasKey(o => o.Id);
                e.Property(o => o.ClienteNombre).HasMaxLength(200).IsRequired();
                e.Property(o => o.Sku).HasMaxLength(50).IsRequired();
                e.Property(o => o.Estado).HasConversion<string>().HasMaxLength(20);
                e.HasIndex(o => o.CreadoEn);
            });


            b.Entity<Product>(e =>
            {
                e.HasKey(p => p.Sku);
                e.Property(p => p.Sku).HasMaxLength(50);
                e.HasKey(p => p.Nombre);
            });



            b.AddInboxStateEntity();
            b.AddOutboxMessageEntity();
            b.AddOutboxStateEntity();
        }

    }
}