using Jobs.Domain.Entities;
using Jobs.Domain.Enums;

namespace Jobs.Application.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Job?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Job>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Job>> GetFilteredAsync(JobType? type, JobStatus? status, Guid? requestedBy, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Job>> GetPendingJobsByPriorityAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<List<Job>> GetStuckProcessingJobsAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    Task UpdateAsync(Job job, CancellationToken cancellationToken = default);
}
