using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteFlashSale;

public record DeleteFlashSaleCommand(Guid Id) : ICommand;
