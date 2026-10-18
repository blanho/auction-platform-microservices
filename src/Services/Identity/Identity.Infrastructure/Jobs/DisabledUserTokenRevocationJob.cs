using BuildingBlocks.Infrastructure.Scheduling;
using Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Identity.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class DisabledUserTokenRevocationJob(
    ILogger<DisabledUserTokenRevocationJob> logger,
    IServiceProvider serviceProvider) : BaseJob(logger, serviceProvider)
{
    public const string JobId = "disabled-user-token-revocation";
    public const string Description = "Revokes refresh tokens belonging to inactive or suspended accounts";

    protected override async Task ExecuteJobAsync(IServiceProvider scopedProvider, CancellationToken cancellationToken)
    {
        var options = scopedProvider.GetRequiredService<IOptions<IdentityJobOptions>>().Value;
        if (!options.DisabledUserTokenRevocationEnabled)
            return;

        var db = scopedProvider.GetRequiredService<ApplicationDbContext>();
        var now = scopedProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var eligible = db.RefreshTokens.Where(token => !token.IsRevoked &&
            (!token.User.IsActive || token.User.IsSuspended));
        var revoked = 0;

        for (var batch = 0; batch < options.MaxBatchesPerRun; batch++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = await eligible.OrderBy(token => token.CreatedAt).ThenBy(token => token.Id)
                .Select(token => token.Id).Take(options.BatchSize).ToListAsync(cancellationToken);
            if (ids.Count == 0)
                break;

            revoked += await eligible.Where(token => ids.Contains(token.Id)).ExecuteUpdateAsync(setters => setters
                .SetProperty(token => token.IsRevoked, true)
                .SetProperty(token => token.RevokedAt, now), cancellationToken);
        }

        Logger.LogInformation("Revoked {Count} refresh tokens for disabled accounts", revoked);
    }
}
