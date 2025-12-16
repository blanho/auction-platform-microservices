using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetFlashSales;

public record GetFlashSalesQuery(
    bool ActiveOnly = true
) : IQuery<List<FlashSaleDto>>;
