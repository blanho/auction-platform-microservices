using Auctions.Application.DTOs;
namespace Auctions.Application.Commands.ImportAuctions;

public record ImportAuctionsCommand(
    List<ImportAuctionDto> Auctions,
    Guid SellerId,
    string Seller
) : ICommand<ImportAuctionsResultDto>;

