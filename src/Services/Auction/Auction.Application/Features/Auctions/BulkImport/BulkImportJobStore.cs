using System.Collections.Concurrent;

namespace Auctions.Application.Features.Auctions.BulkImport;

public interface IBulkImportJobStore
{
    void AddJob(BulkImportJob job);
    BulkImportJob? GetJob(Guid jobId);
    void UpdateJob(BulkImportJob job);
    IEnumerable<BulkImportJob> GetJobsByUser(Guid userId);
    void RemoveJob(Guid jobId);
}

public class InMemoryBulkImportJobStore : IBulkImportJobStore
{
    private readonly ConcurrentDictionary<Guid, BulkImportJob> _jobs = new();
    private const int MaxJobsPerUser = 10;
    private const int JobRetentionHours = 24;

    public void AddJob(BulkImportJob job)
    {
        CleanupOldJobs();
        _jobs[job.Id] = job;
    }

    public BulkImportJob? GetJob(Guid jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    public void UpdateJob(BulkImportJob job)
    {
        _jobs[job.Id] = job;
    }

    public IEnumerable<BulkImportJob> GetJobsByUser(Guid userId)
    {
        return _jobs.Values
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(MaxJobsPerUser);
    }

    public void RemoveJob(Guid jobId)
    {
        _jobs.TryRemove(jobId, out _);
    }

    private void CleanupOldJobs()
    {
        var cutoff = DateTimeOffset.UtcNow.AddHours(-JobRetentionHours);
        var oldJobs = _jobs.Values
            .Where(j => j.CreatedAt < cutoff && j.Status is BulkImportStatus.Completed or BulkImportStatus.Failed)
            .Select(j => j.Id)
            .ToList();

        foreach (var jobId in oldJobs)
        {
            _jobs.TryRemove(jobId, out _);
        }
    }
}
