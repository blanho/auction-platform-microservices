using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.Interfaces;
using NotificationService.Contracts.Enums;

namespace Notification.Infrastructure.Consumers;

public class BidAcceptedConsumer : IConsumer<BidAcceptedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<BidAcceptedConsumer> _logger;

    public BidAcceptedConsumer(
        INotificationService notificationService,
        ILogger<BidAcceptedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidAcceptedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing BidAccepted event for bid {BidId} by {Bidder}",
            @event.BidId, @event.BidderUsername);

        await _notificationService.SendNotificationAsync(
            userId: @event.BidderId,
            type: NotificationType.BidAccepted,
            title: "Bid Accepted",
            message: $"Congratulations! Your bid of {FormatCurrency(@event.Amount)} has been accepted.",
            data: new Dictionary<string, string>
            {
                ["AuctionId"] = @event.AuctionId.ToString(),
                ["BidId"] = @event.BidId.ToString(),
                ["Amount"] = @event.Amount.ToString("F2")
            },
            channels: NotificationChannel.Email | NotificationChannel.Push | NotificationChannel.InApp,
            cancellationToken: context.CancellationToken);
    }

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";
}
