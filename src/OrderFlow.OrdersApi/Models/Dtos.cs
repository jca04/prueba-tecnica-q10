using OrderFlow.Contracts.Events;
using OrderFlow.OrdersApi.Domain;

namespace OrderFlow.OrdersApi.Models;

public sealed record CreateOrderRequest(string ClienteNombre, string Sku, int Cantidad) { }

public sealed record OrderResponse(
    Guid Id, string ClienteNombre, string Sku, int Cantidad,
    string Estado, DateTime CreadoEn, string? MotivoRechazo
)
{
    public static OrderResponse From(Order o) =>
        new(o.Id, o.ClienteNombre, o.Sku, o.Cantidad, o.Estado.ToString(), o.CreadoEn, o.MotivoRechazo);
};

public sealed class CreateOrderResult
{

    public OrderResponse? Created { get; private init; }
    public OrderCreated? Event { get; private init; }
    public IDictionary<string, string[]>? Errors { get; private init; }
    public bool IsSuccess => Created is not null;


    public static CreateOrderResult Success(OrderResponse created, OrderCreated evt) => new() { Created = created, Event = evt };
    public static CreateOrderResult Invalid(IDictionary<string, string[]> errors) => new() { Errors = errors };

}