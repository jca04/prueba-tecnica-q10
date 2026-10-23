using  Microsoft.AspNetCore.Mvc;


namespace OrderFlow.OrdersApi.Endpoints;


public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.MapGet("/",  GetOrders);

        return app;
    }
    

    public static  IResult GetOrders()
    {
        return Results.Ok();
    }
}