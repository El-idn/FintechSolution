using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using NotificationService.DTOs;
using NotificationService.Services;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email notification.
        /// </summary>
        /// <param name="request">Email notification request</param>
        /// <returns>Email notification response</returns>
        [HttpPost("email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendEmailAsync(request);
                _logger.LogInformation("Email notification sent successfully to {Email} for user {UserId}", request.UserEmail, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification to {Email} for user {UserId}", request.UserEmail, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the email notification." });
            }
        }

        /// <summary>
        /// Sends an SMS notification.
        /// </summary>
        /// <param name="request">SMS notification request</param>
        /// <returns>SMS notification response</returns>
        [HttpPost("sms")]
        public async Task<IActionResult> SendSms([FromBody] SmsNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendSmsAsync(request);
                _logger.LogInformation("SMS notification sent successfully to {Phone} for user {UserId}", request.PhoneNumber, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS notification to {Phone} for user {UserId}", request.PhoneNumber, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the SMS notification." });
            }
        }

        /// <summary>
        /// Sends a push notification.
        /// </summary>
        /// <param name="request">Push notification request</param>
        /// <returns>Push notification response</returns>
        [HttpPost("push")]
        public async Task<IActionResult> SendPush([FromBody] PushNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendPushAsync(request);
                _logger.LogInformation("Push notification sent successfully to device {DeviceToken} for user {UserId}", request.DeviceToken, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send push notification to device {DeviceToken} for user {UserId}", request.DeviceToken, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the push notification." });
            }
        }

        /// <summary>
        /// Sends an in-app notification.
        /// </summary>
        /// <param name="request">In-app notification request</param>
        /// <returns>In-app notification response</returns>
        [HttpPost("in-app")]
        public async Task<IActionResult> SendInApp([FromBody] InAppNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendInAppAsync(request);
                _logger.LogInformation("In-app notification sent successfully for user {UserId}", request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send in-app notification for user {UserId}", request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the in-app notification." });
            }
        }

        /// <summary>
        /// Sends a PSD2 Strong Customer Authentication notification.
        /// </summary>
        /// <param name="request">PSD2 SCA notification request</param>
        /// <returns>PSD2 SCA notification response</returns>
        [HttpPost("psd2-sca")]
        public async Task<IActionResult> SendPSD2SCA([FromBody] PSD2SCANotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendPSD2SCAAsync(request);
                _logger.LogInformation("PSD2 SCA notification sent successfully for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send PSD2 SCA notification for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the PSD2 SCA notification." });
            }
        }

        /// <summary>
        /// Sends an Open Banking consent notification.
        /// </summary>
        /// <param name="request">Open Banking consent notification request</param>
        /// <returns>Open Banking consent notification response</returns>
        [HttpPost("open-banking-consent")]
        public async Task<IActionResult> SendOpenBankingConsent([FromBody] OpenBankingConsentNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendOpenBankingConsentAsync(request);
                _logger.LogInformation("Open Banking consent notification sent successfully for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Open Banking consent notification for consent {ConsentId} to user {UserId}", request.ObnConsentId, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the Open Banking consent notification." });
            }
        }

        /// <summary>
        /// Sends a transaction notification.
        /// </summary>
        /// <param name="request">Transaction notification request</param>
        /// <returns>Transaction notification response</returns>
        [HttpPost("transaction")]
        public async Task<IActionResult> SendTransaction([FromBody] TransactionNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendTransactionAsync(request);
                _logger.LogInformation("Transaction notification sent successfully for transaction {TransactionId} to user {UserId}", request.TransactionId, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send transaction notification for transaction {TransactionId} to user {UserId}", request.TransactionId, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the transaction notification." });
            }
        }

        /// <summary>
        /// Sends a security notification.
        /// </summary>
        /// <param name="request">Security notification request</param>
        /// <returns>Security notification response</returns>
        [HttpPost("security")]
        public async Task<IActionResult> SendSecurity([FromBody] SecurityNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendSecurityAsync(request);
                _logger.LogInformation("Security notification sent successfully for event {EventType} to user {UserId}", request.SecurityEventType, request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send security notification for event {EventType} to user {UserId}", request.SecurityEventType, request.UserId);
                return StatusCode(500, new { error = "An error occurred while sending the security notification." });
            }
        }

        /// <summary>
        /// Sends multiple notifications in bulk.
        /// </summary>
        /// <param name="request">Bulk notification request</param>
        /// <returns>Bulk notification response</returns>
        [HttpPost("bulk")]
        public async Task<IActionResult> SendBulk([FromBody] BulkNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "NotificationService ModelState error", details = ModelState });
            }

            try
            {
                var response = await _notificationService.SendBulkAsync(request);
                _logger.LogInformation("Bulk notifications completed for batch {BatchId}: {SuccessCount} success, {FailureCount} failed", response.BatchId, response.SuccessCount, response.FailureCount);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk notifications for batch {BatchId}", request.BatchId);
                return StatusCode(500, new { error = "An error occurred while sending the bulk notifications." });
            }
        }

        /// <summary>
        /// Gets notification statistics for a specific date.
        /// </summary>
        /// <param name="date">Date for statistics (format: yyyy-MM-dd)</param>
        /// <returns>Notification statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics([FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var statisticsDate))
            {
                return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd format." });
            }

            try
            {
                var response = await _notificationService.GetStatisticsAsync(statisticsDate);
                _logger.LogInformation("Retrieved notification statistics for date {Date}", date);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve notification statistics for date {Date}", date);
                return StatusCode(500, new { error = "An error occurred while retrieving notification statistics." });
            }
        }

        /// <summary>
        /// Gets notification statistics for today.
        /// </summary>
        /// <returns>Today's notification statistics</returns>
        [HttpGet("statistics/today")]
        public async Task<IActionResult> GetTodayStatistics()
        {
            try
            {
                var response = await _notificationService.GetStatisticsAsync(DateTime.Today);
                _logger.LogInformation("Retrieved today's notification statistics");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve today's notification statistics");
                return StatusCode(500, new { error = "An error occurred while retrieving today's notification statistics." });
            }
        }

        /// <summary>
        /// Health check endpoint for the notification service.
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "NotificationService",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                OpenBankingSupport = true,
                SupportedTypes = new[] { "EMAIL", "SMS", "PUSH", "IN_APP", "PSD2_SCA", "OPEN_BANKING_CONSENT", "TRANSACTION", "SECURITY" }
            });
        }

        /// <summary>
        /// Gets Open Banking notification capabilities.
        /// </summary>
        /// <returns>Open Banking capabilities</returns>
        [HttpGet("open-banking/capabilities")]
        public IActionResult GetOpenBankingCapabilities()
        {
            return Ok(new
            {
                PSD2Compliance = true,
                SCAMethods = new[] { "SMS", "EMAIL", "APP", "BIOMETRIC" },
                NotificationTypes = new[]
                {
                    "PSD2_SCA_CHALLENGE",
                    "PSD2_SCA_AUTHORIZATION",
                    "OPEN_BANKING_CONSENT_GRANTED",
                    "OPEN_BANKING_CONSENT_REVOKED",
                    "OPEN_BANKING_CONSENT_EXPIRED",
                    "TRANSACTION_CREATED",
                    "TRANSACTION_AUTHORIZED",
                    "TRANSACTION_COMPLETED",
                    "SECURITY_ALERT",
                    "FRAUD_DETECTION"
                },
                SupportedRegulations = new[] { "PSD2", "GDPR", "AML" },
                RealTimeEvents = true,
                EventDrivenArchitecture = true
            });
        }
    }
}
