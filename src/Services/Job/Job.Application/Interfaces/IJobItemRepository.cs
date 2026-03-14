using Jobs.Domain.Entities;
using Jobs.Domain.Enums;

namespace Jobs.Application.Interfaces;

public interface IJobItemRepository
{
    Task<JobItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobItem?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<JobItem>> GetPendingItemsByJobIdAsync(Guid jobId, int batchSize, CancellationToken cancellationToken = default);
    Task<List<JobItem>> GetItemsByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<JobItem>> GetPagedItemsByJobIdAsync(Guid jobId, JobItemStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountByStatusAsync(Guid jobId, JobItemStatus status, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<JobItem> items, CancellationToken cancellationToken = default);
    Task BulkCreateItemsAsync(Guid jobId, int maxRetryCount, IEnumerable<(string PayloadJson, int SequenceNumber)> items, CancellationToken cancellationToken = default);
    Task<List<JobItem>> GetByIdsForUpdateAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task UpdateAsync(JobItem item, CancellationToken cancellationToken = default);
    Task ResetFailedItemsAsync(Guid jobId, CancellationToken cancellationToken = default);
}
