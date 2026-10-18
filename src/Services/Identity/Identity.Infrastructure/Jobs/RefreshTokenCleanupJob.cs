using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Identity.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class RefreshTokenCleanupJob(
    ILogger<RefreshTokenCleanupJob> logger,
    IServiceProvider serviceProvider) : BaseJob(logger, serviceProvider)
{
    public const string JobId = "refresh-token-cleanup";
    public const string Description = "Removes expired refresh-token history after retention when no live session remains";

    protected override async Task ExecuteJobAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var options = scopedProvider.GetRequiredService<IOptions<IdentityJobOptions>>().Value;
        if (!options.RefreshTokenCleanupEnabled)
            return;

        var db = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var now = scopedProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var cutoff = now.AddDays(-options.ExpiredTokenRetentionDays);
        var eligible = db.RefreshTokens.Where(token =>
            (token.ExpiresAt <= cutoff || token.AbsoluteExpiration <= cutoff) &&
            !db.RefreshTokens.Any(session => session.UserId == token.UserId && !session.IsRevoked &&
                session.ExpiresAt > now && (session.AbsoluteExpiration == null || session.AbsoluteExpiration > now)));
        var deleted = 0;

        for (var batch = 0; batch < options.MaxBatchesPerRun; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = await eligible.OrderBy(token => token.ExpiresAt).ThenBy(token => token.Id)
                .Select(token => token.Id).Take(options.BatchSize).ToListAsync(cancellationToken);
            if (ids.Count == 0)
                break;

            deleted += await eligible.Where(token => ids.Contains(token.Id)).ExecuteDeleteAsync(cancellationToken);
        }

        Logger.LogInformation("Deleted {Count} expired refresh tokens past retention", deleted);
    }
}
