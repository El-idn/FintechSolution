using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.DTOs;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request, string idempotencyKey);
        Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
        Task<PaymentResponse> ProcessPaymentAsync(Guid paymentId);
        Task<PaymentResponse> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByAccountAsync(Guid accountId);
    }
} 