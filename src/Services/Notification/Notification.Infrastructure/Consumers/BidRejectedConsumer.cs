using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.Interfaces;
using NotificationService.Contracts.Enums;

namespace Notification.Infrastructure.Consumers;

public class BidRejectedConsumer : IConsumer<BidRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<BidRejectedConsumer> _logger;

    public BidRejectedConsumer(
        INotificationService notificationService,
        ILogger<BidRejectedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BidRejectedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "Processing BidRejected event for bid {BidId} by {Bidder}",
            @event.BidId, @event.BidderUsername);

        await _notificationService.SendNotificationAsync(
            userId: @event.BidderId,
            type: NotificationType.BidRejected,
            title: "Bid Rejected",
            message: $"Your bid of {FormatCurrency(@event.Amount)} was rejected. Reason: {@event.Reason}",
            data: new Dictionary<string, string>
            {
                ["AuctionId"] = @event.AuctionId.ToString(),
                ["BidId"] = @event.BidId.ToString(),
                ["Amount"] = @event.Amount.ToString("F2"),
                ["Reason"] = @event.Reason
            },
            channels: NotificationChannel.Email | NotificationChannel.Push | NotificationChannel.InApp,
            cancellationToken: context.CancellationToken);
    }

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";
}
