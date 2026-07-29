using NotificationService.DTOs;
using MassTransit;
using SharedKernel.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task<NotificationResponse> SendEmailAsync(EmailNotificationRequest request);
        Task<NotificationResponse> SendSmsAsync(SmsNotificationRequest request);
        Task<NotificationResponse> SendPushAsync(PushNotificationRequest request);
        Task<NotificationResponse> SendInAppAsync(InAppNotificationRequest request);
        Task<PSD2SCANotificationResponse> SendPSD2SCAAsync(PSD2SCANotificationRequest request);
        Task<OpenBankingConsentNotificationResponse> SendOpenBankingConsentAsync(OpenBankingConsentNotificationRequest request);
        Task<TransactionNotificationResponse> SendTransactionAsync(TransactionNotificationRequest request);
        Task<SecurityNotificationResponse> SendSecurityAsync(SecurityNotificationRequest request);
        Task<BulkNotificationResponse> SendBulkAsync(BulkNotificationRequest request);
        Task<NotificationStatisticsResponse> GetStatisticsAsync(DateTime date);
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IConfiguration _configuration;

        public NotificationService(
            ILogger<NotificationService> logger,
            IPublishEndpoint publishEndpoint,
            IConfiguration configuration)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _configuration = configuration;
        }

        public async Task<NotificationResponse> SendEmailAsync(EmailNotificationRequest request)
        {
            _logger.LogInformation("Sending email notification to {Email} for user {UserId}", request.UserEmail, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new EmailNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = "EMAIL",
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                MessageId = $"email_{notificationId}",
                DeliveryConfirmation = "DELIVERED",
                DeliveredAt = DateTime.UtcNow
            };

            // Simulate email sending
            await Task.Delay(100);

            // Publish Open Banking event
            await PublishNotificationSentEvent(response, request);

            _logger.LogInformation("Email notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<NotificationResponse> SendSmsAsync(SmsNotificationRequest request)
        {
            _logger.LogInformation("Sending SMS notification to {Phone} for user {UserId}", request.PhoneNumber, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new SmsNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = "SMS",
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                MessageId = $"sms_{notificationId}",
                DeliveryStatus = "DELIVERED",
                DeliveredAt = DateTime.UtcNow,
                Carrier = "SIMULATED"
            };

            // Simulate SMS sending
            await Task.Delay(200);

            // Publish Open Banking event
            await PublishNotificationSentEvent(response, request);

            _logger.LogInformation("SMS notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<NotificationResponse> SendPushAsync(PushNotificationRequest request)
        {
            _logger.LogInformation("Sending push notification to {DeviceToken} for user {UserId}", request.DeviceToken, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new PushNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = "PUSH",
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                DeviceToken = request.DeviceToken,
                Platform = request.Platform,
                DeliveryStatus = "DELIVERED",
                DeliveredAt = DateTime.UtcNow
            };

            // Simulate push notification sending
            await Task.Delay(150);

            // Publish Open Banking event
            await PublishNotificationSentEvent(response, request);

            _logger.LogInformation("Push notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<NotificationResponse> SendInAppAsync(InAppNotificationRequest request)
        {
            _logger.LogInformation("Sending in-app notification for user {UserId}", request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new InAppNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = "IN_APP",
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                Category = request.Category,
                RequiresAcknowledgment = request.RequiresAcknowledgment,
                ExpiresAt = request.ExpiresAt,
                IsRead = false
            };

            // Simulate in-app notification sending
            await Task.Delay(50);

            // Publish Open Banking event
            await PublishNotificationSentEvent(response, request);

            _logger.LogInformation("In-app notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<PSD2SCANotificationResponse> SendPSD2SCAAsync(PSD2SCANotificationRequest request)
        {
            _logger.LogInformation("Sending PSD2 SCA notification for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new PSD2SCANotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = request.NotificationType,
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = true,
                ObnClientName = request.ObnClientName,
                SCAMethod = request.SCAMethod,
                TransactionType = request.TransactionType,
                AuthorizationCode = request.AuthorizationCode,
                ExpiresAt = request.ExpiresAt
            };

            // Simulate PSD2 SCA notification sending
            await Task.Delay(300);

            // Publish PSD2 SCA notification event
            await PublishPSD2SCANotificationEvent(response, request);

            _logger.LogInformation("PSD2 SCA notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<OpenBankingConsentNotificationResponse> SendOpenBankingConsentAsync(OpenBankingConsentNotificationRequest request)
        {
            _logger.LogInformation("Sending Open Banking consent notification for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new OpenBankingConsentNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = request.NotificationType,
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = true,
                ObnClientName = request.ObnClientName,
                Permissions = request.Permissions,
                AccountIds = request.AccountIds,
                ExpiresAt = request.ExpiresAt,
                ActionRequired = request.ActionRequired
            };

            // Simulate Open Banking consent notification sending
            await Task.Delay(250);

            // Publish Open Banking consent notification event
            await PublishOpenBankingConsentNotificationEvent(response, request);

            _logger.LogInformation("Open Banking consent notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<TransactionNotificationResponse> SendTransactionAsync(TransactionNotificationRequest request)
        {
            _logger.LogInformation("Sending transaction notification for transaction {TransactionId} to user {UserId}", request.TransactionId, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new TransactionNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = request.NotificationType,
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                TransactionId = request.TransactionId,
                TransactionType = request.TransactionType,
                TransactionStatus = request.TransactionStatus,
                Amount = request.Amount,
                Currency = request.Currency,
                AccountId = request.AccountId,
                CounterpartyAccount = request.CounterpartyAccount,
                Reference = request.Reference
            };

            // Simulate transaction notification sending
            await Task.Delay(180);

            // Publish transaction notification event
            await PublishTransactionNotificationEvent(response, request);

            _logger.LogInformation("Transaction notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<SecurityNotificationResponse> SendSecurityAsync(SecurityNotificationRequest request)
        {
            _logger.LogInformation("Sending security notification for event {EventType} to user {UserId}", request.SecurityEventType, request.UserId);

            var notificationId = Guid.NewGuid();
            var response = new SecurityNotificationResponse
            {
                NotificationId = notificationId,
                UserId = request.UserId,
                UserEmail = request.UserEmail,
                NotificationType = request.NotificationType,
                Status = "SENT",
                SentAt = DateTime.UtcNow,
                ObnClientId = request.ObnClientId,
                ObnConsentId = request.ObnConsentId,
                IsOpenBankingNotification = request.IsOpenBankingNotification,
                SecurityEventType = request.SecurityEventType,
                RiskLevel = request.RiskLevel,
                IPAddress = request.IPAddress,
                Location = request.Location,
                RequiresAction = request.RequiresAction,
                ActionRequired = request.ActionRequired
            };

            // Simulate security notification sending
            await Task.Delay(120);

            // Publish security notification event
            await PublishSecurityNotificationEvent(response, request);

            _logger.LogInformation("Security notification sent successfully: {NotificationId}", notificationId);
            return response;
        }

        public async Task<BulkNotificationResponse> SendBulkAsync(BulkNotificationRequest request)
        {
            _logger.LogInformation("Sending bulk notifications for batch {BatchId} with {Count} notifications", request.BatchId, request.Notifications.Count);

            var batchId = request.BatchId ?? Guid.NewGuid().ToString();
            var startedAt = DateTime.UtcNow;
            var results = new List<NotificationResponse>();
            var successCount = 0;
            var failureCount = 0;

            if (request.SendInParallel)
            {
                var maxConcurrency = request.MaxConcurrency ?? 10;
                var semaphore = new SemaphoreSlim(maxConcurrency);

                var tasks = request.Notifications.Select(async notification =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var result = await SendNotificationAsync(notification);
                        results.Add(result);
                        if (result.Status == "SENT" || result.Status == "DELIVERED")
                            Interlocked.Increment(ref successCount);
                        else
                            Interlocked.Increment(ref failureCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification in bulk batch {BatchId}", batchId);
                        Interlocked.Increment(ref failureCount);
                        results.Add(new NotificationResponse
                        {
                            NotificationId = Guid.NewGuid(),
                            UserId = notification.UserId,
                            UserEmail = notification.UserEmail,
                            NotificationType = notification.NotificationType,
                            Status = "FAILED",
                            SentAt = DateTime.UtcNow,
                            ErrorMessage = ex.Message
                        });
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }
            else
            {
                foreach (var notification in request.Notifications)
                {
                    try
                    {
                        var result = await SendNotificationAsync(notification);
                        results.Add(result);
                        if (result.Status == "SENT" || result.Status == "DELIVERED")
                            successCount++;
                        else
                            failureCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send notification in bulk batch {BatchId}", batchId);
                        failureCount++;
                        results.Add(new NotificationResponse
                        {
                            NotificationId = Guid.NewGuid(),
                            UserId = notification.UserId,
                            UserEmail = notification.UserEmail,
                            NotificationType = notification.NotificationType,
                            Status = "FAILED",
                            SentAt = DateTime.UtcNow,
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            var completedAt = DateTime.UtcNow;
            var response = new BulkNotificationResponse
            {
                BatchId = batchId,
                Results = results,
                TotalCount = request.Notifications.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                StartedAt = startedAt,
                CompletedAt = completedAt
            };

            _logger.LogInformation("Bulk notifications completed for batch {BatchId}: {SuccessCount} success, {FailureCount} failed", batchId, successCount, failureCount);
            return response;
        }

        public async Task<NotificationStatisticsResponse> GetStatisticsAsync(DateTime date)
        {
            _logger.LogInformation("Getting notification statistics for date {Date}", date.ToString("yyyy-MM-dd"));

            // Simulate statistics retrieval
            await Task.Delay(100);

            var response = new NotificationStatisticsResponse
            {
                Date = date,
                TotalSent = 150,
                TotalDelivered = 142,
                TotalFailed = 8,
                ByType = new Dictionary<string, int>
                {
                    { "EMAIL", 80 },
                    { "SMS", 45 },
                    { "PUSH", 15 },
                    { "IN_APP", 10 }
                },
                ByStatus = new Dictionary<string, int>
                {
                    { "SENT", 150 },
                    { "DELIVERED", 142 },
                    { "FAILED", 8 }
                },
                OpenBankingNotifications = 25,
                PSD2SCANotifications = 12
            };

            return response;
        }

        private async Task<NotificationResponse> SendNotificationAsync(NotificationRequest request)
        {
            return request switch
            {
                EmailNotificationRequest emailRequest => await SendEmailAsync(emailRequest),
                SmsNotificationRequest smsRequest => await SendSmsAsync(smsRequest),
                PushNotificationRequest pushRequest => await SendPushAsync(pushRequest),
                InAppNotificationRequest inAppRequest => await SendInAppAsync(inAppRequest),
                PSD2SCANotificationRequest scaRequest => await SendPSD2SCAAsync(scaRequest),
                OpenBankingConsentNotificationRequest consentRequest => await SendOpenBankingConsentAsync(consentRequest),
                TransactionNotificationRequest transactionRequest => await SendTransactionAsync(transactionRequest),
                SecurityNotificationRequest securityRequest => await SendSecurityAsync(securityRequest),
                _ => throw new ArgumentException($"Unsupported notification type: {request.GetType().Name}")
            };
        }

        // Event Publishing Methods
        private async Task PublishNotificationSentEvent(NotificationResponse response, NotificationRequest request)
        {
            var @event = new NotificationSentEvent
            {
                NotificationId = response.NotificationId,
                UserId = response.UserId,
                UserEmail = response.UserEmail,
                NotificationType = response.NotificationType,
                Subject = request.Subject,
                Content = request.Content,
                SentAt = response.SentAt,
                ObnClientId = response.ObnClientId,
                ObnConsentId = response.ObnConsentId,
                IsOpenBankingNotification = response.IsOpenBankingNotification,
                DeliveryStatus = response.Status
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published NotificationSentEvent for Notification: {NotificationId}", response.NotificationId);
        }

        private async Task PublishPSD2SCANotificationEvent(PSD2SCANotificationResponse response, PSD2SCANotificationRequest request)
        {
            var @event = new PSD2SCANotificationEvent
            {
                NotificationId = response.NotificationId,
                UserId = response.UserId,
                UserEmail = response.UserEmail,
                ObnConsentId = response.ObnConsentId ?? string.Empty,
                ObnClientId = response.ObnClientId ?? string.Empty,
                ObnClientName = response.ObnClientName ?? string.Empty,
                SCAMethod = response.SCAMethod,
                TransactionType = response.TransactionType,
                NotificationType = response.NotificationType,
                SentAt = response.SentAt,
                ChallengeData = request.ChallengeData,
                AuthorizationCode = response.AuthorizationCode,
                ExpiresAt = response.ExpiresAt
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published PSD2SCANotificationEvent for Notification: {NotificationId}", response.NotificationId);
        }

        private async Task PublishOpenBankingConsentNotificationEvent(OpenBankingConsentNotificationResponse response, OpenBankingConsentNotificationRequest request)
        {
            var @event = new OpenBankingConsentNotificationEvent
            {
                NotificationId = response.NotificationId,
                UserId = response.UserId,
                UserEmail = response.UserEmail,
                ObnConsentId = response.ObnConsentId ?? string.Empty,
                ObnClientId = response.ObnClientId ?? string.Empty,
                ObnClientName = response.ObnClientName ?? string.Empty,
                NotificationType = response.NotificationType ?? string.Empty,
                Permissions = response.Permissions ?? Array.Empty<string>(),
                AccountIds = response.AccountIds ?? Array.Empty<string>(),
                SentAt = response.SentAt,
                ExpiresAt = response.ExpiresAt,
                ActionRequired = response.ActionRequired
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published OpenBankingConsentNotificationEvent for Notification: {NotificationId}", response.NotificationId);
        }

        private async Task PublishTransactionNotificationEvent(TransactionNotificationResponse response, TransactionNotificationRequest request)
        {
            var @event = new TransactionNotificationEvent
            {
                NotificationId = response.NotificationId,
                UserId = response.UserId,
                UserEmail = response.UserEmail,
                TransactionId = response.TransactionId,
                TransactionType = response.TransactionType,
                TransactionStatus = response.TransactionStatus,
                Amount = response.Amount,
                Currency = response.Currency,
                NotificationType = response.NotificationType,
                SentAt = response.SentAt,
                ObnClientId = response.ObnClientId,
                ObnConsentId = response.ObnConsentId,
                AccountId = response.AccountId,
                CounterpartyAccount = response.CounterpartyAccount,
                Reference = response.Reference
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published TransactionNotificationEvent for Notification: {NotificationId}", response.NotificationId);
        }

        private async Task PublishSecurityNotificationEvent(SecurityNotificationResponse response, SecurityNotificationRequest request)
        {
            var @event = new SecurityNotificationEvent
            {
                NotificationId = response.NotificationId,
                UserId = response.UserId,
                UserEmail = response.UserEmail,
                SecurityEventType = response.SecurityEventType,
                RiskLevel = response.RiskLevel,
                NotificationType = response.NotificationType,
                SentAt = response.SentAt,
                IPAddress = response.IPAddress,
                UserAgent = request.UserAgent,
                Location = response.Location,
                ObnClientId = response.ObnClientId,
                RequiresAction = response.RequiresAction,
                ActionRequired = response.ActionRequired
            };

            await _publishEndpoint.Publish(@event);
            _logger.LogDebug("Published SecurityNotificationEvent for Notification: {NotificationId}", response.NotificationId);
        }
    }
} 