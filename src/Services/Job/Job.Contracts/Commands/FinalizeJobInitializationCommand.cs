namespace JobService.Contracts.Commands;

public record FinalizeJobInitializationCommand
{
    public Guid JobId { get; init; }
}
