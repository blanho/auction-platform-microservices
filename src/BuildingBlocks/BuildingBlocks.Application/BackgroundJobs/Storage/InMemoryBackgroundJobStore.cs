using System.Collections.Concurrent;
using BuildingBlocks.Application.BackgroundJobs.Core;

namespace BuildingBlocks.Application.BackgroundJobs.Storage;

public sealed class InMemoryBackgroundJobStore : IBackgroundJobStore
{
    private readonly ConcurrentDictionary<Guid, BackgroundJobState> _jobs = new();
    private readonly TimeSpan _defaultRetention = TimeSpan.FromHours(24);
    private readonly int _maxJobsPerUser = 50;

    public Task AddAsync(BackgroundJobState job, CancellationToken ct = default)
    {
        CleanupUserJobsIfNeeded(job.UserId);
        _jobs.TryAdd(job.Id, job);
        return Task.CompletedTask;
    }

    public Task<BackgroundJobState?> GetAsync(Guid jobId, CancellationToken ct = default)
    {
        return Task.FromResult(_jobs.TryGetValue(jobId, out var job) ? job : null);
    }

    public Task UpdateAsync(BackgroundJobState job, CancellationToken ct = default)
    {
        _jobs.AddOrUpdate(job.Id, job, (_, _) => job);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid jobId, CancellationToken ct = default)
    {
        _jobs.TryRemove(jobId, out _);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<BackgroundJobState>> GetByUserAsync(Guid userId, string? jobType = null, CancellationToken ct = default)
    {
        var query = _jobs.Values.Where(j => j.UserId == userId);

        if (!string.IsNullOrEmpty(jobType))
            query = query.Where(j => j.JobType == jobType);

        IReadOnlyList<BackgroundJobState> result = query
            .OrderByDescending(j => j.CreatedAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<BackgroundJobState>> GetRetryableJobsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        IReadOnlyList<BackgroundJobState> result = _jobs.Values
            .Where(j => j.CanRetry && j.NextRetryAt.HasValue && j.NextRetryAt.Value <= now)
            .OrderBy(j => j.NextRetryAt)
            .ToList();

        return Task.FromResult(result);
    }

    public Task CleanupExpiredAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var expiredJobs = _jobs.Values
            .Where(j => j.ExpiresAt.HasValue && j.ExpiresAt.Value < now)
            .ToList();

        foreach (var job in expiredJobs)
        {
            _jobs.TryRemove(job.Id, out _);
            CleanupJobResultFile(job);
        }

        var oldJobs = _jobs.Values
            .Where(j => !j.ExpiresAt.HasValue && 
                        j.Status is BackgroundJobStatus.Completed or BackgroundJobStatus.Failed or BackgroundJobStatus.Cancelled &&
                        j.CreatedAt < now.Subtract(_defaultRetention))
            .ToList();

        foreach (var job in oldJobs)
        {
            _jobs.TryRemove(job.Id, out _);
            CleanupJobResultFile(job);
        }

        return Task.CompletedTask;
    }

    private void CleanupUserJobsIfNeeded(Guid userId)
    {
        var userJobs = _jobs.Values
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToList();

        if (userJobs.Count >= _maxJobsPerUser)
        {
            var jobsToRemove = userJobs
                .Skip(_maxJobsPerUser - 1)
                .Where(j => j.Status is BackgroundJobStatus.Completed or BackgroundJobStatus.Failed or BackgroundJobStatus.Cancelled);

            foreach (var job in jobsToRemove)
            {
                _jobs.TryRemove(job.Id, out _);
                CleanupJobResultFile(job);
            }
        }
    }

    private static void CleanupJobResultFile(BackgroundJobState job)
    {
        if (string.IsNullOrEmpty(job.ResultFileName))
            return;

        var outputPath = Path.Combine(Path.GetTempPath(), $"job_{job.Id}");
        if (Directory.Exists(outputPath))
        {
            try
            {
                Directory.Delete(outputPath, recursive: true);
            }
            catch
            {
            }
        }
    }
}
