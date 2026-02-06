using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobItems;

public record GetJobItemsQuery(
    Guid JobId,
    JobItemStatus? Status,
    int Page = 1,
    int PageSize = 50) : IQuery<PaginatedResult<JobItemDto>>;
