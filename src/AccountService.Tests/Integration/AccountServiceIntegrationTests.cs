using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MassTransit;
using AccountService.Services;
using AccountService.Repositories;
using AccountService.Data;
using AccountService.Domain.Entities;
using AccountService.DTOs;
using AccountService.Enums;
using SharedKernel.Events;

namespace AccountService.Tests.Integration
{
    public class AccountServiceIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<AccountDbContext> _options;
        private readonly AccountDbContext _context;
        private readonly AccountRepository _repository;
        private readonly ILogger<AccountService.Services.AccountService> _logger;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly AccountService.Services.AccountService _accountService;

        public AccountServiceIntegrationTests()
        {
            // Setup in-memory database
            _options = new DbContextOptionsBuilder<AccountDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AccountDbContext(_options);
            _context.Database.EnsureCreated();

            // Setup repository
            _repository = new AccountRepository(_context);

            // Setup logger
            _logger = LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<AccountService.Services.AccountService>();

            // Setup mock publish endpoint
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();

            // Setup service
            _accountService = new AccountService.Services.AccountService(
                _repository, 
                _logger, 
                _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldCreateAccountInDatabaseAndPublishEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                AccountType = AccountType.Savings,
                InitialDeposit = 1000.00m
            };

            // Act
            var result = await _accountService.CreateAccountAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.Balance.Should().Be(1000.00m);
            result.AccountType.Should().Be(AccountType.Savings);
            result.AccountNumber.Should().NotBeNullOrEmpty();

            // Verify account was saved to database
            var savedAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == result.Id);
            savedAccount.Should().NotBeNull();
            savedAccount!.Balance.Should().Be(1000.00m);

            // Verify Open Banking event was published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountCreatedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccountFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = 1000.00m,
                AccountType = AccountType.Savings,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.GetAccountByIdAsync(account.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(account.Id);
            result.AccountNumber.Should().Be("ACCT-12345678");
            result.Balance.Should().Be(1000.00m);
        }

        [Fact]
        public async Task GetAccountsByUserIdAsync_ShouldReturnAllUserAccounts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accounts = new List<Account>
            {
                new Account { Id = Guid.NewGuid(), UserId = userId, AccountNumber = "ACCT-1", Balance = 1000.00m, AccountType = AccountType.Savings },
                new Account { Id = Guid.NewGuid(), UserId = userId, AccountNumber = "ACCT-2", Balance = 2000.00m, AccountType = AccountType.CurrentAccount },
                new Account { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), AccountNumber = "ACCT-3", Balance = 3000.00m, AccountType = AccountType.Savings }
            };

            _context.Accounts.AddRange(accounts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _accountService.GetAccountsByUserIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(account => account.UserId.Should().Be(userId));
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldUpdateBalanceAndPublishEvent()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountNumber = "ACCT-12345678",
                Balance = 1000.00m,
                AccountType = AccountType.Savings,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            var transactionId = Guid.NewGuid();

            // Act
            var result = await _accountService.UpdateAccountBalanceAsync(
                account.Id, 1500.00m, "Test deposit", transactionId);

            // Assert
            result.Should().NotBeNull();
            result.Balance.Should().Be(1500.00m);

            // Verify database was updated
            var updatedAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == account.Id);
            updatedAccount.Should().NotBeNull();
            updatedAccount!.Balance.Should().Be(1500.00m);

            // Verify Open Banking event was published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountBalanceChangedEvent>(), default), Times.Once);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_AccountNotFound_ShouldThrowArgumentException()
        {
            // Arrange
            var nonExistentAccountId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _accountService.UpdateAccountBalanceAsync(nonExistentAccountId, 1500.00m, "Test deposit"));

            // Verify no event was published
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountBalanceChangedEvent>(), default), Times.Never);
        }

        [Fact]
        public async Task CreateMultipleAccounts_ShouldPublishMultipleEvents()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var requests = new[]
            {
                new CreateAccountRequest { AccountType = AccountType.Savings, InitialDeposit = 1000.00m },
                new CreateAccountRequest { AccountType = AccountType.CurrentAccount, InitialDeposit = 2000.00m }
            };

            // Act
            foreach (var request in requests)
            {
                await _accountService.CreateAccountAsync(userId, request);
            }

            // Assert
            _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<AccountCreatedEvent>(), default), Times.Exactly(2));

            var accounts = await _context.Accounts.Where(a => a.UserId == userId).ToListAsync();
            accounts.Should().HaveCount(2);
        }

        [Fact]
        public async Task OpenBankingEvents_ShouldContainCorrectData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateAccountRequest
            {
                AccountType = AccountType.FixedDeposit,
                InitialDeposit = 5000.00m
            };

            // Act
            var result = await _accountService.CreateAccountAsync(userId, request);

            // Assert
            _mockPublishEndpoint.Verify(p => p.Publish(It.Is<AccountCreatedEvent>(e => 
                e.AccountId == result.Id && 
                e.UserId == userId && 
                e.AccountType == AccountType.FixedDeposit.ToString() && 
                e.InitialBalance == 5000.00m), default), Times.Once);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
} 