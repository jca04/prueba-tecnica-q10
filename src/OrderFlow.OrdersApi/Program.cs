using OrderFlow.OrdersApi.Endpoints;
using Microsoft.EntityFrameworkCore;
using OrderFlow.OrdersApi.Data;
using OrderFlow.OrdersApi.Services;
using System.Text.Json.Serialization;
using MassTransit;
using OrderFlow.OrdersApi.Messaging;
using Microsoft.Extensions.Options;
using OrderFlow.OrdersApi.Hubs;
using OrderFlow.OrdersApi.Models;
using OrderFlow.OrdersApi.Consumers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddScoped<OrderService>();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks();

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<ApplicationDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumer<StockResultConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbit = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        cfg.Host(rabbit.Host, "/", h =>
        {
            h.Username(rabbit.Username);
            h.Password(rabbit.Password);
        });

        cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.MapOrdersEndpoints();
app.MapHub<OrdersHub>("/hubs/orders");
app.MapHealthChecks("/health");

// para la idempotencia
if (app.Configuration.GetValue<bool>("Features:DebugEndpoints"))
{
    app.MapPost("/debug/duplicate-order", async (
        CreateOrderRequest request, OrderService service, IBus bus, CancellationToken ct
    ) =>
    {
        var result = await service.CreateAsync(request, ct);
        if (!result.IsSuccess) return Results.ValidationProblem(result.Errors!);

        await bus.Publish(result.Event!, ct);
        return Results.Ok(result.Created);
    });
}

app.Run();