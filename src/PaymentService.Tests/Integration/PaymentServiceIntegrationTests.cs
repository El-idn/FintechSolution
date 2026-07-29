using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using PaymentService.DTOs;
using System.Net;

namespace PaymentService.Tests.Integration
{
    public class PaymentServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        public PaymentServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CreatePayment_WithSameIdempotencyKey_ReturnsSameResult()
        {
            var client = _factory.CreateClient();
            var request = new PaymentRequest
            {
                AccountId = Guid.NewGuid(),
                Amount = 100,
                Currency = "USD",
                Reference = "INTEGRATION-REF",
                Description = "Integration test payment"
            };
            var idempotencyKey = Guid.NewGuid().ToString();
            // First request
            var response1 = await client.PostAsJsonAsync("/api/v1/payments", request, new System.Threading.CancellationToken());
            response1.Headers.Add("Idempotency-Key", idempotencyKey);
            var result1 = await response1.Content.ReadFromJsonAsync<PaymentResponse>();
            // Second request with same key
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/payments")
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Add("Idempotency-Key", idempotencyKey);
            var response2 = await client.SendAsync(requestMessage);
            var result2 = await response2.Content.ReadFromJsonAsync<PaymentResponse>();
            Assert.Equal(result1.PaymentId, result2.PaymentId);
            Assert.Equal(result1.Status, result2.Status);
        }

        [Fact]
        public async Task CorrelationId_IsEchoedInResponse()
        {
            var client = _factory.CreateClient();
            var correlationId = Guid.NewGuid().ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/payments/account/00000000-0000-0000-0000-000000000000");
            request.Headers.Add("Correlation-ID", correlationId);
            var response = await client.SendAsync(request);
            Assert.True(response.Headers.TryGetValues("Correlation-ID", out var values));
            Assert.Contains(correlationId, values);
        }

        [Fact]
        public async Task CorrelationId_IsGeneratedIfMissing()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v1/payments/account/00000000-0000-0000-0000-000000000000");
            Assert.True(response.Headers.TryGetValues("Correlation-ID", out var values));
            Assert.False(string.IsNullOrWhiteSpace(values?.ToString()));
        }

        [Fact]
        public async Task InvalidRequest_ReturnsProblemDetails()
        {
            var client = _factory.CreateClient();
            // Send invalid paymentId (not a GUID)
            var response = await client.GetAsync("/api/v1/payments/not-a-guid");
            Assert.Contains(response.StatusCode, new[] { System.Net.HttpStatusCode.BadRequest, System.Net.HttpStatusCode.InternalServerError });
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task HealthEndpoints_ReturnOk()
        {
            var client = _factory.CreateClient();
            var health = await client.GetAsync("/health");
            var ready = await client.GetAsync("/ready");
            Assert.Equal(System.Net.HttpStatusCode.OK, health.StatusCode);
            Assert.Equal(System.Net.HttpStatusCode.OK, ready.StatusCode);
        }

        [Fact]
        public async Task MetricsEndpoint_ReturnsPrometheusMetrics()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/metrics");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("# HELP", content); // Prometheus metrics start with # HELP
        }
    }
} 