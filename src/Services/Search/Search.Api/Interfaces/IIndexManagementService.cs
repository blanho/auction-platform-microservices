using Search.Api.Models;

namespace Search.Api.Interfaces;

public interface IIndexManagementService
{
    Task<Result> EnsureIndexExistsAsync(CancellationToken ct = default);

    Task<Result> RecreateIndexAsync(CancellationToken ct = default);

    Task<Result<IndexStats>> GetIndexStatsAsync(CancellationToken ct = default);

    Task<Result> IsHealthyAsync(CancellationToken ct = default);
}
