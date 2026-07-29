using Xunit;
using PaymentService.Services;
using PaymentService.Domain.Enums;
using PaymentService.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using PaymentService.Data;

namespace PaymentService.Tests
{
    public class OutboxEventTests
    {
        private static PaymentDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new PaymentDbContext(options);
        }

        [Fact]
        public async Task CreatePaymentAsync_Should_Write_PaymentCreated_OutboxEvent()
        {
            await using var context = CreateContext();
            var service = new PaymentService.Services.PaymentService(context, Mock.Of<ILogger<PaymentService.Services.PaymentService>>());

            var response = await service.CreatePaymentAsync(new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 50,
                Currency = "EUR",
                Reference = "OUTBOX-1",
                Description = "Outbox test"
            }, "idem-1");

            Assert.Equal(PaymentStatus.Pending, response.Status);
            var outbox = Assert.Single(context.OutboxEvents);
            Assert.Equal("PaymentCreated", outbox.EventType);
            Assert.False(outbox.Processed);
            Assert.Contains(response.PaymentId.ToString(), outbox.Payload);
        }

        [Fact]
        public async Task UpdatePaymentStatusAsync_Should_Write_PaymentStatusUpdated_OutboxEvent()
        {
            await using var context = CreateContext();
            var service = new PaymentService.Services.PaymentService(context, Mock.Of<ILogger<PaymentService.Services.PaymentService>>());

            var created = await service.CreatePaymentAsync(new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 25,
                Currency = "EUR",
                Reference = "OUTBOX-2",
                Description = "Outbox status"
            }, "idem-2");

            await service.UpdatePaymentStatusAsync(created.PaymentId, PaymentStatus.Succeeded);

            Assert.Equal(2, context.OutboxEvents.Count());
            Assert.Contains(context.OutboxEvents, e => e.EventType == "PaymentStatusUpdated");
        }
    }
}
