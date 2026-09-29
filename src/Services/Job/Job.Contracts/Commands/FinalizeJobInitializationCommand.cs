namespace JobService.Contracts.Commands;

public record FinalizeJobInitializationCommand
{
    public int ExpectedTotalItems { get; init; }
    public Guid JobId { get; init; }
}
