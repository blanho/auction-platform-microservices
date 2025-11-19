using Notification.Domain.Entities;

namespace Notification.Application.Interfaces;

public interface INotificationSender
{

    Task SendAsync(SendNotificationRequest request, CancellationToken ct = default);

    Task SendEmailAsync(string userId, string templateKey, Dictionary<string, string> data, string recipientEmail, CancellationToken ct = default);

    Task SendSmsAsync(string userId, string templateKey, Dictionary<string, string> data, string phoneNumber, CancellationToken ct = default);

    Task SendPushAsync(string userId, string templateKey, Dictionary<string, string> data, CancellationToken ct = default);

    Task SendInAppAsync(string userId, string title, string message, string? link = null, CancellationToken ct = default);

    Task<List<UserNotification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken ct = default);
}

public record SendNotificationRequest
{

    public required string UserId { get; init; }

    public required string TemplateKey { get; init; }

    public Dictionary<string, string> Data { get; init; } = new();

    public string? RecipientEmail { get; init; }

    public string? RecipientPhone { get; init; }

    public bool SendEmail { get; init; } = true;

    public bool SendSms { get; init; }

    public bool SendPush { get; init; }

    public bool SendInApp { get; init; } = true;

    public string? InAppLink { get; init; }

    public Guid? AuctionId { get; init; }

    public Guid? BidId { get; init; }

    public Guid? OrderId { get; init; }
}
