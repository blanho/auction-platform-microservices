using BuildingBlocks.Infrastructure.Repository;
using BuildingBlocks.Infrastructure.Extensions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;

namespace Jobs.Infrastructure.Persistence.Repositories;

public class JobRepository : EfRepository<Job, JobDbContext>, IJobRepository
{
    protected override void ApplyCreatedAudit(Job job, DateTimeOffset timestamp)
    {
        base.ApplyCreatedAudit(job, timestamp);
        foreach (var item in job.Items)
            item.SetCreatedAudit(GetCreatedBy(job), timestamp);
    }

    protected override void TrackUpdate(Job job)
    {
        if (Context.Entry(job).State == EntityState.Detached)
            Context.Jobs.Update(job);
    }

    public JobRepository(JobDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
        : base(context, dateTime, auditContext)
    {
    }

    public async Task<Job?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Jobs
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Job?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default) =>
        FirstOrDefaultAsync(job => job.CorrelationId == correlationId, cancellationToken);

    public Task<PaginatedResult<Job>> GetFilteredAsync(
        JobType? type,
        JobStatus? status,
        Guid? requestedBy,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        GetPagedAsync(job => (!type.HasValue || job.Type == type)
            && (!status.HasValue || job.Status == status)
            && (!requestedBy.HasValue || job.RequestedBy == requestedBy), page, pageSize, cancellationToken);

    public async Task<List<Job>> GetPendingJobsByPriorityAsync(
        int batchSize, CancellationToken cancellationToken = default)
    {
        return await Context.Jobs
            .Where(x => !x.IsDeleted
                && (x.Status == JobStatus.Pending || x.Status == JobStatus.Processing)
                && x.Items.Any(item => !item.IsDeleted && item.Status == JobItemStatus.Pending))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Job>> GetStuckProcessingJobsAsync(
        TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var cutoff = Clock.UtcNowOffset.Subtract(timeout);
        return await Context.Jobs
            .Where(x => !x.IsDeleted
                && x.Status == JobStatus.Processing
                && x.StartedAt.HasValue
                && x.StartedAt.Value < cutoff)
            .ToListAsync(cancellationToken);
    }
}
