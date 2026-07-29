using System;
using System.Threading.Tasks;
using AccountService.DTOs;
using AccountService.Services;
using AccountService.Domain.Entities;
using AccountService.Services.Interfaces;
using AccountService.Repositories.Interfaces;
using AccountService.Enums;
using Microsoft.Extensions.Logging;
using MassTransit;
using SharedKernel.Events;

namespace AccountService.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<AccountService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public AccountService(IAccountRepository accountRepository, ILogger<AccountService> logger, IPublishEndpoint publishEndpoint)
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Account> CreateAccountAsync(Guid userId, CreateAccountRequest request)
        {
            try
            {
                var account = new Account
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AccountNumber = GenerateAccountNumber(),
                    Balance = request.InitialDeposit,
                    CreatedAt = DateTime.UtcNow,
                    AccountType = request.AccountType
                };

                await _accountRepository.AddAsync(account);
                _logger.LogInformation("Account created: {AccountId} for User: {UserId}", account.Id, userId);

                // Publish Open Banking event
                await _publishEndpoint.Publish(new AccountCreatedEvent
                {
                    AccountId = account.Id,
                    UserId = userId,
                    AccountNumber = account.AccountNumber,
                    InitialBalance = account.Balance,
                    CreatedAt = account.CreatedAt,
                    AccountType = account.AccountType.ToString()
                });

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account for User: {UserId}", userId);
                throw;
            }
        }

        public async Task<Account?> GetAccountByIdAsync(Guid accountId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account != null)
                {
                    _logger.LogInformation("Account retrieved: {AccountId}", accountId);
                }
                else
                {
                    _logger.LogWarning("Account not found: {AccountId}", accountId);
                }
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account: {AccountId}", accountId);
                throw;
            }
        }

        // simple random number
        private string GenerateAccountNumber()
        {
            var random = new Random();
            return $"ACCT-{random.Next(10000000, 99999999)}";
        }

        public async Task<IEnumerable<Account>> GetAccountsByUserIdAsync(Guid userId)
        {
            try
            {
                var accounts = await _accountRepository.GetByUserIdAsync(userId);
                _logger.LogInformation("Accounts retrieved for User: {UserId}", userId);
                return accounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for User: {UserId}", userId);
                throw;
            }
        }

        public async Task<Account> UpdateAccountBalanceAsync(Guid accountId, decimal newBalance, string changeReason, Guid? transactionId = null)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    throw new ArgumentException($"Account with ID {accountId} not found.");
                }

                var previousBalance = account.Balance;
                var changeAmount = newBalance - previousBalance;

                account.Balance = newBalance;
                await _accountRepository.UpdateAsync(account);

                _logger.LogInformation("Account balance updated: {AccountId}, Previous: {PreviousBalance}, New: {NewBalance}", 
                    accountId, previousBalance, newBalance);

                // Publish Open Banking balance change event
                await _publishEndpoint.Publish(new AccountBalanceChangedEvent
                {
                    AccountId = accountId,
                    UserId = account.UserId,
                    PreviousBalance = previousBalance,
                    NewBalance = newBalance,
                    ChangeAmount = changeAmount,
                    ChangeReason = changeReason,
                    ChangedAt = DateTime.UtcNow,
                    TransactionId = transactionId
                });

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account balance for Account: {AccountId}", accountId);
                throw;
            }
        }
    }
}