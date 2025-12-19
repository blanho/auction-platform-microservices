using NotificationService.Domain.Enums;

namespace NotificationService.Application.UseCases.SendNotification;

public record SendNotificationRequest
{
    public required string RecipientId { get; init; }
    public required string RecipientUsername { get; init; }
    public string? RecipientEmail { get; init; }
    public string? RecipientPhone { get; init; }
    public string? DeviceToken { get; init; }
    public required NotificationType NotificationType { get; init; }
    public ChannelType? Channels { get; init; }
    public Dictionary<string, object> TemplateData { get; init; } = new();
    public string? IdempotencyKey { get; init; }
    public Guid? AuctionId { get; init; }
    public Guid? BidId { get; init; }
    public Guid? OrderId { get; init; }
    public string? ReferenceId { get; init; }
}

public record SendNotificationResponse
{
    public Guid NotificationId { get; init; }
    public bool Success { get; init; }
    public List<ChannelResult> ChannelResults { get; init; } = new();
    public string? ErrorMessage { get; init; }
    public bool WasAlreadyProcessed { get; init; }

    public static SendNotificationResponse Succeeded(Guid notificationId, List<ChannelResult> results)
        => new() { NotificationId = notificationId, Success = true, ChannelResults = results };

    public static SendNotificationResponse Failed(string error)
        => new() { Success = false, ErrorMessage = error };

    public static SendNotificationResponse AlreadyProcessed(Guid notificationId)
        => new() { NotificationId = notificationId, Success = true, WasAlreadyProcessed = true };
}

public record ChannelResult(ChannelType Channel, bool Success, string? ErrorMessage = null);
