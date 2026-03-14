using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Notification.Infrastructure.Senders;

public class SendGridEmailSender : IEmailSender
{
    private readonly ISendGridClient _client;
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(
        ISendGridClient client,
        IOptions<SendGridOptions> options,
        ILogger<SendGridEmailSender> logger)
    {
        _client = client;
        _options = options.Value;
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
            return new EmailSendResult(false, Error: "Recipient email is required");

        if (string.IsNullOrEmpty(subject))
            return new EmailSendResult(false, Error: "Subject is required");

        try
        {
            var from = new EmailAddress(_options.FromEmail, _options.FromName);
            var toAddress = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(
                from,
                toAddress,
                subject,
                plainTextBody ?? TemplateHelper.StripHtml(htmlBody),
                htmlBody);

            msg.SetClickTracking(_options.EnableClickTracking, _options.EnableClickTracking);
            msg.SetOpenTracking(_options.EnableOpenTracking);

            msg.AddHeader("X-Entity-Ref-ID", Guid.NewGuid().ToString());

            if (!string.IsNullOrEmpty(_options.DefaultCategory))
            {
                msg.AddCategory(_options.DefaultCategory);
            }

            _logger.LogDebug(
                "Sending email via SendGrid to {To}, Subject: {Subject}",
                EmailHelper.MaskEmail(to),
                subject);

            var response = await _client.SendEmailAsync(msg, ct);

            if (response.IsSuccessStatusCode)
            {

                var messageId = response.Headers.TryGetValues("X-Message-Id", out var values)
                    ? values.FirstOrDefault()
                    : null;

                _logger.LogInformation(
                    "Email sent successfully via SendGrid to {To}. MessageId: {MessageId}, StatusCode: {StatusCode}",
                    EmailHelper.MaskEmail(to),
                    messageId,
                    response.StatusCode);

                return new EmailSendResult(true, messageId);
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "SendGrid returned non-success status: {StatusCode}, Body: {Body}",
                    response.StatusCode,
                    body);

                return new EmailSendResult(false, Error: $"SendGrid error: {response.StatusCode} - {body}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SendGrid to {To}", EmailHelper.MaskEmail(to));
            return new EmailSendResult(false, Error: ex.Message);
        }
    }
}
