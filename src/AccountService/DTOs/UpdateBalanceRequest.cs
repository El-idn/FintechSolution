using System.ComponentModel.DataAnnotations;

namespace AccountService.DTOs
{
    public class UpdateBalanceRequest
    {
        [Required(ErrorMessage = "New balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "New balance must be non-negative.")]
        public decimal NewBalance { get; set; }

        [Required(ErrorMessage = "Change reason is required.")]
        [StringLength(500, ErrorMessage = "Change reason cannot exceed 500 characters.")]
        public string ChangeReason { get; set; } = string.Empty;

        public Guid? TransactionId { get; set; }
    }
} 