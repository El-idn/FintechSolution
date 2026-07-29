using System.ComponentModel.DataAnnotations;
using TransactionService.Domain.Enums;

namespace TransactionService.DTOs
{
    public class TransactionRequest
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // Open Banking fields
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnClientName { get; set; }
    }

    public class TransferRequest
    {
        [Required]
        public Guid FromAccountId { get; set; }

        [Required]
        public Guid ToAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        // Open Banking fields
        public string? ObnConsentId { get; set; }
        public string? ObnClientId { get; set; }
        public string? ObnClientName { get; set; }
    }

    public class ReverseTransactionRequest
    {
        [Required]
        public Guid TransactionId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;
    }
}
