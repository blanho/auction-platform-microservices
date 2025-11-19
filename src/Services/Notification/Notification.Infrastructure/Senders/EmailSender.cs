using Microsoft.Extensions.Logging;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Senders;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(to))
        {
            return new EmailSendResult(false, Error: "Recipient email is required");
        }

        if (string.IsNullOrEmpty(subject))
        {
            return new EmailSendResult(false, Error: "Subject is required");
        }

        try
        {

            _logger.LogInformation(
                "Sending email to {To}, Subject: {Subject}",
                MaskEmail(to),
                subject);

            await Task.Delay(10, ct);

            var messageId = Guid.NewGuid().ToString();

            _logger.LogInformation(
                "Email sent successfully to {To}. MessageId: {MessageId}",
                MaskEmail(to),
                messageId);

            return new EmailSendResult(true, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", MaskEmail(to));
            return new EmailSendResult(false, Error: ex.Message);
        }
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        var maskedLocal = localPart.Length > 2
            ? localPart[..2] + new string('*', Math.Min(localPart.Length - 2, 4))
            : localPart;

        return $"{maskedLocal}@{domain}";
    }
}
