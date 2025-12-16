using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuctionFinishedConsumer> _logger;

        public AuctionFinishedConsumer(
            INotificationService notificationService, 
            IEmailService emailService,
            ILogger<AuctionFinishedConsumer> logger)
        {
            _notificationService = notificationService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionFinishedEvent for auction {AuctionId}", context.Message.AuctionId);

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionFinishedEvent for auction {AuctionId}", context.Message.AuctionId);
            }
        }
    }
}
