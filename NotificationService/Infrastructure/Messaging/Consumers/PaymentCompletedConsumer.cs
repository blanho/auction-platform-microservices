using MassTransit;
using Microsoft.Extensions.Logging;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using Common.Messaging.Events;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging.Consumers;

public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        INotificationService notificationService,
        IEmailService emailService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consuming PaymentCompletedEvent for order {OrderId}", message.OrderId);

        try
        {
            var buyerNotification = new CreateNotificationDto
            {
                UserId = message.BuyerUsername,
                Type = NotificationType.OrderCreated,
                Title = "Payment Confirmed",
                Message = $"Your payment of ${message.Amount:N2} has been confirmed. The seller will ship your item soon.",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.OrderId,
                    message.Amount,
                    message.TransactionId
                })
            };
            await _notificationService.CreateNotificationAsync(buyerNotification);

            var sellerNotification = new CreateNotificationDto
            {
                UserId = message.SellerUsername,
                Type = NotificationType.OrderCreated,
                Title = "Payment Received",
                Message = $"Payment of ${message.Amount:N2} received from {message.BuyerUsername}. Please ship the item.",
                AuctionId = message.AuctionId,
                Data = JsonSerializer.Serialize(new
                {
                    message.OrderId,
                    message.Amount,
                    message.BuyerUsername
                })
            };
            await _notificationService.CreateNotificationAsync(sellerNotification);

            _logger.LogInformation("Sent payment completed notifications for order {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentCompletedEvent for order {OrderId}", message.OrderId);
        }
    }
}
