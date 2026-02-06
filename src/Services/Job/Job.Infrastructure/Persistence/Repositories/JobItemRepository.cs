using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Providers;
using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jobs.Infrastructure.Persistence.Repositories;

public class JobItemRepository : IJobItemRepository
{
    private readonly JobDbContext _context;
    private readonly IDateTimeProvider _dateTime;
    private readonly IAuditContext _auditContext;

    public JobItemRepository(JobDbContext context, IDateTimeProvider dateTime, IAuditContext auditContext)
    {
        _context = context;
        _dateTime = dateTime;
        _auditContext = auditContext;
    }

    public async Task<JobItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobItems
            .Where(x => !x.IsDeleted)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<JobItem?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JobItems
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<JobItem>> GetPendingItemsByJobIdAsync(
        Guid jobId, int batchSize, CancellationToken cancellationToken = default)
    {
        return await _context.JobItems
            .Where(x => !x.IsDeleted && x.JobId == jobId && x.Status == JobItemStatus.Pending)
            .OrderBy(x => x.SequenceNumber)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<JobItem>> GetItemsByJobIdAsync(
        Guid jobId, CancellationToken cancellationToken = default)
    {
        return await _context.JobItems
            .Where(x => !x.IsDeleted && x.JobId == jobId)
            .OrderBy(x => x.SequenceNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<JobItem>> GetPagedItemsByJobIdAsync(
        Guid jobId,
        JobItemStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JobItems
            .Where(x => !x.IsDeleted && x.JobId == jobId)
            .AsNoTracking();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var orderedQuery = query.OrderBy(x => x.SequenceNumber);

        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<JobItem>(items, totalCount, page, pageSize);
    }

    public async Task<int> GetCountByStatusAsync(
        Guid jobId, JobItemStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.JobItems
            .Where(x => !x.IsDeleted && x.JobId == jobId && x.Status == status)
            .CountAsync(cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<JobItem> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            item.CreatedAt = _dateTime.UtcNowOffset;
            item.CreatedBy = _auditContext.UserId;
            item.IsDeleted = false;
        }

        await _context.JobItems.AddRangeAsync(items, cancellationToken);
    }

    public async Task BulkCreateItemsAsync(
        Guid jobId,
        int maxRetryCount,
        IEnumerable<(string PayloadJson, int SequenceNumber)> items,
        CancellationToken cancellationToken = default)
    {
        const int batchSize = 1000;
        var batch = new List<JobItem>(batchSize);

        foreach (var (payloadJson, sequenceNumber) in items)
        {
            var jobItem = JobItem.Create(jobId, payloadJson, sequenceNumber, maxRetryCount);
            jobItem.CreatedAt = _dateTime.UtcNowOffset;
            jobItem.CreatedBy = _auditContext.UserId;
            jobItem.IsDeleted = false;
            batch.Add(jobItem);

            if (batch.Count >= batchSize)
            {
                await _context.JobItems.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _context.ChangeTracker.Clear();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            await _context.JobItems.AddRangeAsync(batch, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            _context.ChangeTracker.Clear();
        }
    }

    public async Task<List<JobItem>> GetByIdsForUpdateAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.JobItems
            .Where(x => !x.IsDeleted && idList.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(JobItem item, CancellationToken cancellationToken = default)
    {
        item.UpdatedAt = _dateTime.UtcNowOffset;
        item.UpdatedBy = _auditContext.UserId;
        _context.JobItems.Update(item);
        return Task.CompletedTask;
    }

    public async Task ResetFailedItemsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var failedItems = await _context.JobItems
            .Where(x => !x.IsDeleted && x.JobId == jobId && x.Status == JobItemStatus.Failed)
            .ToListAsync(cancellationToken);

        foreach (var item in failedItems)
        {
            item.ResetForRetry();
            item.UpdatedAt = _dateTime.UtcNowOffset;
            item.UpdatedBy = _auditContext.UserId;
        }
    }
}
