using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteBrand;

public record DeleteBrandCommand(Guid Id) : ICommand;
