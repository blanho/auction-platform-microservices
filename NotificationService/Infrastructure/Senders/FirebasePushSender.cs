using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class FirebasePushSenderOptions
{
    public string ProjectId { get; set; } = string.Empty;
    public string ServerKey { get; set; } = string.Empty;
}

public class FirebasePushSender : INotificationSender
{
    private readonly FirebasePushSenderOptions _options;
    private readonly ILogger<FirebasePushSender> _logger;
    private readonly HttpClient _httpClient;

    public ChannelType Channel => ChannelType.Push;

    public FirebasePushSender(
        IOptions<FirebasePushSenderOptions> options,
        ILogger<FirebasePushSender> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("Firebase");
    }

    public async Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.DeviceToken))
        {
            return SendResult.Failed(ChannelType.Push, "No device token provided", true);
        }

        if (string.IsNullOrEmpty(_options.ServerKey))
        {
            _logger.LogWarning("Firebase server key not configured, skipping push notification");
            return SendResult.Failed(ChannelType.Push, "Firebase server key not configured");
        }

        try
        {
            var url = "https://fcm.googleapis.com/fcm/send";

            var payload = new
            {
                to = request.DeviceToken,
                notification = new
                {
                    title = request.Subject,
                    body = request.Body
                },
                data = request.Data
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload),
                Headers = { { "Authorization", $"key={_options.ServerKey}" } }
            };

            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Firebase FCM error: {StatusCode} - {Error}", response.StatusCode, error);
                return SendResult.Failed(ChannelType.Push, error);
            }

            var result = await response.Content.ReadFromJsonAsync<FcmResponse>(cancellationToken: cancellationToken);

            if (result?.Success == 0)
            {
                var errorMsg = result.Results?.FirstOrDefault()?.Error ?? "Unknown error";
                _logger.LogError("Firebase FCM delivery failed: {Error}", errorMsg);
                
                var isPermanent = errorMsg is "InvalidRegistration" or "NotRegistered";
                return SendResult.Failed(ChannelType.Push, errorMsg, isPermanent);
            }

            _logger.LogInformation("Push notification sent via Firebase to device {Token}", 
                request.DeviceToken[..Math.Min(20, request.DeviceToken.Length)] + "...");
            
            return SendResult.Succeeded(ChannelType.Push, result?.MulticastId?.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification via Firebase");
            return SendResult.Failed(ChannelType.Push, ex.Message);
        }
    }

    private record FcmResponse(
        long? MulticastId,
        int Success,
        int Failure,
        FcmResult[]? Results);

    private record FcmResult(string? MessageId, string? Error);
}
