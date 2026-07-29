using TransactionService.Domain.Entities;

namespace TransactionService.Interfaces
{
    public interface ITransactionService
    {
        Task<Transaction> DepositAsync(Guid userId, Guid accountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null);
        Task<Transaction> WithdrawAsync(Guid userId, Guid accountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null);
        Task<Transaction> TransferAsync(Guid userId, Guid fromAccountId, Guid toAccountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null);
        Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(Guid userId, Guid accountId);
        Task<Transaction> GetTransactionByIdAsync(Guid transactionId);
        Task<Transaction> ReverseTransactionAsync(Guid userId, Guid transactionId, string reason);
    }
}
