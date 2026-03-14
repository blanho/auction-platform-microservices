using Bidding.Application.DTOs;
using Bidding.Domain.Entities;

namespace Bidding.Application.Extensions.Mappings;

public static class AutoBidMappingExtensions
{
    public static AutoBidDto ToDto(this AutoBid autoBid)
    {
        return new AutoBidDto(
            autoBid.Id,
            autoBid.AuctionId,
            autoBid.UserId,
            autoBid.Username,
            autoBid.MaxAmount,
            autoBid.CurrentBidAmount,
            autoBid.IsActive,
            autoBid.CreatedAt,
            autoBid.LastBidAt);
    }

    public static List<AutoBidDto> ToDtoList(this IEnumerable<AutoBid> autoBids)
    {
        return autoBids.Select(ab => ab.ToDto()).ToList();
    }
}
