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
                interval: TimeSpan.FromSeconds(5),
                jobId: CheckAuctionFinishedJob.JobId,
                description: CheckAuctionFinishedJob.Description,
                runOnStartup: true);

            q.AddIntervalJob<AuctionDeactivationJob>(
                interval: TimeSpan.FromMinutes(1),
                jobId: AuctionDeactivationJob.JobId,
                description: AuctionDeactivationJob.Description,
                runOnStartup: false);

            q.AddIntervalJob<AuctionActivationJob>(
                interval: TimeSpan.FromSeconds(30),
                jobId: AuctionActivationJob.JobId,
                description: AuctionActivationJob.Description,
                runOnStartup: true);

            q.AddIntervalJob<AuctionEndingSoonNotificationJob>(
                interval: TimeSpan.FromMinutes(1),
                jobId: AuctionEndingSoonNotificationJob.JobId,
                description: AuctionEndingSoonNotificationJob.Description,
                runOnStartup: false);
        });

        return services;
    }
}

