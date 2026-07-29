using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.DTOs;
using PaymentService.Services.Interfaces;
using System.Text.Json;

namespace PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        private const int MaxRetryCount = 3;
        private static readonly TimeSpan PaymentExpiryDuration = TimeSpan.FromHours(24);

        public PaymentService(PaymentDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, string idempotencyKey)
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existing = await _context.Payments.FirstOrDefaultAsync(p => p.AccountId == request.AccountId && p.IdempotencyKey == idempotencyKey);
                if (existing != null)
                {
                    return new PaymentResponse
                    {
                        PaymentId = existing.Id,
                        Status = existing.Status,
                        Message = $"Duplicate idempotency key. Existing payment status: {existing.Status}"
                    };
                }
            }
            if (!string.IsNullOrWhiteSpace(request.Reference))
            {
                var existing = await _context.Payments.FirstOrDefaultAsync(p => p.AccountId == request.AccountId && p.Reference == request.Reference);
                if (existing != null)
                {
                    return new PaymentResponse
                    {
                        PaymentId = existing.Id,
                        Status = existing.Status,
                        Message = $"Duplicate payment reference. Existing payment status: {existing.Status}"
                    };
                }
            }

            if (request.Amount <= 0 || request.Amount > 1000000)
            {
                return new PaymentResponse
                {
                    Status = PaymentStatus.Failed,
                    Message = "Invalid payment amount."
                };
            }

            if (request.Description != null && request.Description.Length > Domain.Entities.Payment.DescriptionMaxLength)
            {
                return new PaymentResponse
                {
                    Status = PaymentStatus.Failed,
                    Message = $"Description too long. Max length is {Domain.Entities.Payment.DescriptionMaxLength}."
                };
            }
            if (request.Description != null && Domain.Entities.Payment.ForbiddenWords.Any(w => request.Description.Contains(w, StringComparison.OrdinalIgnoreCase)))
            {
                return new PaymentResponse
                {
                    Status = PaymentStatus.Failed,
                    Message = "Description contains forbidden words."
                };
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = PaymentStatus.Pending,
                Reference = request.Reference,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(PaymentExpiryDuration),
                RetryCount = 0,
                IdempotencyKey = idempotencyKey
            };

            _context.Payments.Add(payment);
            AddOutboxEvent("PaymentCreated", payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment created: {PaymentId}", payment.Id);

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                Message = "Payment created and pending processing."
            };
        }

        public async Task<PaymentResponse> GetPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new PaymentResponse
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Failed,
                    Message = "Payment not found."
                };
            }
            if (payment.Status == PaymentStatus.Pending && payment.ExpiresAt.HasValue && payment.ExpiresAt.Value < DateTime.UtcNow)
            {
                payment.Status = PaymentStatus.Expired;
                AddOutboxEvent("PaymentExpired", payment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment expired: {PaymentId}", payment.Id);
            }
            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                Message = $"Payment status: {payment.Status}"
            };
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(Guid paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new PaymentResponse
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Failed,
                    Message = "Payment not found."
                };
            }
            if (payment.Status == PaymentStatus.Pending && payment.ExpiresAt.HasValue && payment.ExpiresAt.Value < DateTime.UtcNow)
            {
                payment.Status = PaymentStatus.Expired;
                AddOutboxEvent("PaymentExpired", payment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment expired: {PaymentId}", payment.Id);
                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    Status = payment.Status,
                    Message = "Payment expired."
                };
            }
            if (!Payment.IsValidStatusTransition(payment.Status, PaymentStatus.Succeeded) && !Payment.IsValidStatusTransition(payment.Status, PaymentStatus.Failed))
            {
                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    Status = payment.Status,
                    Message = $"Invalid status transition from {payment.Status}."
                };
            }
            if (payment.Status == PaymentStatus.Failed && payment.RetryCount >= MaxRetryCount)
            {
                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    Status = payment.Status,
                    Message = "Maximum retry attempts reached."
                };
            }
            if (payment.Status == PaymentStatus.Failed)
            {
                payment.RetryCount++;
                payment.Status = PaymentStatus.Pending;
                payment.ExpiresAt = DateTime.UtcNow.Add(PaymentExpiryDuration);
                AddOutboxEvent("PaymentRetryStarted", payment);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Payment retry: {PaymentId}, RetryCount: {RetryCount}", payment.Id, payment.RetryCount);
                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    Status = payment.Status,
                    Message = $"Payment retry started. Retry count: {payment.RetryCount}"
                };
            }
            var random = new Random();
            var success = random.Next(1, 11) <= 8;
            payment.Status = success ? PaymentStatus.Succeeded : PaymentStatus.Failed;
            payment.ProcessedAt = DateTime.UtcNow;
            AddOutboxEvent($"Payment{payment.Status}", payment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Payment processed: {PaymentId}, Status: {Status}", payment.Id, payment.Status);
            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                Message = success ? "Payment processed successfully." : "Payment processing failed."
            };
        }

        public async Task<PaymentResponse> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return new PaymentResponse
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Failed,
                    Message = "Payment not found."
                };
            }
            if (!Payment.IsValidStatusTransition(payment.Status, status))
            {
                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    Status = payment.Status,
                    Message = $"Invalid status transition from {payment.Status} to {status}."
                };
            }
            payment.Status = status;
            payment.ProcessedAt = DateTime.UtcNow;
            AddOutboxEvent("PaymentStatusUpdated", payment);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Payment status updated: {PaymentId}, New Status: {Status}", payment.Id, status);
            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Status = payment.Status,
                Message = $"Payment status updated to {status}."
            };
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByAccountAsync(Guid accountId)
        {
            var payments = await _context.Payments
                .Where(p => p.AccountId == accountId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            foreach (var payment in payments)
            {
                if (payment.Status == PaymentStatus.Pending && payment.ExpiresAt.HasValue && payment.ExpiresAt.Value < DateTime.UtcNow)
                {
                    payment.Status = PaymentStatus.Expired;
                    AddOutboxEvent("PaymentExpired", payment);
                    _logger.LogInformation("Payment expired: {PaymentId}", payment.Id);
                }
            }
            await _context.SaveChangesAsync();
            return payments.Select(p => new PaymentResponse
            {
                PaymentId = p.Id,
                Status = p.Status,
                Message = $"Payment {p.Id} - {p.Status}"
            });
        }

        private void AddOutboxEvent(string eventType, Payment payment)
        {
            _context.OutboxEvents.Add(new OutboxEvent
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                Payload = JsonSerializer.Serialize(new
                {
                    payment.Id,
                    payment.AccountId,
                    payment.Amount,
                    payment.Currency,
                    payment.Status,
                    payment.Reference,
                    payment.Description,
                    payment.CreatedAt,
                    payment.ProcessedAt,
                    payment.ExpiresAt,
                    payment.RetryCount
                }),
                OccurredAt = DateTime.UtcNow
            });
        }
    }
}
