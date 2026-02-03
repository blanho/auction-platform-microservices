using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.BackgroundJobs.Queue;
using BuildingBlocks.Application.BackgroundJobs.Storage;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.BackgroundJobs.Services;

public sealed class BackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobQueue _queue;
    private readonly IBackgroundJobStore _store;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(
        IBackgroundJobQueue queue,
        IBackgroundJobStore store,
        ILogger<BackgroundJobService> logger)
    {
        _queue = queue;
        _store = store;
        _logger = logger;
    }

    public async Task<Guid> EnqueueAsync(
        string jobType,
        Guid userId,
        Dictionary<string, object>? metadata = null,
        BackgroundJobPriority priority = BackgroundJobPriority.Normal,
        CancellationToken ct = default)
    {
        var job = new BackgroundJobState(jobType, userId, metadata, priority);

        await _store.AddAsync(job, ct);
        await _queue.EnqueueAsync(job, ct);

        _logger.LogInformation(
            "Enqueued job {JobId} of type {JobType} for user {UserId} with priority {Priority}",
            job.Id, jobType, userId, priority);

        return job.Id;
    }

    public async Task<BackgroundJobState?> GetJobAsync(Guid jobId, CancellationToken ct = default)
    {
        return await _store.GetAsync(jobId, ct);
    }

    public async Task<IReadOnlyList<BackgroundJobState>> GetUserJobsAsync(
        Guid userId,
        string? jobType = null,
        CancellationToken ct = default)
    {
        return await _store.GetByUserAsync(userId, jobType, ct);
    }

    public async Task CancelJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _store.GetAsync(jobId, ct);

        if (job is null)
        {
            _logger.LogWarning("Attempted to cancel non-existent job {JobId}", jobId);
            return;
        }

        if (job.Status is BackgroundJobStatus.Completed or BackgroundJobStatus.Failed or BackgroundJobStatus.Cancelled)
        {
            _logger.LogDebug("Job {JobId} is already in terminal state {Status}", jobId, job.Status);
            return;
        }

        job.MarkCancelled();
        await _store.UpdateAsync(job, ct);

        _logger.LogInformation("Cancelled job {JobId}", jobId);
    }
}
