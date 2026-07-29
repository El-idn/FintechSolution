using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NotificationService.Consumers;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var result = new BadRequestObjectResult(new { error = "ModelStateInvalid", details = context.ModelState });
        result.ContentTypes.Add("application/json");
        return result;
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<TransactionCreatedConsumer>();
    x.AddConsumer<TransactionProcessedConsumer>();
    x.AddConsumer<PaymentSucceededNotificationConsumer>();
    x.AddConsumer<PaymentFailedNotificationConsumer>();
    x.AddConsumer<SecurityAlertConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var username = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMq:Password"] ?? "guest";
        var virtualHost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
        cfg.Host(host, virtualHost, h =>
        {
            h.Username(username);
            h.Password(password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<INotificationService, NotificationService.Services.NotificationService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[GlobalExceptionHandler] {ex.Message}");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = new { error = "GlobalException", message = ex.Message };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(error));
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("NotificationService");
logger.LogInformation("NotificationService starting up with RabbitMQ consumers...");

app.Run();
