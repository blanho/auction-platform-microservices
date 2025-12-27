using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Ports;
using NotificationService.Application.UseCases.SendNotification;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Models;
using NotificationService.Domain.Rules;

namespace NotificationService.Application.Services;

public class NotificationOrchestrator : INotificationOrchestrator
{
    private readonly ITemplateRepository _templateRepository;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly INotificationSenderFactory _senderFactory;
    private readonly ILogger<NotificationOrchestrator> _logger;

    public NotificationOrchestrator(
        ITemplateRepository templateRepository,
        ITemplateRenderer templateRenderer,
        INotificationSenderFactory senderFactory,
        ILogger<NotificationOrchestrator> logger)
    {
        _templateRepository = templateRepository;
        _templateRenderer = templateRenderer;
        _senderFactory = senderFactory;
        _logger = logger;
    }

    public async Task<OrchestrationResult> OrchestrateAsync(
        OrchestrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = new OrchestrationResult
        {
            ChannelResults = new List<ChannelResult>()
        };

        var channelList = GetActiveChannels(request.Channels);

        string? title = null;
        string? plainBody = null;
        string? htmlBody = null;

        foreach (var channel in channelList)
        {
            try
            {
                var template = await _templateRepository.GetTemplateAsync(
                    request.NotificationType,
                    channel,
                    null,
                    cancellationToken);

                if (template == null)
                {
                    template = await _templateRepository.GetTemplateAsync(
                        request.NotificationType,
                        ChannelType.InApp,
                        null,
                        cancellationToken);
                }

                if (template == null)
                {
                    _logger.LogWarning(
                        "No template found for {Type}/{Channel}, using fallback",
                        request.NotificationType, channel);

                    var fallbackContent = CreateFallbackContent(request);
                    title ??= fallbackContent.Title;
                    plainBody ??= fallbackContent.Body;

                    result.ChannelResults.Add(new ChannelResult(
                        channel, true, "Used fallback template"));
                    continue;
                }

                var rendered = await _templateRenderer.RenderAsync(
                    template,
                    request.TemplateData,
                    cancellationToken);

                title ??= rendered.Subject;
                plainBody ??= rendered.Body;
                if (channel == ChannelType.Email && rendered.HtmlBody != null)
                {
                    htmlBody = rendered.HtmlBody;
                }

                var channelResult = await SendToChannelAsync(
                    channel,
                    request,
                    rendered,
                    cancellationToken);

                result.ChannelResults.Add(channelResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send to channel {Channel}", channel);
                result.ChannelResults.Add(new ChannelResult(channel, false, ex.Message));
            }
        }

        return result with
        {
            Title = title ?? "Notification",
            PlainTextBody = plainBody ?? string.Empty,
            HtmlBody = htmlBody
        };
    }

    private async Task<ChannelResult> SendToChannelAsync(
        ChannelType channel,
        OrchestrationRequest request,
        RenderedContent content,
        CancellationToken cancellationToken)
    {
        if (channel == ChannelType.InApp)
        {
            return new ChannelResult(channel, true);
        }

        var sender = _senderFactory.GetSender(channel);
        if (sender == null)
        {
            _logger.LogWarning("No sender configured for channel {Channel}", channel);
            return new ChannelResult(channel, false, "No sender configured");
        }

        var sendRequest = new SendRequest
        {
            Channel = channel,
            RecipientEmail = request.Recipient.Email,
            RecipientPhone = request.Recipient.PhoneNumber,
            DeviceToken = request.Recipient.DeviceToken,
            Subject = content.Subject,
            Body = content.Body,
            HtmlBody = content.HtmlBody,
            Data = content.Metadata
        };

        var sendResult = await sender.SendAsync(sendRequest, cancellationToken);

        if (!sendResult.Success)
        {
            _logger.LogWarning(
                "Failed to send {Channel} notification: {Error}",
                channel, sendResult.ErrorMessage);
        }

        return new ChannelResult(channel, sendResult.Success, sendResult.ErrorMessage);
    }

    private static List<ChannelType> GetActiveChannels(ChannelType channels)
    {
        var result = new List<ChannelType>();

        if (channels.HasFlag(ChannelType.InApp))
            result.Add(ChannelType.InApp);
        if (channels.HasFlag(ChannelType.Email))
            result.Add(ChannelType.Email);
        if (channels.HasFlag(ChannelType.Sms))
            result.Add(ChannelType.Sms);
        if (channels.HasFlag(ChannelType.Push))
            result.Add(ChannelType.Push);

        return result;
    }

    private static (string Title, string Body) CreateFallbackContent(OrchestrationRequest request)
    {
        return request.NotificationType switch
        {
            NotificationType.AuctionCreated => ("New Auction", "A new auction has been created."),
            NotificationType.AuctionStarted => ("Auction Started", "An auction you're watching has started."),
            NotificationType.AuctionEnding => ("Auction Ending Soon", "An auction you're watching is ending soon."),
            NotificationType.AuctionWon => ("Congratulations!", "You won the auction!"),
            NotificationType.AuctionLost => ("Auction Ended", "The auction has ended."),
            NotificationType.Outbid => ("You've Been Outbid", "Someone placed a higher bid."),
            NotificationType.BidPlaced => ("Bid Placed", "Your bid has been placed successfully."),
            NotificationType.BidRejected => ("Bid Rejected", "Your bid could not be processed."),
            NotificationType.PaymentReceived => ("Payment Received", "Your payment has been received."),
            NotificationType.PaymentCompleted => ("Payment Completed", "Payment has been completed."),
            NotificationType.OrderShipped => ("Order Shipped", "Your order has been shipped."),
            NotificationType.OrderDelivered => ("Order Delivered", "Your order has been delivered."),
            NotificationType.System => ("System Notification", "You have a new notification."),
            _ => ("Notification", "You have a new notification.")
        };
    }
}
