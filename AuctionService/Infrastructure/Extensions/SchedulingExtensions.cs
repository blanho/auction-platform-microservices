using AuctionService.Infrastructure.Jobs;
using Common.Scheduling.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuctionService.Infrastructure.Extensions;

public static class SchedulingExtensions
{
    /// <summary>
    /// Adds Quartz.NET scheduled jobs for auction lifecycle management
    /// </summary>
    public static IServiceCollection AddAuctionScheduling(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScheduling(configuration, q =>
        {
            // Check for finished auctions - runs every 5 seconds
            // Critical for timely auction completion processing
            q.AddIntervalJob<CheckAuctionFinishedJob>(
                interval: TimeSpan.FromSeconds(5),
                jobId: CheckAuctionFinishedJob.JobId,
                description: CheckAuctionFinishedJob.Description,
                runOnStartup: true);

            // Auto-deactivate expired auctions - runs every minute
            // Updates status to Finished, ReserveNotMet, or Inactive
            q.AddIntervalJob<AuctionDeactivationJob>(
                interval: TimeSpan.FromMinutes(1),
                jobId: AuctionDeactivationJob.JobId,
                description: AuctionDeactivationJob.Description,
                runOnStartup: false);

            // Activate scheduled auctions - runs every 30 seconds
            // Changes Scheduled auctions to Live when start time arrives
            q.AddIntervalJob<AuctionActivationJob>(
                interval: TimeSpan.FromSeconds(30),
                jobId: AuctionActivationJob.JobId,
                description: AuctionActivationJob.Description,
                runOnStartup: true);

            // Auction ending soon notifications - runs every minute
            // Notifies watchers at 1 hour, 15 min, 5 min before end
            q.AddIntervalJob<AuctionEndingSoonNotificationJob>(
                interval: TimeSpan.FromMinutes(1),
                jobId: AuctionEndingSoonNotificationJob.JobId,
                description: AuctionEndingSoonNotificationJob.Description,
                runOnStartup: false);
        });

        return services;
    }
}
