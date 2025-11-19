using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Api.Data;
using IdentityService.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Api.Jobs;

public class InactiveUserLockJob : BaseJob
{
    public const string JobId = "inactive-user-lock";
    public const string Description = "Locks user accounts that have been inactive for 30 days";
    private const int InactiveDaysThreshold = 30;

    public InactiveUserLockJob(
        ILogger<InactiveUserLockJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(
        IServiceProvider scopedProvider,
        CancellationToken cancellationToken)
    {
        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var eventPublisher = scopedProvider.GetRequiredService<IEventPublisher>();

        var thresholdDate = DateTimeOffset.UtcNow.AddDays(-InactiveDaysThreshold);

        var inactiveUsers = await dbContext.Users
            .Where(u => u.IsActive && !u.IsSuspended)
            .Where(u => u.LastLoginAt == null || u.LastLoginAt < thresholdDate)
            .Where(u => u.CreatedAt < thresholdDate)
            .ToListAsync(cancellationToken);

        if (inactiveUsers.Count == 0)
        {
            return;
        }

        Logger.LogInformation("Found {Count} inactive users to lock", inactiveUsers.Count);
        var processedCount = 0;

        foreach (var user in inactiveUsers)
        {
            try
            {
                user.IsActive = false;
                processedCount++;

                await eventPublisher.PublishAsync(new UserSuspendedEvent
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    Reason = $"Account locked due to {InactiveDaysThreshold} days of inactivity",
                    SuspendedAt = DateTimeOffset.UtcNow
                }, cancellationToken);

                Logger.LogDebug("Locked inactive user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to lock inactive user {UserId}", user.Id);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Completed locking {ProcessedCount}/{TotalCount} inactive users",
            processedCount, inactiveUsers.Count);
    }
}
