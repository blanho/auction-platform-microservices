using BidService.Contracts.Events;
using MassTransit;
using Notification.Application.DTOs;
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

        _logger.LogInformation(
            "Processing Outbid event: {OutbidUser} was outbid by {NewBidder} on auction {AuctionId}",
            @event.OutbidBidderUsername, @event.NewHighBidderUsername, @event.AuctionId);

        await _notificationService.CreateNotificationAsync(
            new CreateNotificationDto
            {
                UserId = @event.OutbidBidderId.ToString(),
                Type = NotificationType.BidOutbid,
                Title = "You've Been Outbid!",
                Message = $"Your bid of {FormatCurrency(@event.PreviousBidAmount)} has been outbid. New high bid: {FormatCurrency(@event.NewHighBidAmount)}",
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

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";
}
