using Common.Scheduling.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.Jobs;

public class NotificationCleanupJob : BaseJob
{
    private const int RetentionDays = 30;
    private const int BatchSize = 1000;

    public NotificationCleanupJob(
        ILogger<NotificationCleanupJob> logger,
        IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    protected override async Task ExecuteJobAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var notificationRepository = scopedProvider.GetRequiredService<INotificationRepository>();
        var unitOfWork = scopedProvider.GetRequiredService<IUnitOfWork>();

        var totalDeleted = 0;
        var hasMore = true;

        while (hasMore && !cancellationToken.IsCancellationRequested)
        {
            var oldNotifications = await notificationRepository.GetOldReadNotificationsAsync(
                RetentionDays,
                cancellationToken);

            if (oldNotifications.Count == 0)
            {
                hasMore = false;
                continue;
            }

            var batch = oldNotifications.Take(BatchSize).ToList();
            
            await notificationRepository.DeleteRangeAsync(batch, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            totalDeleted += batch.Count;

            Logger.LogInformation(
                "Deleted batch of {BatchCount} old read notifications. Total deleted so far: {TotalDeleted}",
                batch.Count,
                totalDeleted);

            hasMore = oldNotifications.Count > BatchSize;
        }

        if (totalDeleted > 0)
        {
            Logger.LogInformation(
                "Notification cleanup completed. Total notifications deleted: {TotalDeleted}",
                totalDeleted);
        }
        else
        {
            Logger.LogInformation("No old read notifications found for cleanup");
        }
    }
}
