using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Messaging.Consumers;
using Notification.Infrastructure.Persistence;
using OrchestrationService.Contracts.Events;
using TestSupport;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Tests;

public class AuctionCompletionNotificationTests
{
    [PostgresFact]
    public async Task ReplayedCommandAcknowledgesWithoutDuplicatingRecipientNotifications()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<NotificationDbContext>().UseNpgsql(database.ConnectionString).Options;
        var message = new SendAuctionCompletionNotifications
        {
            CorrelationId = Guid.NewGuid(),
            AuctionId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            WinnerId = Guid.NewGuid(),
            SellerId = Guid.NewGuid(),
            ItemTitle = "item"
        };
        var acknowledgements = new List<AuctionCompletionNotificationsSent>();
        for (var attempt = 0; attempt < 2; attempt++)
        {
            await using var db = new NotificationDbContext(options);
            if (attempt == 0) await db.Database.MigrateAsync();
            await using var tx = await db.Database.BeginTransactionAsync();
            var service = TestProxy.Create<INotificationService>((method, args) => method.Name == nameof(INotificationService.CreateNotificationAsync)
                ? Create((CreateNotificationDto)args![0]!, (CancellationToken)args[1]!) : throw new NotSupportedException(method.Name));
            async Task<NotificationDto> Create(CreateNotificationDto dto, CancellationToken ct)
            {
                db.Notifications.Add(NotificationEntity.Create(dto.UserId, dto.UserId, dto.Type, "title", "message",
                    auctionId: dto.AuctionId, referenceId: dto.ReferenceId));
                await db.SaveChangesAsync(ct);
                return new NotificationDto();
            }
            var context = TestProxy.Create<ConsumeContext<SendAuctionCompletionNotifications>>((method, args) => method.Name switch
            {
                "get_Message" => message,
                "get_CancellationToken" => CancellationToken.None,
                "Publish" => Record((AuctionCompletionNotificationsSent)args![0]!),
                _ => throw new NotSupportedException(method.Name)
            });
            Task Record(AuctionCompletionNotificationsSent acknowledgement)
            {
                acknowledgements.Add(acknowledgement); return Task.CompletedTask;
            }
            await new SendAuctionCompletionNotificationsConsumer(db, service).Consume(context);
            await tx.CommitAsync();
        }
        await using var check = new NotificationDbContext(options);
        var notifications = await check.Notifications.ToListAsync();
        Assert.Equal(2, notifications.Count);
        Assert.Contains(notifications, x => x.UserId == message.WinnerId.ToString());
        Assert.Contains(notifications, x => x.UserId == message.SellerId.ToString());
        Assert.Equal(2, acknowledgements.Count);
    }
}
