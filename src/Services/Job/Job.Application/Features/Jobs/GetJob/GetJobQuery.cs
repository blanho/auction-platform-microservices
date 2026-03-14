namespace Jobs.Application.Features.Jobs.GetJob;

public record GetJobQuery(Guid JobId) : IQuery<JobDto>;
