namespace AuctionService.Contracts.Events;

public record ImportRowErrorPayload
{
    public int RowNumber { get; init; }
    public string Field { get; init; } = string.Empty;
    public string ErrorMessage { get; init; } = string.Empty;
}
