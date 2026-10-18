using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Localization;
using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Infrastructure.Persistence;
using IdentityService.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Identity.Infrastructure.Jobs;

[DisallowConcurrentExecution]
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

        var options = scopedProvider.GetRequiredService<IOptions<IdentityJobOptions>>().Value;
        var now = scopedProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var thresholdDate = now.AddDays(-InactiveDaysThreshold);
        var processedCount = 0;

        for (var batch = 0; batch < options.MaxBatchesPerRun; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var inactiveUsers = await dbContext.Users
                .Where(u => u.IsActive && !u.IsSuspended)
                .Where(u => u.LastLoginAt == null || u.LastLoginAt < thresholdDate)
                .Where(u => u.CreatedAt < thresholdDate)
                .OrderBy(u => u.CreatedAt).ThenBy(u => u.Id)
                .Take(options.BatchSize)
                .ToListAsync(cancellationToken);
            if (inactiveUsers.Count == 0)
                break;

            foreach (var user in inactiveUsers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                user.IsActive = false;
                user.ConcurrencyStamp = Guid.NewGuid().ToString();
                user.SecurityStamp = Guid.NewGuid().ToString();

                await eventPublisher.PublishAsync(new UserSuspendedEvent
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    Reason = UserMessageLocalizer.FormatInvariant(
                        "Account locked due to {0} days of inactivity", [InactiveDaysThreshold.ToString()]),
                    SuspendedAt = now
                }, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            processedCount += inactiveUsers.Count;
        }

        Logger.LogInformation("Locked {Count} inactive users", processedCount);
    }
}
