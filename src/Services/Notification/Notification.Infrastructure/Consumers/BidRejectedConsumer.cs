using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

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

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BidderId.ToString(),
                Type = NotificationType.BidRejected,
                Title = "Bid Rejected",
                Message = $"Your bid of {FormatCurrency(@event.Amount)} was rejected. Reason: {@event.Reason}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["BidId"] = @event.BidId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2"),
                    ["Reason"] = @event.Reason
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            context.CancellationToken);
    }

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";
}
