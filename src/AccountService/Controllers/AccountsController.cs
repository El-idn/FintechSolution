using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using System.Security.Claims;
using AccountService.DTOs;
using AccountService.Repositories;
using AccountService.Repositories.Interfaces;
using AccountService.Services;
using AccountService.Services.Interfaces;
using SharedKernel.Controllers;
using System.IdentityModel.Tokens.Jwt;
using SharedKernel.Constants;
using Microsoft.Extensions.Logging;


[ApiController]
[Route("api/[controller]")]
public class AccountsController : BaseApiController
{
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountsController> _logger;

    /// <summary>
    /// Constructor for AccountsController.
    /// </summary>
    /// <param name="accountService">Injected account service</param>
    /// <param name="mapper">Injected AutoMapper instance</param>
    /// <param name="logger">Injected logger</param>
    public AccountsController(IAccountService accountService, IMapper mapper, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new account for the authenticated user.
    /// </summary>
    /// <param name="request">Account creation request</param>
    /// <returns>Created account DTO</returns>
    [Authorize(Policy = PolicyConstants.CanCreateAccount)]
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Guid userId;
        try
        {
            userId = GetUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        request.UserId = userId;

        // Log claims for debugging (remove or lower log level in production)
        foreach (var claim in User.Claims)
        {
            _logger.LogInformation("CLAIM TYPE: {ClaimType}, VALUE: {ClaimValue}", claim.Type, claim.Value);
        }

        var account = await _accountService.CreateAccountAsync(userId, request);
        var response = _mapper.Map<AccountDto>(account);
        return Ok(response);
    }

    /// <summary>
    /// Gets an account by its ID.
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <returns>Account DTO if found</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null)
            return NotFound();
        var response = _mapper.Map<AccountDto>(account);
        return Ok(response);
    }

    /// <summary>
    /// Gets all accounts for the authenticated user.
    /// </summary>
    /// <returns>List of account DTOs</returns>
    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyAccounts()
    {
        Guid userId;
        try
        {
            userId = GetUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        var accounts = await _accountService.GetAccountsByUserIdAsync(userId);
        return Ok(accounts);
    }

    /// <summary>
    /// Updates account balance (for Open Banking compliance).
    /// </summary>
    /// <param name="id">Account ID</param>
    /// <param name="request">Balance update request</param>
    /// <returns>Updated account DTO</returns>
    [HttpPut("{id}/balance")]
    [Authorize(Policy = PolicyConstants.CanCreateAccount)]
    public async Task<IActionResult> UpdateAccountBalance(Guid id, [FromBody] UpdateBalanceRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var account = await _accountService.UpdateAccountBalanceAsync(id, request.NewBalance, request.ChangeReason, request.TransactionId);
            var response = _mapper.Map<AccountDto>(account);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account balance for Account: {AccountId}", id);
            return StatusCode(500, "An error occurred while updating the account balance.");
        }
    }
}