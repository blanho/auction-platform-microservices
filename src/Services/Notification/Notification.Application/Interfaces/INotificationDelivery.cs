namespace Notification.Application.Interfaces;

public interface INotificationDelivery
{
    Task SendEmailAsync(string userId, string templateKey, string recipientEmail, string subject,
        string body, CancellationToken ct = default);
    Task SendSmsAsync(string userId, string templateKey, string phoneNumber, string message,
        CancellationToken ct = default);
    Task SendPushAsync(string userId, string templateKey, string title, string body,
        Dictionary<string, string> data, CancellationToken ct = default);
}
