namespace Bidding.Application.DTOs
{
    public record AutoBidDto(
        Guid Id,
        Guid AuctionId,
        Guid UserId,
        string Username,
        decimal MaxAmount,
        decimal CurrentBidAmount,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset? LastBidAt
    );

    public record CreateAutoBidDto(
        Guid AuctionId,
        decimal MaxAmount
    );

    public record UpdateAutoBidDto(
        decimal MaxAmount,
        bool? IsActive
    );
}

