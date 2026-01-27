using Microsoft.EntityFrameworkCore;
using MassTransit;
using BuildingBlocks.Infrastructure.Repository.Converters;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> Templates { get; set; }
    public DbSet<NotificationRecord> Records { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetUtcConverter>();

        configurationBuilder.Properties<DateTimeOffset?>()
            .HaveConversion<NullableDateTimeOffsetUtcConverter>();
    }
}
