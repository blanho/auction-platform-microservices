using BuildingBlocks.Application.Abstractions.Providers;
using BuildingBlocks.Application.Abstractions.Auditing;
using AutoMapper;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Features.Notifications.GetUserNotifications;
using Notification.Application.Features.Notifications.GetNotificationSummary;
using Notification.Application.Services;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using TestSupport;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Tests;

public sealed class NotificationPagingTests
{
    [PostgresFact]
    public async Task UnreadFilter_AppliesBeforeCountingAndPagingAndPreservesUserIsolation()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        await using var db = new NotificationDbContext(new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(database.ConnectionString).Options);
        await db.Database.MigrateAsync();
        var timestamp = DateTimeOffset.UtcNow.AddDays(-1);
        var notifications = Enumerable.Range(0, 4).Select(index =>
        {
            var notification = NotificationEntity.Create(index == 3 ? "another-user" : "user", "username",
                NotificationType.General, "title", "message");
            notification.SetCreatedAudit(Guid.Empty, timestamp);
            notification.MarkAsSent();
            if (index == 0) notification.MarkAsRead();
            return notification;
        }).ToArray();
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var mapper = TestProxy.Create<IMapper>((_, args) =>
        {
            var notification = Assert.IsType<NotificationEntity>(args![0]);
            return new NotificationDto { Id = notification.Id, UserId = notification.UserId, Status = notification.Status.ToString() };
        });
        var repository = new NotificationRepository(db, new DateTimeProvider(),
            TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : null));
        var handler = new GetUserNotificationsQueryHandler(repository, mapper, NullLogger<GetUserNotificationsQueryHandler>.Instance);
        var first = await handler.Handle(new GetUserNotificationsQuery("user", 1, 1, NotificationStatus.Unread), default);
        var second = await handler.Handle(new GetUserNotificationsQuery("user", 2, 1, NotificationStatus.Unread), default);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.NotNull(first.Value);
        Assert.NotNull(second.Value);
        Assert.Equal(2, first.Value.TotalCount);
        Assert.Equal(2, first.Value.TotalPages);
        Assert.True(first.Value.HasNextPage);
        Assert.True(second.Value.HasPreviousPage);
        Assert.Equal(notifications.Skip(1).Take(2).OrderBy(notification => notification.Id).Select(notification => notification.Id),
            first.Value.Items.Concat(second.Value.Items).Select(notification => notification.Id));
        Assert.All(first.Value.Items.Concat(second.Value.Items), notification =>
        {
            Assert.Equal("user", notification.UserId);
            Assert.Equal(nameof(NotificationStatus.Unread), notification.Status);
        });
        var normalized = await repository.GetPaginatedAsync(0, 500, "user");
        Assert.Equal(1, normalized.Page);
        Assert.Equal(100, normalized.PageSize);
        Assert.Equal(3, normalized.TotalCount);
        var extreme = await repository.GetPaginatedAsync(int.MaxValue, 100, "user");
        Assert.Empty(extreme.Items);
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [PostgresFact]
    public async Task Summary_QueriesOnlyTheRecentPageAndComputesCountsInTheDatabase()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var commands = new List<string>();
        await using var db = new NotificationDbContext(new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(database.ConnectionString).AddInterceptors(new ReadCommandRecorder(commands)).Options);
        await db.Database.MigrateAsync();
        foreach (var index in Enumerable.Range(0, 25))
        {
            var notification = NotificationEntity.Create("user", "username", NotificationType.General, "title", "message");
            notification.MarkAsSent();
            if (index < 5) notification.MarkAsRead();
            db.Notifications.Add(notification);
        }
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        commands.Clear();
        var mapper = TestProxy.Create<IMapper>((_, args) =>
        {
            var notification = Assert.IsType<NotificationEntity>(args![0]);
            return new NotificationDto { Id = notification.Id, UserId = notification.UserId };
        });
        var repository = new NotificationRepository(db, new DateTimeProvider(),
            TestProxy.Create<IAuditContext>((method, _) => method.Name == "get_UserId" ? Guid.Empty : null));
        var handler = new GetNotificationSummaryQueryHandler(repository, mapper, NullLogger<GetNotificationSummaryQueryHandler>.Instance);
        var result = await handler.Handle(new GetNotificationSummaryQuery("user"), default);
        Assert.NotNull(result.Value);
        var service = new NotificationServiceImpl(repository, null!, null!, mapper, null!, null!);
        var fromService = await service.GetNotificationSummaryAsync("user");
        Assert.Equal(25, result.Value.TotalCount);
        Assert.Equal(20, result.Value.UnreadCount);
        Assert.Equal(10, result.Value.RecentNotifications.Count);
        Assert.Equal(result.Value.TotalCount, fromService.TotalCount);
        Assert.Equal(result.Value.UnreadCount, fromService.UnreadCount);
        Assert.Equal(result.Value.RecentNotifications.Select(notification => notification.Id),
            fromService.RecentNotifications.Select(notification => notification.Id));
        Assert.Equal(2, commands.Count(sql => sql.Contains("LIMIT")));
        Assert.All(commands, sql => Assert.True(sql.Contains("count(", StringComparison.OrdinalIgnoreCase) || sql.Contains("LIMIT"), sql));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    private sealed class ReadCommandRecorder(List<string> commands) : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            commands.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
