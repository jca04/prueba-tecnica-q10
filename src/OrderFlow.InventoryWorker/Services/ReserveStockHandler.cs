using Microsoft.EntityFrameworkCore;
using OrderFlow.InventoryWorker.Data;
using OrderFlow.InventoryWorker.Domain;

namespace OrderFlow.InventoryWorker.Services;


public sealed record ReservationResult(ReservationOutcome Outcome, string? Motivo, bool WasDuplicate);

public sealed class ReserveStockHandler(ApplicationDbContext db, ILogger<ReserveStockHandler> logger)
{
    public async Task<ReservationResult> HandleAsync(
        Guid eventId, Guid orderId, string sku, int cantidad, CancellationToken ct
    )
    {
        var previous = await FindProcessedAsync(eventId, ct);
        if (previous is not null) return ToResult(previous, duplicate: true);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var affected = await db.Stocks
                .Where(s => s.Sku == sku && s.Disponible >= cantidad)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Disponible, x => x.Disponible - cantidad), ct);

            string? motivo = null;

            if (affected == 0)
            {
                var skuExists = await db.Stocks.AnyAsync(s => s.Sku == sku, ct);
                motivo = skuExists ? "Stock insuficiente" : "El SKU no existe en inventario";
            }

            var processed = new ProcessedEvent
            {
                EventId = eventId,
                OrderId = orderId,
                Outcome = affected > 0 ? ReservationOutcome.Reserved : ReservationOutcome.Rejected,
                Motivo = motivo,
                ProcesadoEn = DateTime.UtcNow
            };

            db.ProcessedEvents.Add(processed);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            logger.LogInformation("Evento {EventId} (pedido {OrderId}): {Outcome} {Sku} x{Cantidad}",
                eventId, orderId, processed.Outcome, sku, cantidad);

            return ToResult(processed, duplicate: false);
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync(ct);
            db.ChangeTracker.Clear();

            var raced = await FindProcessedAsync(eventId, ct);
            if (raced is null) throw;
            return ToResult(raced, duplicate: true);
        }
    }


    private Task<ProcessedEvent?> FindProcessedAsync(Guid eventId, CancellationToken ct) =>
         db.ProcessedEvents.AsNoTracking().FirstOrDefaultAsync(p => p.EventId == eventId, ct);

    private static ReservationResult ToResult(ProcessedEvent p, bool duplicate) =>
        new(p.Outcome, p.Motivo, duplicate);
}