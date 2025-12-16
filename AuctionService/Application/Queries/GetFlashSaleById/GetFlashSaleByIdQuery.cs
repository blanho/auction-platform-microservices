using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetFlashSaleById;

public record GetFlashSaleByIdQuery(Guid Id) : IQuery<FlashSaleDto>;
