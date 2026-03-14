namespace JobService.Contracts.Commands;

public record FailJobByCorrelationCommand
{
    public string CorrelationId { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
