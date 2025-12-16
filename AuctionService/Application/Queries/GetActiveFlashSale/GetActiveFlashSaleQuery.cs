using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetActiveFlashSale;

public record GetActiveFlashSaleQuery() : IQuery<ActiveFlashSaleDto?>;
