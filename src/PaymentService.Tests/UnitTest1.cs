using Xunit;
using PaymentService.Services;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Enums;
using PaymentService.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PaymentService.Data;

namespace PaymentService.Tests
{
    public class PaymentServiceBusinessRulesTests
    {
        private PaymentDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PaymentDbContext(options);
        }

        private PaymentService.Services.PaymentService GetService(PaymentDbContext context, out Mock<ILogger<PaymentService.Services.PaymentService>> loggerMock)
        {
            loggerMock = new Mock<ILogger<PaymentService.Services.PaymentService>>();
            return new PaymentService.Services.PaymentService(context, loggerMock.Object);
        }

        // 1. Reference Uniqueness & Idempotency
        [Fact]
        public async Task CreatePaymentAsync_Should_Prevent_Duplicate_Reference()
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "REF123",
                Description = "Test payment"
            };
            var first = await service.CreatePaymentAsync(request, "test-key");
            var second = await service.CreatePaymentAsync(request, "test-key");
            Assert.Equal(first.PaymentId, second.PaymentId);
            Assert.Contains("Duplicate idempotency key", second.Message);
        }

        // 2. Payment Status Transitions
        [Fact]
        public async Task UpdatePaymentStatusAsync_Should_Enforce_Valid_Transitions()
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "REF456",
                Description = "Test payment"
            };
            var createResp = await service.CreatePaymentAsync(request, "test-key");
            var paymentId = createResp.PaymentId;
            // Valid transition: Pending -> Succeeded
            var validResp = await service.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Succeeded);
            Assert.Equal(PaymentStatus.Succeeded, validResp.Status);
            // Invalid transition: Succeeded -> Pending
            var invalidResp = await service.UpdatePaymentStatusAsync(paymentId, PaymentStatus.Pending);
            Assert.Contains("Invalid status transition", invalidResp.Message);
        }

        // 3. Payment Amount Validation
        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        [InlineData(1000001)]
        public async Task CreatePaymentAsync_Should_Validate_Amount(decimal amount)
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = amount,
                Currency = "USD",
                Reference = Guid.NewGuid().ToString(),
                Description = "Test payment"
            };
            var resp = await service.CreatePaymentAsync(request, "test-key");
            Assert.Equal(PaymentStatus.Failed, resp.Status);
            Assert.Contains("Invalid payment amount", resp.Message);
        }

        // 4. Retry Logic for Failed Payments
        [Fact]
        public async Task ProcessPaymentAsync_Should_Enforce_MaxRetryCount()
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "RETRY-TEST",
                Description = "Test payment"
            };
            var createResp = await service.CreatePaymentAsync(request, "test-key");
            var paymentId = createResp.PaymentId;
            // Simulate failed payments by setting status and retry count manually
            var payment = await context.Payments.FindAsync(paymentId);
            payment.Status = PaymentStatus.Failed;
            payment.RetryCount = 3;
            await context.SaveChangesAsync();
            var resp = await service.ProcessPaymentAsync(paymentId);
            Assert.Equal(PaymentStatus.Failed, resp.Status);
            Assert.Contains("Maximum retry attempts reached", resp.Message);
        }

        // 5. Audit Logging
        [Fact]
        public async Task All_Actions_Should_Log_Appropriately()
        {
            var context = GetInMemoryDbContext();
            var loggerMock = new Mock<ILogger<PaymentService.Services.PaymentService>>();
            var service = new PaymentService.Services.PaymentService(context, loggerMock.Object);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "LOG-TEST",
                Description = "Test payment"
            };
            var createResp = await service.CreatePaymentAsync(request, "test-key");
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Payment created")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // 6. Payment Expiry
        [Fact]
        public async Task GetPaymentAsync_Should_Expire_Overdue_Payments()
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "EXPIRY-TEST",
                Description = "Test payment"
            };
            var createResp = await service.CreatePaymentAsync(request, "test-key");
            var paymentId = createResp.PaymentId;
            // Simulate expiry by setting ExpiresAt in the past
            var payment = await context.Payments.FindAsync(paymentId);
            payment.ExpiresAt = DateTime.UtcNow.AddHours(-1);
            await context.SaveChangesAsync();
            var resp = await service.GetPaymentAsync(paymentId);
            Assert.Equal(PaymentStatus.Expired, resp.Status);
            Assert.Contains("Payment status", resp.Message);
        }

        // 7. Custom Payment Descriptions
        [Theory]
        [InlineData("This is a fraud payment")]
        [InlineData("This description is way too long.............................................................................................................................................................................................................")]
        public async Task CreatePaymentAsync_Should_Validate_Description(string description)
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = Guid.NewGuid().ToString(),
                Description = description
            };
            var resp = await service.CreatePaymentAsync(request, "test-key");
            Assert.Equal(PaymentStatus.Failed, resp.Status);
            Assert.True(resp.Message.Contains("forbidden words") || resp.Message.Contains("too long"));
        }

        // 8. Consistency & Error Handling
        [Fact]
        public async Task All_Methods_Should_Handle_Errors_Gracefully()
        {
            var context = GetInMemoryDbContext();
            var service = GetService(context, out _);
            // Non-existent payment
            var resp = await service.GetPaymentAsync(Guid.NewGuid());
            Assert.Equal(PaymentStatus.Failed, resp.Status);
            Assert.Contains("not found", resp.Message);
        }
    }
}
