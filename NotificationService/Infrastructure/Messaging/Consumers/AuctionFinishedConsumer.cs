using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class AuctionFinishedConsumer : IdempotentConsumerBase<AuctionFinishedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public AuctionFinishedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<AuctionFinishedConsumer> logger) : base(cache, logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
    }

    protected override async Task ProcessMessageAsync(ConsumeContext<AuctionFinishedEvent> context)
    {
        var sellerNotification = new CreateNotificationDto
        {
            UserId = context.Message.SellerUsername,
            Type = NotificationType.AuctionFinished,
            Title = "Auction Ended",
            Message = context.Message.ItemSold
                ? $"Your auction has ended! Final bid: ${context.Message.SoldAmount:N2} by {context.Message.WinnerUsername}"
                : "Your auction has ended with no bids or reserve price not met.",
            AuctionId = context.Message.AuctionId,
            Data = JsonSerializer.Serialize(new
            {
                context.Message.ItemSold,
                context.Message.SoldAmount,
                context.Message.WinnerUsername
            })
        };
        await _notificationService.CreateNotificationAsync(sellerNotification);

        if (context.Message.ItemSold && !string.IsNullOrEmpty(context.Message.WinnerUsername))
        {
            var winnerNotification = new CreateNotificationDto
            {
                UserId = context.Message.WinnerUsername,
                Type = NotificationType.AuctionWon,
                Title = "Congratulations! You Won!",
                Message = $"You won the auction with a bid of ${context.Message.SoldAmount:N2}",
                AuctionId = context.Message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    context.Message.SoldAmount,
                    context.Message.ItemSold
                })
            };
            await _notificationService.CreateNotificationAsync(winnerNotification);

            await _emailService.SendAuctionWonEmailAsync(
                context.Message.WinnerUsername,
                "Auction Item",
                context.Message.SoldAmount ?? 0,
                context.Message.AuctionId);
        }
    }

    protected override string GetIdempotencyKey(ConsumeContext<AuctionFinishedEvent> context)
    {
        return $"auction-finished:{context.Message.AuctionId}";
    }
}
