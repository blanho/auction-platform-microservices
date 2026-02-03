using BuildingBlocks.Application.BackgroundJobs.Core;

namespace BuildingBlocks.Application.BackgroundJobs.Storage;

public interface IBackgroundJobStore
{
    Task AddAsync(BackgroundJobState job, CancellationToken ct = default);
    Task<BackgroundJobState?> GetAsync(Guid jobId, CancellationToken ct = default);
    Task UpdateAsync(BackgroundJobState job, CancellationToken ct = default);
    Task RemoveAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<BackgroundJobState>> GetByUserAsync(Guid userId, string? jobType = null, CancellationToken ct = default);
    Task<IReadOnlyList<BackgroundJobState>> GetRetryableJobsAsync(CancellationToken ct = default);
    Task CleanupExpiredAsync(CancellationToken ct = default);
}
