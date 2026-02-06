using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jobs.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly JobDbContext _context;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditContext _auditContext;

    public JobRepository(JobDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
    {
        _context = context;
        _dateTime = dateTime;
        _auditContext = auditContext;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Job?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Job?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Job?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CorrelationId == correlationId, cancellationToken);
    }

    public async Task<PaginatedResult<Job>> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Job>(items, totalCount, page, pageSize);
    }

    public async Task<PaginatedResult<Job>> GetFilteredAsync(
        JobType? type,
        JobStatus? status,
        Guid? requestedBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs
            .Where(x => !x.IsDeleted)
            .AsNoTracking();

        if (type.HasValue)
            query = query.Where(x => x.Type == type.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (requestedBy.HasValue)
            query = query.Where(x => x.RequestedBy == requestedBy.Value);

        var orderedQuery = query.OrderByDescending(x => x.CreatedAt);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Job>(items, totalCount, page, pageSize);
    }

    public async Task<List<Job>> GetPendingJobsByPriorityAsync(
        int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Where(x => !x.IsDeleted && x.Status == JobStatus.Pending)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Job>> GetStuckProcessingJobsAsync(
        TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var cutoff = _dateTime.UtcNowOffset.Subtract(timeout);
        return await _context.Jobs
            .Where(x => !x.IsDeleted
                && x.Status == JobStatus.Processing
                && x.StartedAt.HasValue
                && x.StartedAt.Value < cutoff)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        job.CreatedAt = _dateTime.UtcNowOffset;
        job.CreatedBy = _auditContext.UserId;
        job.IsDeleted = false;

        foreach (var item in job.Items)
        {
            item.CreatedAt = _dateTime.UtcNowOffset;
            item.CreatedBy = _auditContext.UserId;
            item.IsDeleted = false;
        }

        await _context.Jobs.AddAsync(job, cancellationToken);
        return job;
    }

    public Task UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        job.UpdatedAt = _dateTime.UtcNowOffset;
        job.UpdatedBy = _auditContext.UserId;
        _context.Jobs.Update(job);
        return Task.CompletedTask;
    }
}
