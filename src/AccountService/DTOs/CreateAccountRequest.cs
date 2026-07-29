using System.Text.Json.Serialization;
using AccountService.Enums;
using System.ComponentModel.DataAnnotations;


namespace AccountService.DTOs
{
    public class CreateAccountRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Required(ErrorMessage = "Account type is required.")]
        public AccountType AccountType { get; set; } // ✅ Enum, not string

        [Range(0, double.MaxValue, ErrorMessage = "Initial deposit must be non-negative.")]
        public decimal InitialDeposit { get; set; }

        [JsonIgnore] // Do not accept from client, it will come from token
        public Guid UserId { get; set; }
    }
}