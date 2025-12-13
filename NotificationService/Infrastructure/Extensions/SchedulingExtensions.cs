using Common.Scheduling.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Infrastructure.Jobs;

namespace NotificationService.Infrastructure.Extensions;

public static class SchedulingExtensions
{
    public static IServiceCollection AddNotificationScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            q.AddCronJob<NotificationCleanupJob>(
                cronExpression: "0 0 3 * * ?",
                jobId: "notification-cleanup",
                description: "Removes old read notifications"
            );
        });

        return services;
    }
}
