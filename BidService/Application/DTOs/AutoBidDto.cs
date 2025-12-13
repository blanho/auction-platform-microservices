namespace BidService.Application.DTOs
{
    public record AutoBidDto(
        Guid Id,
        Guid AuctionId,
        string Bidder,
        int MaxAmount,
        int CurrentBidAmount,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? LastBidAt
    );

    public record CreateAutoBidDto(
        Guid AuctionId,
        int MaxAmount
    );

    public record UpdateAutoBidDto(
        int MaxAmount,
        bool? IsActive
    );
}
