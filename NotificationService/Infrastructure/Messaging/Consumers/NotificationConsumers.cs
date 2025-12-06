using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers
{
    public class AuctionCreatedConsumer : IConsumer<AuctionCreatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionCreatedConsumer> _logger;

        public AuctionCreatedConsumer(INotificationService notificationService, ILogger<AuctionCreatedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionCreatedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionCreatedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Seller,
                    Type = NotificationType.AuctionCreated,
                    Title = "Auction Created",
                    Message = $"Your auction '{context.Message.Make} {context.Message.Model}' has been created successfully.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.Make,
                        context.Message.Model,
                        context.Message.Year,
                        context.Message.ReservePrice,
                        context.Message.AuctionEnd
                    })
                };

                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionCreatedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }

    public class AuctionFinishedConsumer : IConsumer<AuctionFinishedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionFinishedConsumer> _logger;

        public AuctionFinishedConsumer(INotificationService notificationService, ILogger<AuctionFinishedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionFinishedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionFinishedEvent for auction {AuctionId}", context.Message.AuctionId);

            try
            {
                // Notify seller
                var sellerNotification = new CreateNotificationDto
                {
                    UserId = context.Message.Seller,
                    Type = NotificationType.AuctionFinished,
                    Title = "Auction Ended",
                    Message = context.Message.ItemSold
                        ? $"Your auction has ended! Final bid: ${context.Message.SoldAmount} by {context.Message.Winner}"
                        : "Your auction has ended with no bids or reserve price not met.",
                    AuctionId = context.Message.AuctionId,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.ItemSold,
                        context.Message.SoldAmount,
                        context.Message.Winner
                    })
                };
                await _notificationService.CreateNotificationAsync(sellerNotification);

                // Notify winner if item sold
                if (context.Message.ItemSold && !string.IsNullOrEmpty(context.Message.Winner))
                {
                    var winnerNotification = new CreateNotificationDto
                    {
                        UserId = context.Message.Winner,
                        Type = NotificationType.AuctionWon,
                        Title = "Congratulations! You Won!",
                        Message = $"You won the auction with a bid of ${context.Message.SoldAmount}",
                        AuctionId = context.Message.AuctionId,
                        Data = JsonSerializer.Serialize(new
                        {
                            context.Message.SoldAmount,
                            context.Message.ItemSold
                        })
                    };
                    await _notificationService.CreateNotificationAsync(winnerNotification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionFinishedEvent for auction {AuctionId}", context.Message.AuctionId);
            }
        }
    }

    public class BidPlacedConsumer : IConsumer<BidPlacedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<BidPlacedConsumer> _logger;

        public BidPlacedConsumer(INotificationService notificationService, ILogger<BidPlacedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BidPlacedEvent> context)
        {
            _logger.LogInformation("Consuming BidPlacedEvent for auction {AuctionId}", context.Message.AuctionId);

            try
            {
                // Create notification for bid placed
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Bidder,
                    Type = NotificationType.BidPlaced,
                    Title = "Bid Placed Successfully",
                    Message = $"Your bid of ${context.Message.BidAmount} has been placed with status: {context.Message.BidStatus}",
                    AuctionId = context.Message.AuctionId,
                    BidId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.BidAmount,
                        context.Message.Bidder,
                        context.Message.BidStatus,
                        context.Message.BidTime
                    })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing BidPlacedEvent for auction {AuctionId}", context.Message.AuctionId);
            }
        }
    }

    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdatedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionUpdatedConsumer> _logger;

        public AuctionUpdatedConsumer(INotificationService notificationService, ILogger<AuctionUpdatedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionUpdatedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionUpdatedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Seller,
                    Type = NotificationType.AuctionUpdated,
                    Title = "Auction Updated",
                    Message = $"Your auction has been updated successfully.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new
                    {
                        context.Message.Title,
                        context.Message.Description,
                        context.Message.Make,
                        context.Message.Model,
                        context.Message.Year,
                        context.Message.Color,
                        context.Message.Mileage
                    })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionUpdatedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }

    public class AuctionDeletedConsumer : IConsumer<AuctionDeletedEvent>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuctionDeletedConsumer> _logger;

        public AuctionDeletedConsumer(INotificationService notificationService, ILogger<AuctionDeletedConsumer> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuctionDeletedEvent> context)
        {
            _logger.LogInformation("Consuming AuctionDeletedEvent for auction {AuctionId}", context.Message.Id);

            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = context.Message.Seller,
                    Type = NotificationType.AuctionDeleted,
                    Title = "Auction Deleted",
                    Message = "Your auction has been deleted.",
                    AuctionId = context.Message.Id,
                    Data = JsonSerializer.Serialize(new { DeletedAt = DateTime.UtcNow })
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing AuctionDeletedEvent for auction {AuctionId}", context.Message.Id);
            }
        }
    }
}
