using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteAuction;

/// <summary>
/// Command to delete an auction
/// </summary>
public record DeleteAuctionCommand(Guid Id) : ICommand<bool>;
