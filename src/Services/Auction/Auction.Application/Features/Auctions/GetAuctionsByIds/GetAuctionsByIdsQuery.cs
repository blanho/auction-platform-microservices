using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Auctions.GetAuctionsByIds;

public record GetAuctionsByIdsQuery(IEnumerable<Guid> Ids) : IQuery<IEnumerable<AuctionDto>>;

