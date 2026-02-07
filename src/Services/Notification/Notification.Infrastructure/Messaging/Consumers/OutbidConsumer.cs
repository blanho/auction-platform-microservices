using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Application.Interfaces;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Consumers;

public class OutbidConsumer : IConsumer<OutbidEvent>
{
    private readonly INotificationService _notificationService;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<OutbidConsumer> _logger;

    public OutbidConsumer(
        INotificationService notificationService,
        INotificationHubService hubService,
        ILogger<OutbidConsumer> logger)
    {
        _notificationService = notificationService;
        _hubService = hubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OutbidEvent> context)
    {
        var @event = context.Message;

        _logger.LogDebug(
            "Processing Outbid event for auction {AuctionId}",
            @event.AuctionId);

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.OutbidBidderId.ToString(),
                Type = NotificationType.BidOutbid,
                Title = "You've Been Outbid!",
                Message = $"Your bid of {NotificationFormattingHelper.FormatCurrency(@event.PreviousBidAmount)} has been outbid. New high bid: {NotificationFormattingHelper.FormatCurrency(@event.NewHighBidAmount)}",
                Data = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    ["AuctionId"] = @event.AuctionId.ToString(),
                    ["PreviousBidAmount"] = @event.PreviousBidAmount.ToString("F2"),
                    ["NewHighBidAmount"] = @event.NewHighBidAmount.ToString("F2")
                }),
                AuctionId = @event.AuctionId
            },
            context.CancellationToken);

        await _hubService.SendOutbidNotificationAsync(
            @event.OutbidBidderId.ToString(),
            new
            {
                AuctionId = @event.AuctionId,
                PreviousBidAmount = @event.PreviousBidAmount,
                NewHighBidAmount = @event.NewHighBidAmount,
                OutbidAt = @event.OutbidAt
            });
    }
}
