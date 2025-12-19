using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Senders;

public class SmtpEmailSenderOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool EnableSsl { get; set; } = false;
    public string FromAddress { get; set; } = "noreply@auction.com";
    public string FromName { get; set; } = "Auction Platform";
}

public class SmtpEmailSender : INotificationSender
{
    private readonly SmtpEmailSenderOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public ChannelType Channel => ChannelType.Email;

    public SmtpEmailSender(
        IOptions<SmtpEmailSenderOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SendResult> SendAsync(SendRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.RecipientEmail))
        {
            return SendResult.Failed(ChannelType.Email, "No email address provided");
        }

        try
        {
            using var client = CreateSmtpClient();
            var message = CreateMailMessage(request);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent to {Recipient}", request.RecipientEmail);
            return SendResult.Succeeded(ChannelType.Email);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Recipient}", request.RecipientEmail);
            return SendResult.Failed(ChannelType.Email, ex.Message, IsPermanentError(ex));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipient}", request.RecipientEmail);
            return SendResult.Failed(ChannelType.Email, ex.Message);
        }
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        return client;
    }

    private MailMessage CreateMailMessage(SendRequest request)
    {
        var from = new MailAddress(_options.FromAddress, _options.FromName);
        var to = new MailAddress(request.RecipientEmail!);

        var message = new MailMessage(from, to)
        {
            Subject = request.Subject,
            IsBodyHtml = !string.IsNullOrEmpty(request.HtmlBody)
        };

        if (!string.IsNullOrEmpty(request.HtmlBody))
        {
            message.Body = request.HtmlBody;
            
            if (!string.IsNullOrEmpty(request.Body))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(
                    request.Body,
                    null,
                    "text/plain");
                message.AlternateViews.Add(plainView);
            }
        }
        else
        {
            message.Body = request.Body;
        }

        return message;
    }

    private static bool IsPermanentError(SmtpException ex)
    {
        return ex.StatusCode switch
        {
            SmtpStatusCode.MailboxUnavailable => true,
            SmtpStatusCode.UserNotLocalWillForward => true,
            SmtpStatusCode.MailboxBusy => false,
            SmtpStatusCode.InsufficientStorage => false,
            SmtpStatusCode.ServiceNotAvailable => false,
            _ => false
        };
    }
}
