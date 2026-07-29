using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TransactionService.DTOs;
using TransactionService.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using SharedKernel.Controllers;
using SharedKernel.Constants;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : BaseApiController
{
    private readonly ITransactionService _transactionService;
    private readonly IMapper _mapper;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionService transactionService,
        IMapper mapper,
        ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new transaction (deposit or withdrawal).
    /// </summary>
    /// <param name="request">Transaction request</param>
    /// <returns>Created transaction</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request)
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
            _logger.LogWarning("Unauthorized transaction attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        try
        {
            Transaction result;

            switch (request.Type)
            {
                case TransactionType.Deposit:
                    result = await _transactionService.DepositAsync(
                        userId, request.AccountId, request.Amount, 
                        request.Description, request.ObnConsentId, request.ObnClientId);
                    break;

                case TransactionType.Withdrawal:
                    result = await _transactionService.WithdrawAsync(
                        userId, request.AccountId, request.Amount, 
                        request.Description, request.ObnConsentId, request.ObnClientId);
                    break;

                default:
                    _logger.LogWarning("Invalid transaction type: {TransactionType}", request.Type);
                    return BadRequest($"Invalid transaction type: {request.Type}");
            }

            var response = _mapper.Map<TransactionResponse>(result);
            _logger.LogInformation("Transaction created successfully: {TransactionId} for User: {UserId}", result.Id, userId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized transaction attempt for User: {UserId}, Account: {AccountId}", userId, request.AccountId);
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid transaction operation for User: {UserId}, Account: {AccountId}: {Message}", userId, request.AccountId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction for User: {UserId}, Account: {AccountId}", userId, request.AccountId);
            return StatusCode(500, "An error occurred while processing the transaction.");
        }
    }

    /// <summary>
    /// Creates a transfer between accounts.
    /// </summary>
    /// <param name="request">Transfer request</param>
    /// <returns>Created transfer transaction</returns>
    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransfer([FromBody] TransferRequest request)
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
            _logger.LogWarning("Unauthorized transfer attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        try
        {
            var result = await _transactionService.TransferAsync(
                userId, request.FromAccountId, request.ToAccountId, request.Amount,
                request.Description, request.ObnConsentId, request.ObnClientId);

            var response = _mapper.Map<TransactionResponse>(result);
            _logger.LogInformation("Transfer created successfully: {TransactionId} from Account: {FromAccount} to Account: {ToAccount}", 
                result.Id, request.FromAccountId, request.ToAccountId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized transfer attempt for User: {UserId}, From Account: {FromAccount}", userId, request.FromAccountId);
            return Unauthorized(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid transfer operation for User: {UserId}, From Account: {FromAccount}: {Message}", userId, request.FromAccountId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transfer for User: {UserId}, From Account: {FromAccount}", userId, request.FromAccountId);
            return StatusCode(500, "An error occurred while processing the transfer.");
        }
    }

    /// <summary>
    /// Gets transaction history for an account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <returns>Transaction history</returns>
    [HttpGet("history/{accountId}")]
    public async Task<IActionResult> GetTransactionHistory(Guid accountId)
    {
        Guid userId;
        try
        {
            userId = GetUserId();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized history access attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        try
        {
            var transactions = await _transactionService.GetTransactionHistoryAsync(userId, accountId);
            var responses = _mapper.Map<IEnumerable<TransactionResponse>>(transactions);

            var response = new TransactionHistoryResponse
            {
                AccountId = accountId,
                UserId = userId,
                Transactions = responses,
                TotalCount = responses.Count(),
                CurrentBalance = responses.Any() ? responses.Last().NewBalance : 0
            };

            _logger.LogInformation("Transaction history retrieved for User: {UserId}, Account: {AccountId}, Count: {Count}", 
                userId, accountId, response.TotalCount);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized history access for User: {UserId}, Account: {AccountId}", userId, accountId);
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction history for User: {UserId}, Account: {AccountId}", userId, accountId);
            return StatusCode(500, "An error occurred while retrieving transaction history.");
        }
    }

    /// <summary>
    /// Gets a specific transaction by ID.
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Transaction details</returns>
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransaction(Guid transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
            var response = _mapper.Map<TransactionResponse>(transaction);

            _logger.LogInformation("Transaction retrieved: {TransactionId}", transactionId);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Transaction not found: {TransactionId}", transactionId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction: {TransactionId}", transactionId);
            return StatusCode(500, "An error occurred while retrieving the transaction.");
        }
    }

    /// <summary>
    /// Reverses a completed transaction.
    /// </summary>
    /// <param name="request">Reverse transaction request</param>
    /// <returns>Reversal transaction</returns>
    [HttpPost("reverse")]
    public async Task<IActionResult> ReverseTransaction([FromBody] ReverseTransactionRequest request)
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
            _logger.LogWarning("Unauthorized reversal attempt: {Message}", ex.Message);
            return Unauthorized(ex.Message);
        }

        try
        {
            var result = await _transactionService.ReverseTransactionAsync(userId, request.TransactionId, request.Reason);
            var response = _mapper.Map<TransactionResponse>(result);

            _logger.LogInformation("Transaction reversed successfully: {ReversalId} for original transaction: {OriginalId}", 
                result.Id, request.TransactionId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized reversal attempt for User: {UserId}, Transaction: {TransactionId}", userId, request.TransactionId);
            return Unauthorized(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Transaction not found for reversal: {TransactionId}", request.TransactionId);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid reversal operation for Transaction: {TransactionId}: {Message}", request.TransactionId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reversing transaction: {TransactionId}", request.TransactionId);
            return StatusCode(500, "An error occurred while reversing the transaction.");
        }
    }

    /// <summary>
    /// Gets all transaction types for reference.
    /// </summary>
    /// <returns>Available transaction types</returns>
    [HttpGet("types")]
    [AllowAnonymous]
    public IActionResult GetTransactionTypes()
    {
        var types = Enum.GetValues<TransactionType>()
            .Select(t => new { Value = (int)t, Name = t.ToString() })
            .ToList();

        return Ok(types);
    }

    /// <summary>
    /// Gets all transaction statuses for reference.
    /// </summary>
    /// <returns>Available transaction statuses</returns>
    [HttpGet("statuses")]
    [AllowAnonymous]
    public IActionResult GetTransactionStatuses()
    {
        var statuses = Enum.GetValues<TransactionStatus>()
            .Select(s => new { Value = (int)s, Name = s.ToString() })
            .ToList();

        return Ok(statuses);
    }
}
