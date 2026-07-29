using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Domain.Enums;
using SharedKernel.Events;

namespace PaymentService.Infrastructure
{
    public class OutboxPublisher : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisher> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public OutboxPublisher(IServiceScopeFactory scopeFactory, ILogger<OutboxPublisher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PublishPendingAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Outbox publisher iteration failed");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        private async Task PublishPendingAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var pending = await db.OutboxEvents
                .Where(e => !e.Processed)
                .OrderBy(e => e.OccurredAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var outboxEvent in pending)
            {
                try
                {
                    await PublishTypedEventAsync(publishEndpoint, outboxEvent.EventType, outboxEvent.Payload, cancellationToken);
                    outboxEvent.Processed = true;
                    outboxEvent.ProcessedAt = DateTime.UtcNow;
                    await db.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Published outbox event {EventId} ({EventType})", outboxEvent.Id, outboxEvent.EventType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish outbox event {EventId} ({EventType})", outboxEvent.Id, outboxEvent.EventType);
                }
            }
        }

        private static async Task PublishTypedEventAsync(
            IPublishEndpoint publishEndpoint,
            string eventType,
            string payload,
            CancellationToken cancellationToken)
        {
            var data = JsonSerializer.Deserialize<PaymentOutboxPayload>(payload, JsonOptions)
                ?? throw new InvalidOperationException("Invalid outbox payload");

            switch (eventType)
            {
                case "PaymentCreated":
                    await publishEndpoint.Publish(new PaymentCreatedEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        Currency = data.Currency ?? "EUR",
                        Reference = data.Reference,
                        Description = data.Description,
                        CreatedAt = data.CreatedAt
                    }, cancellationToken);
                    break;
                case "PaymentSucceeded":
                    await publishEndpoint.Publish(new PaymentSucceededEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        Currency = data.Currency ?? "EUR",
                        Reference = data.Reference,
                        ProcessedAt = data.ProcessedAt ?? DateTime.UtcNow
                    }, cancellationToken);
                    break;
                case "PaymentFailed":
                    await publishEndpoint.Publish(new PaymentFailedEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        Currency = data.Currency ?? "EUR",
                        Reference = data.Reference,
                        FailedAt = data.ProcessedAt ?? DateTime.UtcNow
                    }, cancellationToken);
                    break;
                case "PaymentStatusUpdated":
                    await publishEndpoint.Publish(new PaymentStatusUpdatedEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        Currency = data.Currency ?? "EUR",
                        Status = data.Status.ToString(),
                        UpdatedAt = data.ProcessedAt ?? DateTime.UtcNow
                    }, cancellationToken);
                    break;
                case "PaymentExpired":
                    await publishEndpoint.Publish(new PaymentExpiredEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        Currency = data.Currency ?? "EUR",
                        ExpiredAt = DateTime.UtcNow
                    }, cancellationToken);
                    break;
                case "PaymentRetryStarted":
                    await publishEndpoint.Publish(new PaymentRetryStartedEvent
                    {
                        PaymentId = data.Id,
                        AccountId = data.AccountId,
                        Amount = data.Amount,
                        RetryCount = data.RetryCount,
                        StartedAt = DateTime.UtcNow
                    }, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown outbox event type: {eventType}");
            }
        }

        private sealed class PaymentOutboxPayload
        {
            public Guid Id { get; set; }
            public Guid AccountId { get; set; }
            public decimal Amount { get; set; }
            public string? Currency { get; set; }
            public PaymentStatus Status { get; set; }
            public string? Reference { get; set; }
            public string? Description { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? ProcessedAt { get; set; }
            public DateTime? ExpiresAt { get; set; }
            public int RetryCount { get; set; }
        }
    }
}
