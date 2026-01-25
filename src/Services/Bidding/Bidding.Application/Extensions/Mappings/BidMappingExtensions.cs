using Bidding.Application.DTOs;
using Bidding.Domain.Entities;

namespace Bidding.Application.Extensions.Mappings;

public static class BidMappingExtensions
{
    public static BidDto ToDto(this Bid bid)
    {
        return new BidDto
        {
            Id = bid.Id,
            AuctionId = bid.AuctionId,
            BidderId = bid.BidderId,
            BidderUsername = bid.BidderUsername,
            Amount = bid.Amount,
            BidTime = bid.BidTime,
            Status = bid.Status.ToString(),
            CreatedAt = bid.CreatedAt,
            UpdatedAt = bid.UpdatedAt
        };
    }

    public static List<BidDto> ToDtoList(this IEnumerable<Bid> bids)
    {
        return bids.Select(b => b.ToDto()).ToList();
    }
}
