using NotificationService.Domain.Enums;

namespace NotificationService.Application.Ports;

public interface INotificationSender
{
    ChannelType Channel { get; }
    
    Task<SendResult> SendAsync(
        SendRequest request,
        CancellationToken cancellationToken = default);
}

public record SendRequest
{
    public ChannelType Channel { get; init; }
    public string? RecipientEmail { get; init; }
    public string? RecipientPhone { get; init; }
    public string? DeviceToken { get; init; }
    public string? Subject { get; init; }
    public string? Body { get; init; }
    public string? HtmlBody { get; init; }
    public Dictionary<string, string>? Data { get; init; }
}

public record SendResult
{
    public ChannelType Channel { get; init; }
    public bool Success { get; init; }
    public string? ExternalId { get; init; }
    public string? ErrorMessage { get; init; }
    public bool IsPermanentError { get; init; }

    public static SendResult Succeeded(ChannelType channel, string? externalId = null) 
        => new() { Channel = channel, Success = true, ExternalId = externalId };

    public static SendResult Failed(ChannelType channel, string errorMessage, bool isPermanent = false)
        => new() { Channel = channel, Success = false, ErrorMessage = errorMessage, IsPermanentError = isPermanent };
}
