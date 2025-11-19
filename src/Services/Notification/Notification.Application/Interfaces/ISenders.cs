namespace Notification.Application.Interfaces;

public interface IEmailSender
{

    Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default);
}

public interface ISmsSender
{

    Task<SmsSendResult> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken ct = default);
}

public interface IPushSender
{

    Task<PushSendResult> SendAsync(
        string userId,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        CancellationToken ct = default);
}

public record EmailSendResult(bool Success, string? MessageId = null, string? Error = null);

public record SmsSendResult(bool Success, string? MessageId = null, string? Error = null);

public record PushSendResult(bool Success, string? MessageId = null, string? Error = null);
