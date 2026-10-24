using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderFlow.InventoryWorker.Data;
using OrderFlow.InventoryWorker.Domain;
using OrderFlow.InventoryWorker.Services;

namespace OrderFlow.Tests;

public sealed class SqliteWorkerDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SqliteWorkerDb()
    {
        _connection.Open();
        using var ctx = CreateContext();
        ctx.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}

public class ReserveStockHandlerTests
{
    private static async Task<(SqliteWorkerDb fixture, ApplicationDbContext db)> ArrangeAsync(int stock)
    {
        var fixture = new SqliteWorkerDb();
        var db = fixture.CreateContext();
        db.Stocks.Add(new Stock { Sku = "ABC-01", Disponible = stock });
        await db.SaveChangesAsync();
        return (fixture, db);
    }


    [Fact]
    public async Task Same_event_processed_twice_deducts_stock_only_once()
    {
        var (fixture, db) = await ArrangeAsync(100);
        using var _ = fixture;
        await using var __ = db;
        var handler = new ReserveStockHandler(db, NullLogger<ReserveStockHandler>.Instance);
        var eventId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var first = await handler.HandleAsync(eventId, orderId, "ABC-01", 10, default);
        var second = await handler.HandleAsync(eventId, orderId, "ABC-01", 10, default);

        Assert.False(first.WasDuplicate);
        Assert.True(second.WasDuplicate);
        Assert.Equal(ReservationOutcome.Reserved, second.Outcome); // reenvía la decisión original
        Assert.Equal(90, (await db.Stocks.AsNoTracking().SingleAsync()).Disponible);

    }

    [Fact]
    public async Task Insufficient_stock_is_rejected_and_stock_is_untouched()
    {
        var (fixture, db) = await ArrangeAsync(5);
        using var _ = fixture;
        await using var __ = db;
        var handler = new ReserveStockHandler(db, NullLogger<ReserveStockHandler>.Instance);

        var result = await handler.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), "ABC-01", 10, default);

        Assert.Equal(ReservationOutcome.Rejected, result.Outcome);
        Assert.Contains("insuficiente", result.Motivo!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(5, (await db.Stocks.AsNoTracking().SingleAsync()).Disponible);
    }

}