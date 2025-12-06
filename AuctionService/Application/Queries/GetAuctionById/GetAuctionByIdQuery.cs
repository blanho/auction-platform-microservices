using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctionById;

public record GetAuctionByIdQuery(Guid Id) : IQuery<AuctionDto>;
