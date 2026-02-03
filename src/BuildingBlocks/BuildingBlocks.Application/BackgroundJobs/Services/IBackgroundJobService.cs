using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.BackgroundJobs.Queue;
using BuildingBlocks.Application.BackgroundJobs.Storage;

namespace BuildingBlocks.Application.BackgroundJobs.Services;

public interface IBackgroundJobService
{
    Task<Guid> EnqueueAsync(
        string jobType,
        Guid userId,
        Dictionary<string, object>? metadata = null,
        BackgroundJobPriority priority = BackgroundJobPriority.Normal,
        CancellationToken ct = default);

    Task<BackgroundJobState?> GetJobAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<BackgroundJobState>> GetUserJobsAsync(Guid userId, string? jobType = null, CancellationToken ct = default);
    Task CancelJobAsync(Guid jobId, CancellationToken ct = default);
}
