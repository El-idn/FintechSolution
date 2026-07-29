using System;
using System.Threading.Tasks;
using AccountService.DTOs;
using AccountService.Domain.Entities;
using AccountService.Services.Interfaces;
using AccountService.Repositories.Interfaces;

namespace AccountService.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request);
        Task<Account?> GetAccountByIdAsync(Guid accountId);
        Task<IEnumerable<Account>> GetAccountsByUserIdAsync(Guid userId);
        Task<Account> UpdateAccountBalanceAsync(Guid accountId, decimal newBalance, string changeReason, Guid? transactionId = null);
    }
}
