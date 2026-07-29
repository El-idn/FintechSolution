using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WalletService.Clients
{
    public interface IAccountClient
    {
        Task<IReadOnlyList<AccountClientDto>> GetMyAccountsAsync(CancellationToken cancellationToken = default);
        Task UpdateBalanceAsync(Guid accountId, decimal newBalance, string changeReason, CancellationToken cancellationToken = default);
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

        public async Task<IReadOnlyList<AccountClientDto>> GetMyAccountsAsync(CancellationToken cancellationToken = default)
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Get, "api/accounts/mine");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var accounts = await response.Content.ReadFromJsonAsync<List<AccountClientDto>>(JsonOptions, cancellationToken);
            return accounts ?? [];
        }

        public async Task UpdateBalanceAsync(Guid accountId, decimal newBalance, string changeReason, CancellationToken cancellationToken = default)
        {
            using var request = CreateAuthorizedRequest(HttpMethod.Put, $"api/accounts/{accountId}/balance");
            request.Content = JsonContent.Create(new
            {
                NewBalance = newBalance,
                ChangeReason = changeReason
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
