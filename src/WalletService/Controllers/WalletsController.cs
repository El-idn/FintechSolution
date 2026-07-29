using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletService.Clients;

namespace WalletService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly IAccountClient _accountClient;
        private readonly ILogger<WalletsController> _logger;

        public WalletsController(IAccountClient accountClient, ILogger<WalletsController> logger)
        {
            _accountClient = accountClient;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBalance(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsCurrentUser(userId))
            {
                return Forbid();
            }

            var account = await ResolvePrimaryAccountAsync(cancellationToken);
            if (account == null)
            {
                return NotFound(new { Message = "No account found for user. Create an account first." });
            }

            return Ok(new
            {
                UserId = userId,
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            });
        }

        [HttpPost("{userId}/credit")]
        public async Task<IActionResult> Credit(Guid userId, [FromQuery] decimal amount, CancellationToken cancellationToken)
        {
            if (!IsCurrentUser(userId))
            {
                return Forbid();
            }

            if (amount <= 0)
            {
                return BadRequest(new { Message = "Amount must be positive." });
            }

            var account = await ResolvePrimaryAccountAsync(cancellationToken);
            if (account == null)
            {
                return NotFound(new { Message = "No account found for user. Create an account first." });
            }

            var newBalance = account.Balance + amount;
            await _accountClient.UpdateBalanceAsync(account.Id, newBalance, "Wallet credit", cancellationToken);
            _logger.LogInformation("Wallet credit for user {UserId}: {Amount}", userId, amount);

            return Ok(new { UserId = userId, AccountId = account.Id, Credited = amount, Balance = newBalance });
        }

        [HttpPost("{userId}/debit")]
        public async Task<IActionResult> Debit(Guid userId, [FromQuery] decimal amount, CancellationToken cancellationToken)
        {
            if (!IsCurrentUser(userId))
            {
                return Forbid();
            }

            if (amount <= 0)
            {
                return BadRequest(new { Message = "Amount must be positive." });
            }

            var account = await ResolvePrimaryAccountAsync(cancellationToken);
            if (account == null)
            {
                return NotFound(new { Message = "No account found for user. Create an account first." });
            }

            if (account.Balance < amount)
            {
                return BadRequest(new { Message = "Insufficient funds." });
            }

            var newBalance = account.Balance - amount;
            await _accountClient.UpdateBalanceAsync(account.Id, newBalance, "Wallet debit", cancellationToken);
            _logger.LogInformation("Wallet debit for user {UserId}: {Amount}", userId, amount);

            return Ok(new { UserId = userId, AccountId = account.Id, Debited = amount, Balance = newBalance });
        }

        private async Task<AccountClientDto?> ResolvePrimaryAccountAsync(CancellationToken cancellationToken)
        {
            var accounts = await _accountClient.GetMyAccountsAsync(cancellationToken);
            return accounts.OrderBy(a => a.AccountNumber).FirstOrDefault();
        }

        private bool IsCurrentUser(Guid userId)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            return Guid.TryParse(claim, out var currentUserId) && currentUserId == userId;
        }
    }
}
