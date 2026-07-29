using TransactionService.Clients;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Repositories;
using TransactionService.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using SharedKernel.Events;

namespace TransactionService.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IAccountClient _accountClient;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IPublishEndpoint publishEndpoint,
            IAccountClient accountClient,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _publishEndpoint = publishEndpoint;
            _accountClient = accountClient;
            _logger = logger;
        }

        public async Task<Transaction> DepositAsync(Guid userId, Guid accountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing deposit for User: {UserId}, Account: {AccountId}, Amount: {Amount}", userId, accountId, amount);

            if (!await CheckAccountOwnershipAsync(userId, accountId))
            {
                throw new UnauthorizedAccessException("User does not own this account.");
            }

            var currentBalance = await GetAccountBalanceAsync(accountId);
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                UserId = userId,
                Amount = amount,
                PreviousBalance = currentBalance,
                NewBalance = currentBalance + amount,
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Pending,
                Description = description ?? "Deposit",
                Reference = Guid.NewGuid().ToString(),
                Currency = "EUR",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = !string.IsNullOrEmpty(obnConsentId),
                ObnConsentId = obnConsentId,
                ObnClientId = obnClientId
            };

            await _transactionRepository.AddAsync(transaction);
            await PublishTransactionCreatedEvent(transaction);
            return await ProcessTransactionAsync(transaction);
        }

        public async Task<Transaction> WithdrawAsync(Guid userId, Guid accountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing withdrawal for User: {UserId}, Account: {AccountId}, Amount: {Amount}", userId, accountId, amount);

            if (!await CheckAccountOwnershipAsync(userId, accountId))
            {
                throw new UnauthorizedAccessException("User does not own this account.");
            }

            var currentBalance = await GetAccountBalanceAsync(accountId);
            if (currentBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds.");
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                UserId = userId,
                Amount = -amount,
                PreviousBalance = currentBalance,
                NewBalance = currentBalance - amount,
                Type = TransactionType.Withdrawal,
                Status = TransactionStatus.Pending,
                Description = description ?? "Withdrawal",
                Reference = Guid.NewGuid().ToString(),
                Currency = "EUR",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = !string.IsNullOrEmpty(obnConsentId),
                ObnConsentId = obnConsentId,
                ObnClientId = obnClientId
            };

            await _transactionRepository.AddAsync(transaction);
            await PublishTransactionCreatedEvent(transaction);
            return await ProcessTransactionAsync(transaction);
        }

        public async Task<Transaction> TransferAsync(Guid userId, Guid fromAccountId, Guid toAccountId, decimal amount, string? description = null, string? obnConsentId = null, string? obnClientId = null)
        {
            _logger.LogInformation("Processing transfer for User: {UserId}, From: {FromAccount}, To: {ToAccount}, Amount: {Amount}",
                userId, fromAccountId, toAccountId, amount);

            if (!await CheckAccountOwnershipAsync(userId, fromAccountId))
            {
                throw new UnauthorizedAccessException("User does not own the source account.");
            }

            var destination = await _accountClient.GetAccountAsync(toAccountId)
                ?? throw new ArgumentException("Destination account not found.");

            var currentBalance = await GetAccountBalanceAsync(fromAccountId);
            if (currentBalance < amount)
            {
                throw new InvalidOperationException("Insufficient funds for transfer.");
            }

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = fromAccountId,
                UserId = userId,
                Amount = -amount,
                PreviousBalance = currentBalance,
                NewBalance = currentBalance - amount,
                Type = TransactionType.Transfer,
                Status = TransactionStatus.Pending,
                Description = description ?? $"Transfer to {toAccountId}",
                Reference = Guid.NewGuid().ToString(),
                ExternalReference = toAccountId.ToString(),
                Currency = "EUR",
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = !string.IsNullOrEmpty(obnConsentId),
                ObnConsentId = obnConsentId,
                ObnClientId = obnClientId
            };

            await _transactionRepository.AddAsync(transaction);
            await PublishTransactionCreatedEvent(transaction);
            return await ProcessTransactionAsync(transaction, destination.Id, destination.Balance + amount);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(Guid userId, Guid accountId)
        {
            if (!await CheckAccountOwnershipAsync(userId, accountId))
            {
                throw new UnauthorizedAccessException("User does not own this account.");
            }

            return await _transactionRepository.GetByAccountIdAsync(accountId);
        }

        public async Task<Transaction> GetTransactionByIdAsync(Guid transactionId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                throw new ArgumentException("Transaction not found.");
            }

            return transaction;
        }

        public async Task<Transaction> ReverseTransactionAsync(Guid userId, Guid transactionId, string reason)
        {
            var originalTransaction = await GetTransactionByIdAsync(transactionId);

            if (originalTransaction.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not own this transaction.");
            }

            if (originalTransaction.Status != TransactionStatus.Completed)
            {
                throw new InvalidOperationException("Can only reverse completed transactions.");
            }

            var currentBalance = await GetAccountBalanceAsync(originalTransaction.AccountId);
            var reversalAmount = -originalTransaction.Amount;

            var reversalTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = originalTransaction.AccountId,
                UserId = userId,
                Amount = reversalAmount,
                PreviousBalance = currentBalance,
                NewBalance = currentBalance + reversalAmount,
                Type = TransactionType.Refund,
                Status = TransactionStatus.Pending,
                Description = $"Reversal: {originalTransaction.Description}",
                Reference = Guid.NewGuid().ToString(),
                ExternalReference = originalTransaction.Id.ToString(),
                Currency = originalTransaction.Currency,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = originalTransaction.IsOpenBankingCompliant,
                ObnConsentId = originalTransaction.ObnConsentId,
                ObnClientId = originalTransaction.ObnClientId
            };

            await _transactionRepository.AddAsync(reversalTransaction);
            await PublishTransactionReversedEvent(reversalTransaction, originalTransaction, reason);
            return await ProcessTransactionAsync(reversalTransaction);
        }

        private async Task<Transaction> ProcessTransactionAsync(
            Transaction transaction,
            Guid? destinationAccountId = null,
            decimal? destinationNewBalance = null)
        {
            try
            {
                await _accountClient.UpdateBalanceAsync(
                    transaction.AccountId,
                    transaction.NewBalance,
                    $"{transaction.Type} {transaction.Id}",
                    transaction.Id);

                if (destinationAccountId.HasValue && destinationNewBalance.HasValue)
                {
                    await _accountClient.UpdateBalanceAsync(
                        destinationAccountId.Value,
                        destinationNewBalance.Value,
                        $"Transfer credit from {transaction.AccountId}",
                        transaction.Id);
                }

                transaction.Status = TransactionStatus.Completed;
                transaction.ProcessedAt = DateTime.UtcNow;
                await _transactionRepository.UpdateAsync(transaction);
                await PublishTransactionProcessedEvent(transaction);
                return transaction;
            }
            catch (Exception ex)
            {
                transaction.Status = TransactionStatus.Failed;
                transaction.FailureReason = ex.Message;
                await _transactionRepository.UpdateAsync(transaction);
                await PublishTransactionFailedEvent(transaction, ex.Message);
                _logger.LogError(ex, "Transaction processing failed: {TransactionId}", transaction.Id);
                throw;
            }
        }

        private async Task PublishTransactionCreatedEvent(Transaction transaction)
        {
            await _publishEndpoint.Publish(new TransactionCreatedEvent
            {
                TransactionId = transaction.Id,
                AccountId = transaction.AccountId,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                TransactionType = transaction.Type.ToString(),
                Description = transaction.Description ?? string.Empty,
                Reference = transaction.Reference ?? string.Empty,
                Currency = transaction.Currency ?? "EUR",
                Timestamp = transaction.Timestamp,
                IsOpenBankingCompliant = transaction.IsOpenBankingCompliant,
                ObnConsentId = transaction.ObnConsentId,
                ObnClientId = transaction.ObnClientId
            });
        }

        private async Task PublishTransactionProcessedEvent(Transaction transaction)
        {
            await _publishEndpoint.Publish(new TransactionProcessedEvent
            {
                TransactionId = transaction.Id,
                AccountId = transaction.AccountId,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                PreviousBalance = transaction.PreviousBalance,
                NewBalance = transaction.NewBalance,
                TransactionType = transaction.Type.ToString(),
                Status = transaction.Status.ToString(),
                Reference = transaction.Reference ?? string.Empty,
                ProcessedAt = transaction.ProcessedAt ?? DateTime.UtcNow,
                IsOpenBankingCompliant = transaction.IsOpenBankingCompliant,
                ObnConsentId = transaction.ObnConsentId,
                ObnClientId = transaction.ObnClientId
            });
        }

        private async Task PublishTransactionFailedEvent(Transaction transaction, string failureReason)
        {
            await _publishEndpoint.Publish(new TransactionFailedEvent
            {
                TransactionId = transaction.Id,
                AccountId = transaction.AccountId,
                UserId = transaction.UserId,
                Amount = transaction.Amount,
                TransactionType = transaction.Type.ToString(),
                FailureReason = failureReason,
                Reference = transaction.Reference ?? string.Empty,
                FailedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = transaction.IsOpenBankingCompliant,
                ObnConsentId = transaction.ObnConsentId,
                ObnClientId = transaction.ObnClientId
            });
        }

        private async Task PublishTransactionReversedEvent(Transaction reversalTransaction, Transaction originalTransaction, string reason)
        {
            await _publishEndpoint.Publish(new TransactionReversedEvent
            {
                TransactionId = reversalTransaction.Id,
                OriginalTransactionId = originalTransaction.Id,
                AccountId = reversalTransaction.AccountId,
                UserId = reversalTransaction.UserId,
                Amount = reversalTransaction.Amount,
                PreviousBalance = reversalTransaction.PreviousBalance,
                NewBalance = reversalTransaction.NewBalance,
                Reason = reason,
                Reference = reversalTransaction.Reference ?? string.Empty,
                ReversedAt = DateTime.UtcNow,
                IsOpenBankingCompliant = reversalTransaction.IsOpenBankingCompliant,
                ObnConsentId = reversalTransaction.ObnConsentId,
                ObnClientId = reversalTransaction.ObnClientId
            });
        }

        private async Task<bool> CheckAccountOwnershipAsync(Guid userId, Guid accountId)
        {
            var account = await _accountClient.GetAccountAsync(accountId);
            return account != null && account.UserId == userId;
        }

        private async Task<decimal> GetAccountBalanceAsync(Guid accountId)
        {
            var account = await _accountClient.GetAccountAsync(accountId)
                ?? throw new ArgumentException("Account not found.");
            return account.Balance;
        }
    }
}
