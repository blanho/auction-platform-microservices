using BuildingBlocks.Application.Abstractions;
using Search.Domain.Models;

using Search.Domain.Models;

namespace Search.Application.Interfaces;

public interface IIndexManagementService
{
    Task<Result> EnsureIndexExistsAsync(CancellationToken ct = default);

    Task<Result> RecreateIndexAsync(CancellationToken ct = default);

    Task<Result<IndexStats>> GetIndexStatsAsync(CancellationToken ct = default);

    Task<Result> IsHealthyAsync(CancellationToken ct = default);
}
