using System;
using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class PaymentRequest
    {
        [Required]
        public Guid AccountId { get; set; }
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        public string Currency { get; set; } = "USD";
        public string? Reference { get; set; }
        public string? Description { get; set; }
    }
} 