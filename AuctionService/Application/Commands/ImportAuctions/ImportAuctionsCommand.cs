using AuctionService.Application.DTOs;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.ImportAuctions;

public record ImportAuctionsCommand(
    List<ImportAuctionDto> Auctions,
    string Seller
) : ICommand<ImportAuctionsResultDto>;
