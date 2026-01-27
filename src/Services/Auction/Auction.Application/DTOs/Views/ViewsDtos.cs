namespace Auctions.Application.DTOs.Views;

public record RecordViewResponseDto
{
    public bool Success { get; init; }
}

public record ViewCountDto
{
    public Guid AuctionId { get; init; }
    public int ViewCount { get; init; }
}
