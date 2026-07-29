using System;
using PaymentService.Domain.Enums;

namespace PaymentService.DTOs
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Message { get; set; }
    }
} 