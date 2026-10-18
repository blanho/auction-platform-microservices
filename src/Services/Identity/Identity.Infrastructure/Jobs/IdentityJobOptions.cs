using System.ComponentModel.DataAnnotations;

namespace Identity.Infrastructure.Jobs;

public sealed class IdentityJobOptions
{
    public const string SectionName = "IdentityJobs";

    [Range(1, 10000)]
    public int BatchSize { get; set; } = 500;

    [Range(1, 100)]
    public int MaxBatchesPerRun { get; set; } = 10;

    [Range(1, 3650)]
    public int ExpiredTokenRetentionDays { get; set; } = 30;

    public bool RefreshTokenCleanupEnabled { get; set; } = true;

    public bool DisabledUserTokenRevocationEnabled { get; set; } = true;
}
