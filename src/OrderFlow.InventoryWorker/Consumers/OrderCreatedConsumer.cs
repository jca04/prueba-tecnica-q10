
using MassTransit;
using OrderFlow.Contracts.Events;
using OrderFlow.InventoryWorker.Domain;
using OrderFlow.InventoryWorker.Services;

namespace OrderFlow.InventoryWorker.Consumers;

public class OrderCreatedConsumer(ReserveStockHandler handler, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var m = context.Message;

        var result = await handler.HandleAsync(m.EventId, m.OrderId, m.Sku, m.Cantidad, context.CancellationToken);

        if (result.WasDuplicate)
        {
            logger.LogWarning("Evento {EventId} duplicado: no se descuenta stock; se reenvía la decisión previa ({Outcome})",
                m.EventId, result.Outcome);
        }

        if (result.Outcome == ReservationOutcome.Reserved)
            await context.Publish(new StockReserved(Guid.NewGuid(), m.OrderId, DateTime.UtcNow));
        else
            await context.Publish(new StockRejected(Guid.NewGuid(), m.OrderId, result.Motivo ?? "Stock Insuficiente", DateTime.UtcNow));

    }
}