using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MassTransit;
using AccountService.Services;
using AccountService.Repositories.Interfaces;
using AccountService.Domain.Entities;
using AccountService.DTOs;
using AccountService.Enums;
using SharedKernel.Events;

namespace AccountService.Tests.Unit
{
    public class AccountServiceTests
    {
        private readonly Mock<IAccountRepository> _mockRepository;
        private readonly Mock<ILogger<AccountService.Services.AccountService>> _mockLogger;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly AccountService.Services.AccountService _accountService;

        public AccountServiceTests()
        {
            _mockRepository = new Mock<IAccountRepository>();
            _mockLogger = new Mock<ILogger<AccountService.Services.AccountService>>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            
            _accountService = new AccountService.Services.AccountService(
                _mockRepository.Object, 
                _mockLogger.Object, 
                _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ValidRequest_ShouldCreateAccountAndPublishEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                AccountType = AccountType.Savings,
                InitialDeposit = 1000.00m
            };

            var expectedAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = 1000.00m,
                AccountType = AccountType.Savings,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync(expectedAccount);

            // Act
            var result = await _accountService.CreateAccountAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Balance.Should().Be(1000.00m);
            result.AccountType.Should().Be(AccountType.Savings);
            result.AccountNumber.Should().NotBeNullOrEmpty();

            // Verify repository was called
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);

            // Verify Open Banking event was published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountCreatedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task CreateAccountAsync_RepositoryThrowsException_ShouldRethrowException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                AccountType = AccountType.Savings,
                InitialDeposit = 1000.00m
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Account>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _accountService.CreateAccountAsync(userId, request));

            // Verify event was not published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountCreatedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ExistingAccount_ShouldReturnAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var expectedAccount = new Account
            {
                Id = accountId,
                UserId = Guid.NewGuid(),
                AccountNumber = "ACCT-12345678",
                Balance = 1000.00m,
                AccountType = AccountType.Savings
            };

            _mockRepository.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync(expectedAccount);

            // Act
            var result = await _accountService.GetAccountByIdAsync(accountId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(accountId);
            result.AccountNumber.Should().Be("ACCT-12345678");
            result.Balance.Should().Be(1000.00m);
        }

        [Fact]
        public async Task GetAccountByIdAsync_NonExistingAccount_ShouldReturnNull()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync((Account?)null);

            // Act
            var result = await _accountService.GetAccountByIdAsync(accountId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAccountsByUserIdAsync_ExistingUser_ShouldReturnAccounts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedAccounts = new List<Account>
            {
                new Account { Id = Guid.NewGuid(), UserId = userId, AccountNumber = "ACCT-1", Balance = 1000.00m },
                new Account { Id = Guid.NewGuid(), UserId = userId, AccountNumber = "ACCT-2", Balance = 2000.00m }
            };

            _mockRepository.Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedAccounts);

            // Act
            var result = await _accountService.GetAccountsByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(account => account.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ValidUpdate_ShouldUpdateBalanceAndPublishEvent()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var existingAccount = new Account
            {
                Id = accountId,
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = 1000.00m,
                AccountType = AccountType.Savings
            };

            var updatedAccount = new Account
            {
                Id = accountId,
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = 1500.00m,
                AccountType = AccountType.Savings
            };

            _mockRepository.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync(existingAccount);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                .ReturnsAsync(updatedAccount);

            // Act
            var result = await _accountService.UpdateAccountBalanceAsync(
                accountId, 1500.00m, "Test deposit", Guid.NewGuid());

            // Assert
            result.Should().NotBeNull();
            result.Balance.Should().Be(1500.00m);

            // Verify repository calls
            _mockRepository.Verify(r => r.GetByIdAsync(accountId), Times.Once);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Once);

            // Verify Open Banking event was published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountBalanceChangedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_AccountNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync((Account?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.UpdateAccountBalanceAsync(accountId, 1500.00m, "Test deposit"));

            // Verify event was not published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountBalanceChangedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_RepositoryThrowsException_ShouldRethrowException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var existingAccount = new Account
            {
                Id = accountId,
                UserId = Guid.NewGuid(),
                Balance = 1000.00m
            };

            _mockRepository.Setup(r => r.GetByIdAsync(accountId))
                .ReturnsAsync(existingAccount);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => 
                _accountService.UpdateAccountBalanceAsync(accountId, 1500.00m, "Test deposit"));

            // Verify event was not published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountBalanceChangedEvent>(), default), Times.Never);
        }

        [Theory]
        [InlineData(AccountType.Savings, 1000.00)]
        [InlineData(AccountType.FixedDeposit, 500.00)]
        [InlineData(AccountType.CurrentAccount, 10000.00)]
        public async Task CreateAccountAsync_DifferentAccountTypes_ShouldCreateCorrectAccountType(AccountType accountType, decimal initialDeposit)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                AccountType = accountType,
                InitialDeposit = initialDeposit
            };

            var expectedAccount = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = initialDeposit,
                AccountType = accountType,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<Account>()))
                .ReturnsAsync(expectedAccount);

            // Act
            var result = await _accountService.CreateAccountAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.AccountType.Should().Be(accountType);
            result.Balance.Should().Be(initialDeposit);
        }
    }
} 