using AccountService.Services.Interfaces;
using MassTransit;
using SharedKernel.Events;

namespace AccountService.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<PaymentSucceededConsumer> _logger;

        public PaymentSucceededConsumer(IAccountService accountService, ILogger<PaymentSucceededConsumer> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation(
                "Settling payment {PaymentId} for account {AccountId}, amount {Amount}",
                message.PaymentId, message.AccountId, message.Amount);

            var account = await _accountService.GetAccountByIdAsync(message.AccountId);
            if (account == null)
            {
                _logger.LogError("Account {AccountId} not found for payment {PaymentId}", message.AccountId, message.PaymentId);
                return;
            }

            if (account.Balance < message.Amount)
            {
                _logger.LogError(
                    "Insufficient funds on account {AccountId} for payment {PaymentId}. Balance={Balance}, Amount={Amount}",
                    message.AccountId, message.PaymentId, account.Balance, message.Amount);
                return;
            }

            var newBalance = account.Balance - message.Amount;
            await _accountService.UpdateAccountBalanceAsync(
                message.AccountId,
                newBalance,
                $"Payment {message.PaymentId} succeeded",
                message.PaymentId);

            _logger.LogInformation(
                "Account {AccountId} debited for payment {PaymentId}. New balance: {Balance}",
                message.AccountId, message.PaymentId, newBalance);
        }
    }
}
