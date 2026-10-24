
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrdersApi.Domain;

namespace OrderFlow.OrdersApi.Data
{
    public static class DatabaseInitializer
    {

        private static readonly Product[] stocks =
        {
            new() { Sku =  "ABC-01", Nombre = "Camiseta" },
            new() { Sku = "ABC-02",Nombre = "Pantalon" },
            new() { Sku = "ABC-03",Nombre = "Chaqueta" },
        };


        public static async Task InitializeAsync(IServiceProvider service, CancellationToken ct = default)
        {
            using var scope = service.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync(ct);

            if (!await db.Products.AnyAsync(ct))
            {
                db.Products.AddRange(stocks);
                await db.SaveChangesAsync(ct);
            }
        }

    }
}