using System;
using AccountService.Enums;


namespace AccountService.Domain.Entities
{
    public class Account
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        // public string AccountType { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
