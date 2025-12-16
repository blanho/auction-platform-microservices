using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctionsByIds;

public record GetAuctionsByIdsQuery(IEnumerable<Guid> Ids) : IQuery<IEnumerable<AuctionDto>>;
