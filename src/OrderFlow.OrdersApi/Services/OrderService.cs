using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts.Events;
using OrderFlow.OrdersApi.Data;
using OrderFlow.OrdersApi.Domain;
using OrderFlow.OrdersApi.Models;
using OrderFlow.OrdersApi.Validation;

namespace OrderFlow.OrdersApi.Services
{
    public sealed class OrderService(
        ApplicationDbContext db,
        IPublishEndpoint publisher,
        ILogger<OrderService> logger
    )
    {
        public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken ct)
        {
            var input = request with
            {
                ClienteNombre = request.ClienteNombre?.Trim() ?? string.Empty,
                Sku = request.Sku?.Trim() ?? string.Empty
            };

            var skuExists = input.Sku.Length > 0 && await db.Products.AnyAsync(p => p.Sku == input.Sku, ct);
            var errors = OrderValidator.Validate(input, skuExists);
            if (errors.Count > 0) return CreateOrderResult.Invalid(errors);

            var order = Order.Create(input.ClienteNombre, input.Sku, input.Cantidad);
            var evt = new OrderCreated(Guid.NewGuid(), order.Id, order.Sku, order.Cantidad, DateTime.UtcNow);

            db.Orders.Add(order);

            await publisher.Publish(evt, ct);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Pedido {OrderId} creado({Sku} x{Cantidad}); evento {EventId} en outbox", order.Id, order.Sku, order.Cantidad, evt.EventId);

            return CreateOrderResult.Success(OrderResponse.From(order), evt);
        }
    };

}