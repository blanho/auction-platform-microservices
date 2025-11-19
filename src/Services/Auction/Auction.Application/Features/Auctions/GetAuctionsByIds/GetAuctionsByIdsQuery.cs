using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetAuctionsByIds;

public record GetAuctionsByIdsQuery(IEnumerable<Guid> Ids) : IQuery<IEnumerable<AuctionDto>>;

