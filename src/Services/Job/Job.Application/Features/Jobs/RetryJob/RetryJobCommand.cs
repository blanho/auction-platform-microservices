namespace Jobs.Application.Features.Jobs.RetryJob;

public record RetryJobCommand(Guid JobId) : ICommand<JobDto>;
