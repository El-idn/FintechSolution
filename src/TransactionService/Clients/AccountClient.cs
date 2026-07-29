using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransactionService.Clients
{
    public interface IAccountClient
    {
        Task<AccountClientDto?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default);
        Task UpdateBalanceAsync(Guid accountId, decimal newBalance, string changeReason, Guid? transactionId = null, CancellationToken cancellationToken = default);
    }

    public class AccountClient : IAccountClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public AccountClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AccountClientDto?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken = default)
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, $"api/accounts/{accountId}");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AccountClientDto>(JsonOptions, cancellationToken);
        }

        public async Task UpdateBalanceAsync(
            Guid accountId,
            decimal newBalance,
            string changeReason,
            Guid? transactionId = null,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Put, $"api/accounts/{accountId}/balance");
            request.Content = JsonContent.Create(new
            {
                NewBalance = newBalance,
                ChangeReason = changeReason,
                TransactionId = transactionId
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"Failed to update account balance ({response.StatusCode}): {body}");
            }
        }

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path)
        {
            var request = new HttpRequestMessage(method, path);
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.TryAddWithoutValidation("Authorization", authHeader);
            }

            return request;
        }
    }

    public class AccountClientDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
