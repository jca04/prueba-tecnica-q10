

using Microsoft.EntityFrameworkCore;
using OrderFlow.InventoryWorker.Domain;

namespace OrderFlow.InventoryWorker.Data
{
    public class DatabaseInitializer
    {
        private static readonly Stock[] Seed =
        [
            new() { Sku = "ABC-01", Disponible = 100 },
            new() { Sku = "ABC-02", Disponible = 50 },
            new() { Sku = "ABC-03", Disponible = 5 },
        ];

        public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync(ct);

            if (!await db.Stocks.AnyAsync(ct))
            {
                db.Stocks.AddRange(Seed);
                await db.SaveChangesAsync(ct);
            }
        }
    }
}