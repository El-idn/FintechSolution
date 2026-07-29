using AccountService.Enums; // Or wherever your enum is defined
using System.Text.Json.Serialization;

namespace AccountService.DTOs
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType AccountType { get; set; }
    }
}
