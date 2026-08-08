using Auctions.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auctions.Infrastructure.Extensions;

public static class SchedulingExtensions
{
    public static IServiceCollection AddAuctionScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            q.AddIntervalJob<CheckAuctionFinishedJob>(
                interval: TimeSpan.FromSeconds(AuctionDefaults.Scheduling.CheckFinishedIntervalSeconds),
                jobId: CheckAuctionFinishedJob.JobId,
                description: CheckAuctionFinishedJob.Description,
                runOnStartup: true);

            q.AddIntervalJob<AuctionActivationJob>(
                interval: TimeSpan.FromSeconds(AuctionDefaults.Scheduling.ActivationIntervalSeconds),
                jobId: AuctionActivationJob.JobId,
                description: AuctionActivationJob.Description,
                runOnStartup: true);

            q.AddIntervalJob<AuctionEndingSoonNotificationJob>(
                interval: TimeSpan.FromMinutes(AuctionDefaults.Scheduling.EndingSoonNotificationIntervalMinutes),
                jobId: AuctionEndingSoonNotificationJob.JobId,
                description: AuctionEndingSoonNotificationJob.Description,
                runOnStartup: false);
        });

        return services;
    }
}
