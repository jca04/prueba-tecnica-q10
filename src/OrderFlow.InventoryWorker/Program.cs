using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderFlow.InventoryWorker.Consumers;
using OrderFlow.InventoryWorker.Data;
using OrderFlow.InventoryWorker.Messaging;
using OrderFlow.InventoryWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var rabbitSection = builder.Configuration.GetSection("RabbitMq");

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();


builder.Services.AddScoped<ReserveStockHandler>();

builder.Services.AddMassTransit(x =>
{

    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {

        var rabbit = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

        cfg.Host(rabbit.Host, h =>
        {
            h.Username(rabbit.Username);
            h.Password(rabbit.Password);
        });

        cfg.UseMessageRetry(r => r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(context);
    });
});




var host = builder.Build();

await DatabaseInitializer.InitializeAsync(host.Services);
await host.RunAsync();
