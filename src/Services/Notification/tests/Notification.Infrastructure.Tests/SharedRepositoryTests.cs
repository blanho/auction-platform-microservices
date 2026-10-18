using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Microsoft.EntityFrameworkCore;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using TestSupport;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Tests;

public sealed class SharedRepositoryTests
{
    private static readonly Guid Actor = Guid.NewGuid();
    private static IAuditContext Audit => TestProxy.Create<IAuditContext>((method, _) =>
        method.Name == "get_UserId" ? Actor : null);

    [PostgresFact]
    public async Task NotificationsStageAuditedWritesAndPreservePhysicalDeletion()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<NotificationDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new NotificationDbContext(options);
        await db.Database.MigrateAsync();
        INotificationRepository repository = new NotificationRepository(db, new DateTimeProvider(), Audit);
        var notification = NotificationEntity.Create("user", "username", NotificationType.General, "title", "message");
        await repository.CreateAsync(notification);
        notification.MarkAsSent();
        notification.Archive();
        await repository.UpdateAsync(notification);
        Assert.Equal(EntityState.Added, db.Entry(notification).State);
        Assert.Equal(Actor, notification.CreatedBy);
        Assert.Equal(Actor, notification.UpdatedBy);
        await using var observer = new NotificationDbContext(options);
        Assert.Empty(await observer.Notifications.ToListAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.NotNull(await repository.GetByIdAsync(notification.Id));
        Assert.Empty(db.ChangeTracker.Entries());
        await repository.DeleteAsync(notification.Id);
        Assert.True(await observer.Notifications.AsNoTracking().AnyAsync());
        await db.SaveChangesAsync();
        Assert.Empty(await observer.Notifications.IgnoreQueryFilters().ToListAsync());
    }

    [PostgresFact]
    public async Task PreferencesStageSharedAuditAndPersistChangedSettingsAtUnitOfWorkCommit()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<NotificationDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new NotificationDbContext(options);
        await db.Database.MigrateAsync();
        INotificationPreferenceRepository repository = new NotificationPreferenceRepository(db, new DateTimeProvider(), Audit);
        var preference = NotificationPreference.CreateDefault("user");
        Assert.Same(preference, await repository.CreateAsync(preference));
        await using var observer = new NotificationDbContext(options);
        Assert.Empty(await observer.NotificationPreferences.ToListAsync());
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var tracked = (await repository.GetByUserIdAsync("user"))!;
        tracked.Update(emailEnabled: false);
        await repository.UpdateAsync(tracked);
        Assert.True((await observer.NotificationPreferences.AsNoTracking().SingleAsync()).EmailEnabled);
        await db.SaveChangesAsync();
        var persisted = await observer.NotificationPreferences.AsNoTracking().SingleAsync();
        Assert.False(persisted.EmailEnabled);
        Assert.Equal(Actor, persisted.CreatedBy);
        Assert.Equal(Actor, persisted.UpdatedBy);
    }

    [PostgresFact]
    public async Task TemplatesKeepImmediateCommitsAndPhysicalDeletionAroundSharedWrites()
    {
        await using var database = await PostgresTestDatabase.CreateAsync();
        var options = new DbContextOptionsBuilder<NotificationDbContext>().UseNpgsql(database.ConnectionString).Options;
        await using var db = new NotificationDbContext(options);
        await db.Database.MigrateAsync();
        ITemplateRepository repository = new TemplateRepository(db, new DateTimeProvider(), Audit);
        var template = NotificationTemplate.Create("KEY", "name", "subject", "body");
        await repository.AddAsync(template);
        await using var observer = new NotificationDbContext(options);
        Assert.Equal(Actor, (await observer.Templates.AsNoTracking().SingleAsync()).CreatedBy);
        db.ChangeTracker.Clear();
        var detached = (await repository.GetByKeyAsync("KEY"))!;
        detached.Update("changed", "new subject", "new body");
        await repository.UpdateAsync(detached);
        var persisted = await observer.Templates.AsNoTracking().SingleAsync();
        Assert.Equal("changed", persisted.Name);
        Assert.Equal(Actor, persisted.UpdatedBy);
        await repository.DeleteAsync(detached.Id);
        Assert.Empty(await observer.Templates.IgnoreQueryFilters().ToListAsync());
        await repository.DeleteAsync(Guid.NewGuid());
    }
}
