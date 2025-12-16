using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateAuction;

public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes
) : ICommand<bool>;
