using Microsoft.AspNetCore.Mvc;
using PaymentService.Domain.Enums;
using PaymentService.DTOs;
using PaymentService.Services.Interfaces;

namespace PaymentService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> CreatePayment(
            [FromBody] PaymentRequest request,
            [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {
            var response = await _paymentService.CreatePaymentAsync(request, idempotencyKey);
            return Ok(response);
        }

        [HttpGet("{paymentId}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(Guid paymentId)
        {
            var response = await _paymentService.GetPaymentAsync(paymentId);
            
            if (response.Status == PaymentStatus.Failed && response.Message?.Contains("not found") == true)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpPost("{paymentId}/process")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment(Guid paymentId)
        {
            var response = await _paymentService.ProcessPaymentAsync(paymentId);
            return Ok(response);
        }

        [HttpPatch("{paymentId}/status")]
        public async Task<ActionResult<PaymentResponse>> UpdatePaymentStatus(
            Guid paymentId, 
            [FromBody] PaymentStatus status)
        {
            var response = await _paymentService.UpdatePaymentStatusAsync(paymentId, status);
            
            if (response.Status == PaymentStatus.Failed && response.Message?.Contains("not found") == true)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpGet("account/{accountId}")]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPaymentsByAccount(Guid accountId)
        {
            var payments = await _paymentService.GetPaymentsByAccountAsync(accountId);
            return Ok(payments);
        }
    }
} 