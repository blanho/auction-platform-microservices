using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

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

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.BidderId.ToString(),
                Type = NotificationType.BidAccepted,
                Title = "Bid Accepted",
                Message = $"Congratulations! Your bid of {FormatCurrency(@event.Amount)} has been accepted.",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["BidId"] = @event.BidId.ToString(),
                    ["Amount"] = @event.Amount.ToString("F2")
                }),
                AuctionId = @event.AuctionId,
                BidId = @event.BidId
            },
            context.CancellationToken);
    }

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";
}
