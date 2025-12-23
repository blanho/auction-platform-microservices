using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Application.Services;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class EmailRequestedConsumer : IConsumer<EmailRequestedEvent>
{
    private readonly INotificationSenderFactory _senderFactory;
    private readonly ILogger<EmailRequestedConsumer> _logger;

    public EmailRequestedConsumer(
        INotificationSenderFactory senderFactory,
        ILogger<EmailRequestedConsumer> logger)
    {
        _senderFactory = senderFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmailRequestedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "Processing EmailRequestedEvent: {TemplateName} to {Email} from {Source}",
            message.TemplateName,
            message.RecipientEmail,
            message.Source);

        try
        {
            var emailSender = _senderFactory.GetSender(ChannelType.Email);
            var request = new SendRequest
            {
                Channel = ChannelType.Email,
                RecipientEmail = message.RecipientEmail,
                Subject = message.Subject,
                HtmlBody = message.HtmlBody,
                Body = message.HtmlBody
            };

            var result = await emailSender.SendAsync(request, context.CancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Email sent successfully to {Email}, ExternalId: {ExternalId}",
                    message.RecipientEmail,
                    result.ExternalId);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to send email to {Email}: {Error}",
                    message.RecipientEmail,
                    result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing EmailRequestedEvent for {Email}", message.RecipientEmail);
            throw;
        }
    }
}
