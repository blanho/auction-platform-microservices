using BuildingBlocks.Infrastructure.Extensions;
using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;

namespace Jobs.Infrastructure.Persistence.Repositories;

public class JobExecutionLogRepository : IJobExecutionLogRepository
{
    private readonly JobDbContext _context;

    public JobExecutionLogRepository(JobDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<JobExecutionLog>> GetByJobIdAsync(
        Guid jobId,
        int page,
        int pageSize,
        JobLogLevel? logLevel = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<JobExecutionLog>()
            .Where(l => l.JobId == jobId)
            .AsNoTracking();

        if (logLevel.HasValue)
            query = query.Where(l => l.LogLevel == logLevel.Value);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .ThenBy(entity => entity.Id)
            .ToPaginatedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<List<JobExecutionLog>> GetStateTransitionsAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<JobExecutionLog>()
            .Where(l => l.JobId == jobId && l.LogLevel == JobLogLevel.StateTransition)
            .OrderBy(l => l.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(JobExecutionLog log, CancellationToken cancellationToken = default)
    {
        await _context.Set<JobExecutionLog>().AddAsync(log, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<JobExecutionLog> logs, CancellationToken cancellationToken = default)
    {
        await _context.Set<JobExecutionLog>().AddRangeAsync(logs, cancellationToken);
    }
}
