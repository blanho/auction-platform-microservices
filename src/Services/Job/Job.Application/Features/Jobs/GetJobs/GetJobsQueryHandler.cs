using AutoMapper;
using Jobs.Domain.Enums;

namespace Jobs.Application.Features.Jobs.GetJobs;

public class GetJobsQueryHandler(
    IJobRepository jobRepository,
    IMapper mapper,
    ILogger<GetJobsQueryHandler> logger)
    : IQueryHandler<GetJobsQuery, PaginatedResult<JobSummaryDto>>
{
    public async Task<Result<PaginatedResult<JobSummaryDto>>> Handle(
        GetJobsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Retrieving jobs page {Page} with filters Type={Type}, Status={Status}, RequestedBy={RequestedBy}",
            request.Page, request.Type, request.Status, request.RequestedBy);

        var paginatedJobs = await jobRepository.GetFilteredAsync(
            request.Type,
            request.Status,
            request.RequestedBy,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = mapper.Map<IReadOnlyList<JobSummaryDto>>(paginatedJobs.Items);

        return Result<PaginatedResult<JobSummaryDto>>.Success(
            new PaginatedResult<JobSummaryDto>(
                items,
                paginatedJobs.TotalCount,
                paginatedJobs.Page,
                paginatedJobs.PageSize));
    }
}
