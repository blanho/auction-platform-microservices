using Jobs.Domain.Entities;
using Jobs.Domain.Enums;

namespace Jobs.Application.Interfaces;

public interface IJobExecutionLogRepository
{
    Task<PaginatedResult<JobExecutionLog>> GetByJobIdAsync(
        Guid jobId,
        int page,
        int pageSize,
        JobLogLevel? logLevel = null,
        CancellationToken cancellationToken = default);

    Task<List<JobExecutionLog>> GetStateTransitionsAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task AddAsync(JobExecutionLog log, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<JobExecutionLog> logs, CancellationToken cancellationToken = default);
}
