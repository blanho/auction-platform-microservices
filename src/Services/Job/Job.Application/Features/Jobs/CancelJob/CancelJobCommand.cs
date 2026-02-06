namespace Jobs.Application.Features.Jobs.CancelJob;

public record CancelJobCommand(Guid JobId) : ICommand<JobDto>;
