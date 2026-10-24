using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using OrderFlow.OrdersApi.Data;
using OrderFlow.OrdersApi.Domain;
using OrderFlow.OrdersApi.Hubs;
using OrderFlow.OrdersApi.Models;

namespace OrderFlow.OrdersApi.Consumers
{
    public class StockResultConsumer(
    ApplicationDbContext db,
    IHubContext<OrdersHub> hub,
    ILogger<StockResultConsumer> logger)
    : IConsumer<StockReserved>, IConsumer<StockRejected>
    {
        public Task Consume(ConsumeContext<StockReserved> context) =>
            ApplyAsync(context.Message.EventId, context.Message.OrderId,
                o => o.TryConfirm(), context.CancellationToken);

        public Task Consume(ConsumeContext<StockRejected> context) =>
            ApplyAsync(context.Message.EventId, context.Message.OrderId,
                o => o.TryReject(context.Message.Motivo), context.CancellationToken);

        private async Task ApplyAsync(Guid eventId, Guid orderId, Func<Order, bool> transition, CancellationToken ct)
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct);

            if (order is null)
            {
                logger.LogWarning("Evento {EventId} para pedido inexistente {OrderId}", eventId, orderId);
                return;
            }

            if (!transition(order))
            {
                logger.LogInformation("Evento {EventId} ignorado: pedido {OrderId} ya está {Estado}",
                eventId, orderId, order.Estado);
                return;
            }

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Pedido {OrderId} -> {Estado} (evento {EventId})", orderId, order.Estado, eventId);

            await hub.Clients.All.SendAsync("OrderUpdated", OrderResponse.From(order), ct);
        }
    }
}