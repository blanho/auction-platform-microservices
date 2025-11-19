using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IQuery<AuctionDto>;

