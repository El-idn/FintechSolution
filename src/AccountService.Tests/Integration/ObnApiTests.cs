using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

public class ObnApiTests
{
    private static readonly string AuthUrl = "http://localhost:5258/api/auth";
    private static readonly string TransactionUrl = "http://localhost:5050/api/transactions";
    private static readonly string NotificationUrl = "http://localhost:5132/api/notifications";
    private static readonly string AccountUrl = "http://localhost:5018/api/accounts";
    private static readonly HttpClient Client = new HttpClient();

    private static string UniqueEmail() => $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}@example.com";

    private static async Task<string> RegisterAndLoginAndGetToken(string email)
    {
        var registerPayload = new
        {
            email = email,
            password = "Admin123@Secure",
            confirmPassword = "Admin123@Secure",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123",
            obnClientName = "FintechApp"
        };
        var regResp = await Client.PostAsJsonAsync($"http://localhost:5258/api/auth/register", registerPayload);
        if (!regResp.IsSuccessStatusCode)
        {
            var error = await regResp.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {regResp.StatusCode} - {error}");
        }
        // Removed test-only email confirmation call
        var loginPayload = new
        {
            email = email,
            password = "Admin123@Secure",
            ipAddress = "127.0.0.1",
            userAgent = "xunit",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123"
        };
        var loginResp = await Client.PostAsJsonAsync($"http://localhost:5258/api/auth/login", loginPayload);
        if (!loginResp.IsSuccessStatusCode)
        {
            var error = await loginResp.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {loginResp.StatusCode} - {error}");
        }
        var loginJson = JObject.Parse(await loginResp.Content.ReadAsStringAsync());
        return loginJson["token"]?.ToString() ?? string.Empty;
    }

    [Fact]
    public async Task Auth_Register_And_Login_Should_Return_ObnFields()
    {
        var email = UniqueEmail();
        var registerPayload = new
        {
            email = email,
            password = "StrongP@ssw0rd!",
            confirmPassword = "StrongP@ssw0rd!",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123",
            obnClientName = "FintechApp"
        };

        // Register
        var regResp = await Client.PostAsJsonAsync($"{AuthUrl}/register", registerPayload);
        regResp.EnsureSuccessStatusCode();
        var regJson = JObject.Parse(await regResp.Content.ReadAsStringAsync());
        regJson.SelectToken("user.obnClientId").Should().NotBeNull();
        regJson.SelectToken("user.obnConsentId").Should().NotBeNull();

        // Login
        var loginPayload = new
        {
            email = email,
            password = "StrongP@ssw0rd!",
            ipAddress = "127.0.0.1",
            userAgent = "xunit",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123"
        };
        var loginResp = await Client.PostAsJsonAsync($"{AuthUrl}/login", loginPayload);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = JObject.Parse(await loginResp.Content.ReadAsStringAsync());
        loginJson["token"].Should().NotBeNull();
        loginJson["refreshToken"].Should().NotBeNull();
    }

    // Add DTOs for test payloads
    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        Transfer = 2
    }
    public class TransactionRequestDto
    {
        [JsonPropertyName("accountId")]
        public Guid AccountId { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("type")]
        public TransactionType Type { get; set; } // Use enum, not string!
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("obnConsentId")]
        public string ObnConsentId { get; set; }
        [JsonPropertyName("obnClientId")]
        public string ObnClientId { get; set; }
        [JsonPropertyName("obnClientName")]
        public string ObnClientName { get; set; }
    }
    public class TransferRequestDto
    {
        [JsonPropertyName("fromAccountId")]
        public Guid FromAccountId { get; set; }
        [JsonPropertyName("toAccountId")]
        public Guid ToAccountId { get; set; }
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("obnConsentId")]
        public string ObnConsentId { get; set; }
        [JsonPropertyName("obnClientId")]
        public string ObnClientId { get; set; }
        [JsonPropertyName("obnClientName")]
        public string ObnClientName { get; set; }
    }

    [Fact]
    public async Task Transaction_Deposit_And_Transfer_Should_Return_ObnFields()
    {
        var token = await RegisterAndLoginAndGetToken(UniqueEmail());
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Create first account
        var accountPayload1 = new
        {
            AccountType = "Savings",
            InitialDeposit = 1000.00m
        };
        var accResp1 = await Client.PostAsJsonAsync($"{AccountUrl}", accountPayload1);
        var accBody1 = await accResp1.Content.ReadAsStringAsync();
        if (!accResp1.IsSuccessStatusCode)
        {
            throw new Exception($"Account creation failed: Status: {accResp1.StatusCode}, Body: {accBody1}");
        }
        var accJson1 = JObject.Parse(accBody1);
        if (accJson1["id"] == null)
        {
            throw new Exception($"Account creation response missing 'id': {accBody1}");
        }
        var accountId = accJson1["id"].ToString();
        await Task.Delay(100); // Ensure account is committed
        // Verify account exists
        var verifyResp1 = await Client.GetAsync($"{AccountUrl}/{accountId}");
        var verifyBody1 = await verifyResp1.Content.ReadAsStringAsync();
        if (!verifyResp1.IsSuccessStatusCode)
        {
            throw new Exception($"Account {accountId} was not found after creation. Status: {verifyResp1.StatusCode}, Body: {verifyBody1}");
        }

        // Create second account
        var accountPayload2 = new
        {
            AccountType = "CurrentAccount",
            InitialDeposit = 500.00m
        };
        var accResp2 = await Client.PostAsJsonAsync($"{AccountUrl}", accountPayload2);
        var accBody2 = await accResp2.Content.ReadAsStringAsync();
        if (!accResp2.IsSuccessStatusCode)
        {
            throw new Exception($"Account creation failed: Status: {accResp2.StatusCode}, Body: {accBody2}");
        }
        var accJson2 = JObject.Parse(accBody2);
        if (accJson2["id"] == null)
        {
            throw new Exception($"Account creation response missing 'id': {accBody2}");
        }
        var toAccountId = accJson2["id"].ToString();
        await Task.Delay(100); // Ensure account is committed
        // Verify account exists
        var verifyResp2 = await Client.GetAsync($"{AccountUrl}/{toAccountId}");
        var verifyBody2 = await verifyResp2.Content.ReadAsStringAsync();
        if (!verifyResp2.IsSuccessStatusCode)
        {
            throw new Exception($"Account {toAccountId} was not found after creation. Status: {verifyResp2.StatusCode}, Body: {verifyBody2}");
        }

        // Deposit
        var depositPayload = new TransactionRequestDto
        {
            AccountId = Guid.Parse(accountId),
            Amount = 1000.00m,
            Type = TransactionType.Deposit, // Use enum value
            Description = "Initial deposit",
            ObnConsentId = "consent-abc-123",
            ObnClientId = "fintech-app-001",
            ObnClientName = "FintechApp"
        };
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            // Do NOT add JsonStringEnumConverter here!
        };
        var json = JsonSerializer.Serialize(depositPayload, options);
        Console.WriteLine($"Deposit JSON: {json}"); // Debug print
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var depResp = await Client.PostAsync($"{TransactionUrl}", content);
        var depBody = await depResp.Content.ReadAsStringAsync();
        if (!depResp.IsSuccessStatusCode)
        {
            throw new Exception($"Deposit failed: Status: {depResp.StatusCode}, Body: {depBody}");
        }
        var depJson = JObject.Parse(depBody);
        if (depJson["obnClientId"] == null || depJson["obnConsentId"] == null)
        {
            throw new Exception($"Deposit response missing OBN fields: {depBody}");
        }
        depJson["obnClientId"].Should().NotBeNull();
        depJson["obnConsentId"].Should().NotBeNull();

        // Transfer
        var transferPayload = new TransferRequestDto
        {
            FromAccountId = Guid.Parse(accountId),
            ToAccountId = Guid.Parse(toAccountId),
            Amount = 100.00m,
            Description = "Test transfer",
            ObnConsentId = "consent-abc-123",
            ObnClientId = "fintech-app-001",
            ObnClientName = "FintechApp"
        };
        var transResp = await Client.PostAsJsonAsync($"{TransactionUrl}/transfer", transferPayload);
        var transBody = await transResp.Content.ReadAsStringAsync();
        if (!transResp.IsSuccessStatusCode)
        {
            throw new Exception($"Transfer failed: Status: {transResp.StatusCode}, Body: {transBody}");
        }
        var transJson = JObject.Parse(transBody);
        if (transJson["obnClientId"] == null || transJson["obnConsentId"] == null)
        {
            throw new Exception($"Transfer response missing OBN fields: {transBody}");
        }
        transJson["obnClientId"].Should().NotBeNull();
        transJson["obnConsentId"].Should().NotBeNull();
    }

    [Fact]
    public async Task Notification_Email_Should_Return_ObnFields()
    {
        var token = await RegisterAndLoginAndGetToken(UniqueEmail());
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var email = UniqueEmail();
        var regResp = await Client.PostAsJsonAsync($"http://localhost:5258/api/auth/register", new { email = email, password = "Admin123@Secure", confirmPassword = "Admin123@Secure", obnClientId = "fintech-app-001", obnConsentId = "consent-abc-123", obnClientName = "FintechApp" });
        var regBody = await regResp.Content.ReadAsStringAsync();
        if (!regResp.IsSuccessStatusCode)
        {
            throw new Exception($"Registration failed: Status: {regResp.StatusCode}, Body: {regBody}");
        }
        var regJson = JObject.Parse(regBody);
        if (regJson["user"] == null || regJson["user"]["id"] == null)
        {
            throw new Exception($"Registration response missing user id: {regBody}");
        }
        var userId = regJson["user"]["id"].ToString();
        var userEmail = email;

        var payload = new
        {
            userId = Guid.Parse(userId),
            userEmail = userEmail,
            notificationType = "EMAIL",
            subject = "Welcome to FintechApp",
            content = "Thank you for joining!",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123",
            isOpenBankingNotification = true
        };
        var resp = await Client.PostAsJsonAsync($"{NotificationUrl}/email", payload);
        var rawBody = await resp.Content.ReadAsStringAsync();
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        Console.WriteLine($"Notification_Email_Should_Return_ObnFields: Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        if (resp.IsSuccessStatusCode && contentType != null && contentType.Contains("application/json"))
        {
            var json = JObject.Parse(rawBody);
            if (json["obnClientId"] == null || json["obnConsentId"] == null)
            {
                throw new Exception($"Notification email response missing OBN fields: {rawBody}");
            }
            json["obnClientId"].Should().NotBeNull();
            json["obnConsentId"].Should().NotBeNull();
        }
        else
        {
            throw new Exception($"Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        }
    }

    [Fact]
    public async Task Notification_Sca_Should_Return_ObnFields()
    {
        var token = await RegisterAndLoginAndGetToken(UniqueEmail());
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var email = UniqueEmail();
        var regResp = await Client.PostAsJsonAsync($"http://localhost:5258/api/auth/register", new { email = email, password = "Admin123@Secure", confirmPassword = "Admin123@Secure", obnClientId = "fintech-app-001", obnConsentId = "consent-abc-123", obnClientName = "FintechApp" });
        var regBody = await regResp.Content.ReadAsStringAsync();
        if (!regResp.IsSuccessStatusCode)
        {
            throw new Exception($"Registration failed: Status: {regResp.StatusCode}, Body: {regBody}");
        }
        var regJson = JObject.Parse(regBody);
        if (regJson["user"] == null || regJson["user"]["id"] == null)
        {
            throw new Exception($"Registration response missing user id: {regBody}");
        }
        var userId = regJson["user"]["id"].ToString();
        var userEmail = email;

        var payload = new
        {
            userId = Guid.Parse(userId),
            userEmail = userEmail,
            notificationType = "SCA",
            subject = "SCA Challenge",
            content = "Please complete SCA.",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123",
            obnClientName = "FintechApp",
            sCAMethod = "SMS",
            transactionType = "PAYMENT",
            expiresAt = DateTime.UtcNow.AddMinutes(10),
            isOpenBankingNotification = true
        };
        var resp = await Client.PostAsJsonAsync($"{NotificationUrl}/psd2-sca", payload);
        var rawBody = await resp.Content.ReadAsStringAsync();
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        Console.WriteLine($"Notification_Sca_Should_Return_ObnFields: Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        if (resp.IsSuccessStatusCode && contentType != null && contentType.Contains("application/json"))
        {
            var json = JObject.Parse(rawBody);
            if (json["obnClientId"] == null || json["obnConsentId"] == null)
            {
                throw new Exception($"Notification SCA response missing OBN fields: {rawBody}");
            }
            json["obnClientId"].Should().NotBeNull();
            json["obnConsentId"].Should().NotBeNull();
        }
        else
        {
            throw new Exception($"Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        }
    }

    [Fact]
    public async Task Notification_Consent_Should_Return_ObnFields()
    {
        var token = await RegisterAndLoginAndGetToken(UniqueEmail());
        Client.DefaultRequestHeaders.Clear();
        Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var email = UniqueEmail();
        var regResp = await Client.PostAsJsonAsync($"http://localhost:5258/api/auth/register", new { email = email, password = "Admin123@Secure", confirmPassword = "Admin123@Secure", obnClientId = "fintech-app-001", obnConsentId = "consent-abc-123", obnClientName = "FintechApp" });
        var regBody = await regResp.Content.ReadAsStringAsync();
        if (!regResp.IsSuccessStatusCode)
        {
            throw new Exception($"Registration failed: Status: {regResp.StatusCode}, Body: {regBody}");
        }
        var regJson = JObject.Parse(regBody);
        if (regJson["user"] == null || regJson["user"]["id"] == null)
        {
            throw new Exception($"Registration response missing user id: {regBody}");
        }
        var userId = regJson["user"]["id"].ToString();
        var userEmail = email;

        // Use real account IDs for AccountIds
        var accountPayload1 = new { AccountType = "Savings", InitialDeposit = 1000.00m };
        var accResp1 = await Client.PostAsJsonAsync($"{AccountUrl}", accountPayload1);
        var accBody1 = await accResp1.Content.ReadAsStringAsync();
        if (!accResp1.IsSuccessStatusCode)
        {
            throw new Exception($"Account creation failed: Status: {accResp1.StatusCode}, Body: {accBody1}");
        }
        var accJson1 = JObject.Parse(accBody1);
        if (accJson1["id"] == null)
        {
            throw new Exception($"Account creation response missing 'id': {accBody1}");
        }
        var accountId1 = accJson1["id"].ToString();
        var accountPayload2 = new { AccountType = "CurrentAccount", InitialDeposit = 500.00m };
        var accResp2 = await Client.PostAsJsonAsync($"{AccountUrl}", accountPayload2);
        var accBody2 = await accResp2.Content.ReadAsStringAsync();
        if (!accResp2.IsSuccessStatusCode)
        {
            throw new Exception($"Account creation failed: Status: {accResp2.StatusCode}, Body: {accBody2}");
        }
        var accJson2 = JObject.Parse(accBody2);
        if (accJson2["id"] == null)
        {
            throw new Exception($"Account creation response missing 'id': {accBody2}");
        }
        var accountId2 = accJson2["id"].ToString();

        var payload = new
        {
            userId = Guid.Parse(userId),
            userEmail = userEmail,
            notificationType = "GRANTED",
            subject = "Consent Granted",
            content = "You have granted consent.",
            obnClientId = "fintech-app-001",
            obnConsentId = "consent-abc-123",
            obnClientName = "FintechApp",
            permissions = new[] { "ACCOUNTS_READ", "TRANSACTIONS_READ" },
            accountIds = new[] { accountId1, accountId2 },
            expiresAt = DateTime.UtcNow.AddMonths(6),
            isOpenBankingNotification = true
        };
        var resp = await Client.PostAsJsonAsync($"{NotificationUrl}/open-banking-consent", payload);
        var rawBody = await resp.Content.ReadAsStringAsync();
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        Console.WriteLine($"Notification_Consent_Should_Return_ObnFields: Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        if (resp.IsSuccessStatusCode && contentType != null && contentType.Contains("application/json"))
        {
            var json = JObject.Parse(rawBody);
            if (json["obnClientId"] == null || json["obnConsentId"] == null)
            {
                throw new Exception($"Notification consent response missing OBN fields: {rawBody}");
            }
            json["obnClientId"].Should().NotBeNull();
            json["obnConsentId"].Should().NotBeNull();
        }
        else
        {
            throw new Exception($"Status: {resp.StatusCode}, Content-Type: {contentType}, Body: {rawBody}");
        }
    }
} 