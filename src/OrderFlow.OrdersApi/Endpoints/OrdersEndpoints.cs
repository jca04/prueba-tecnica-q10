using Microsoft.EntityFrameworkCore;
using OrderFlow.OrdersApi.Data;
using OrderFlow.OrdersApi.Models;
using OrderFlow.OrdersApi.Services;


namespace OrderFlow.OrdersApi.Endpoints;


public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/", GetOrders);
        group.MapGet("/{id:guid}", GetOrderById);
        group.MapPost("/", CreateOrder);

        return app;
    }


    private static async Task<IResult> GetOrders(ApplicationDbContext db, CancellationToken ct)
    {
        var orders = await db.Orders.AsNoTracking()
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync(ct);

        return Results.Ok(orders.Select(OrderResponse.From));
    }

    private static async Task<IResult> GetOrderById(Guid id, ApplicationDbContext db, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? Results.NotFound() : Results.Ok(OrderResponse.From(order));
    }

    private static async Task<IResult> CreateOrder(CreateOrderRequest request, OrderService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return result.IsSuccess
            ? Results.Created($"/orders/${result.Created!.Id}", result.Created)
            : Results.ValidationProblem(result.Errors!);
    }
}