using AccountService.Consumers;
using AccountService.Domain.Entities;
using AccountService.Enums;
using AccountService.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using SharedKernel.Events;
using Xunit;

namespace AccountService.Tests.Unit
{
    public class PaymentSucceededConsumerTests
    {
        [Fact]
        public async Task Consume_Should_Debit_Account_When_Funds_Available()
        {
            var accountId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                UserId = Guid.NewGuid(),
                AccountNumber = "ACCT-1",
                Balance = 200m,
                AccountType = AccountType.Savings,
                CreatedAt = DateTime.UtcNow
            };

            var accountService = new Mock<IAccountService>();
            accountService.Setup(s => s.GetAccountByIdAsync(accountId)).ReturnsAsync(account);
            accountService.Setup(s => s.UpdateAccountBalanceAsync(accountId, 150m, It.IsAny<string>(), paymentId))
                .ReturnsAsync(account);

            var consumer = new PaymentSucceededConsumer(accountService.Object, Mock.Of<ILogger<PaymentSucceededConsumer>>());
            var context = Mock.Of<ConsumeContext<PaymentSucceededEvent>>(c =>
                c.Message == new PaymentSucceededEvent
                {
                    PaymentId = paymentId,
                    AccountId = accountId,
                    Amount = 50m,
                    Currency = "EUR",
                    ProcessedAt = DateTime.UtcNow
                });

            await consumer.Consume(context);

            accountService.Verify(s => s.UpdateAccountBalanceAsync(accountId, 150m, It.IsAny<string>(), paymentId), Times.Once);
        }

        [Fact]
        public async Task Consume_Should_Not_Debit_When_Insufficient_Funds()
        {
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                UserId = Guid.NewGuid(),
                AccountNumber = "ACCT-2",
                Balance = 10m,
                AccountType = AccountType.CurrentAccount,
                CreatedAt = DateTime.UtcNow
            };

            var accountService = new Mock<IAccountService>();
            accountService.Setup(s => s.GetAccountByIdAsync(accountId)).ReturnsAsync(account);

            var consumer = new PaymentSucceededConsumer(accountService.Object, Mock.Of<ILogger<PaymentSucceededConsumer>>());
            var context = Mock.Of<ConsumeContext<PaymentSucceededEvent>>(c =>
                c.Message == new PaymentSucceededEvent
                {
                    PaymentId = Guid.NewGuid(),
                    AccountId = accountId,
                    Amount = 50m,
                    Currency = "EUR",
                    ProcessedAt = DateTime.UtcNow
                });

            await consumer.Consume(context);

            accountService.Verify(s => s.UpdateAccountBalanceAsync(
                It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<Guid?>()), Times.Never);
        }
    }
}
