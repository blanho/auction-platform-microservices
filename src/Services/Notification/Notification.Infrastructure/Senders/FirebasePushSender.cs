using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;

namespace Notification.Infrastructure.Senders;

public class FirebasePushSender : IPushSender
{
    private readonly FirebaseMessaging _messaging;
    private readonly FirebaseOptions _options;
    private readonly ILogger<FirebasePushSender> _logger;

    public FirebasePushSender(
        FirebaseMessaging messaging,
        IOptions<FirebaseOptions> options,
        ILogger<FirebasePushSender> logger)
    {
        _messaging = messaging;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PushSendResult> SendAsync(
        string userId,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
            return new PushSendResult(false, Error: "User ID is required");

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(body))
            return new PushSendResult(false, Error: "Title or body is required");

        try
        {

            var topic = $"user_{userId}";

            var message = new Message
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body,
                    ImageUrl = data?.GetValueOrDefault("imageUrl")
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Title = title,
                        Body = body,
                        ClickAction = data?.GetValueOrDefault("clickAction") ?? "FLUTTER_NOTIFICATION_CLICK",
                        ChannelId = _options.DefaultAndroidChannel,
                        Sound = "default"
                    },

                    TimeToLive = TimeSpan.FromHours(_options.TimeToLiveHours)
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = title,
                            Body = body
                        },
                        Sound = "default",
                        Badge = 1,
                        ContentAvailable = true
                    }
                },
                Webpush = new WebpushConfig
                {
                    Notification = new WebpushNotification
                    {
                        Title = title,
                        Body = body,
                        Icon = _options.WebPushIcon
                    },
                    FcmOptions = new WebpushFcmOptions
                    {
                        Link = data?.GetValueOrDefault("clickAction")
                    }
                }
            };

            _logger.LogDebug(
                "Sending push notification via Firebase to topic {Topic}, Title: {Title}",
                topic,
                title);

            var response = await _messaging.SendAsync(message, ct);

            _logger.LogInformation(
                "Push notification sent successfully via Firebase. MessageId: {MessageId}, Topic: {Topic}",
                response,
                topic);

            return new PushSendResult(true, response);
        }
        catch (FirebaseMessagingException ex)
        {
            _logger.LogError(ex,
                "Firebase push notification failed: {ErrorCode}, UserId: {UserId}",
                ex.MessagingErrorCode,
                userId);

            var isPermanent = ex.MessagingErrorCode is
                MessagingErrorCode.InvalidArgument or
                MessagingErrorCode.Unregistered or
                MessagingErrorCode.SenderIdMismatch;

            return new PushSendResult(false, Error: $"{ex.MessagingErrorCode}: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to user {UserId}", userId);
            return new PushSendResult(false, Error: ex.Message);
        }
    }

    public async Task<PushSendResult> SendToDeviceAsync(
        IEnumerable<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default)
    {
        var tokens = deviceTokens.ToList();
        if (tokens.Count == 0)
            return new PushSendResult(false, Error: "No device tokens provided");

        try
        {
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            var response = await _messaging.SendEachForMulticastAsync(message, ct);

            _logger.LogInformation(
                "Push notifications sent: Success={SuccessCount}, Failure={FailureCount}",
                response.SuccessCount,
                response.FailureCount);

            for (var i = 0; i < response.Responses.Count; i++)
            {
                if (!response.Responses[i].IsSuccess)
                {
                    var error = response.Responses[i].Exception;
                    if (error?.MessagingErrorCode == MessagingErrorCode.Unregistered)
                    {
                        _logger.LogWarning(
                            "Device token unregistered, should be removed: {Token}",
                            SecurityHelper.MaskToken(tokens[i]));
                    }
                }
            }

            if (response.SuccessCount > 0)
            {
                return new PushSendResult(true, $"Sent to {response.SuccessCount}/{tokens.Count} devices");
            }

            return new PushSendResult(false, Error: $"All {tokens.Count} sends failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send multicast push notification");
            return new PushSendResult(false, Error: ex.Message);
        }
    }
}
