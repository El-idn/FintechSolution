using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SharedKernel.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string correlationId = context.Request.Headers[CorrelationIdHeader];
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers[CorrelationIdHeader] = correlationId;
            }
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            using (_logger.BeginScope("Correlation-ID: {CorrelationId}", correlationId))
            {
                await _next(context);
            }
        }
    }
} 