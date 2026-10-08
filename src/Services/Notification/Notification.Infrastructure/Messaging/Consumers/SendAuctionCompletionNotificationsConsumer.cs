using Notification.Application.DTOs;
using Notification.Application.Helpers;
using Notification.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;

namespace Notification.Infrastructure.Messaging.Consumers;

public class SendAuctionCompletionNotificationsConsumer(NotificationDbContext db, INotificationService notifications)
    : IConsumer<SendAuctionCompletionNotifications>
{
    public async Task Consume(ConsumeContext<SendAuctionCompletionNotifications> context)
    {
        var m = context.Message;
        var ct = context.CancellationToken;
        if (db.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Saga notifications require a transactional consumer outbox");
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({m.OrderId.ToString()}, 0))", ct);
        foreach (var userId in new[] { m.WinnerId, m.SellerId }.Distinct())
        {
            var reference = $"auction-completion:{m.OrderId}:{userId}";
            if (await db.Notifications.AnyAsync(n => n.ReferenceId == reference, ct)) continue;
            await notifications.CreateNotificationAsync(new CreateNotificationDto
            {
                UserId = userId.ToString(),
                Type = NotificationType.AuctionFinished,
                AuctionId = m.AuctionId,
                ReferenceId = reference,
                LocalizedText = new(NotificationMessageKeys.AuctionCompletedTitle,
                    NotificationMessageKeys.AuctionCompletedMessage, m.ItemTitle),
                Data = NotificationDataBuilder.Create().Add("OrderId", m.OrderId).Add("AuctionId", m.AuctionId).Build()
            }, ct);
        }
        await context.Publish(new AuctionCompletionNotificationsSent
        {
            CorrelationId = m.CorrelationId,
            AuctionId = m.AuctionId,
            SentAt = DateTimeOffset.UtcNow
        });
    }
}
