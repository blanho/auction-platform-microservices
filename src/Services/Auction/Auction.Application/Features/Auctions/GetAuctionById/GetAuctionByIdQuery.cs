using Auctions.Application.DTOs;
namespace Auctions.Application.Features.Auctions.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IQuery<AuctionDto>;

