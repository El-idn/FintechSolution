using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AccountService.Services;
using AccountService.Services.Interfaces;

public class AccountOwnerHandler : AuthorizationHandler<AccountOwnerRequirement, Guid>
{
    private readonly IAccountService _accountService;

    public AccountOwnerHandler(IAccountService accountService)
    {
        _accountService = accountService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountOwnerRequirement requirement,
        Guid accountId)
    {
        var userIdClaim = context.User.FindFirst("sub")?.Value;
        if (userIdClaim == null)
        {
            return;
        }

        var account = await _accountService.GetAccountByIdAsync(accountId);
        if (account == null)
        {
            return;
        }

        if (account.UserId == Guid.Parse(userIdClaim))
        {
            context.Succeed(requirement);
        }
    }
}
