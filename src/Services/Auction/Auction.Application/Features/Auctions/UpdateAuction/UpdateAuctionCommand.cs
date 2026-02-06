namespace Auctions.Application.Commands.UpdateAuction;

public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes,
    Guid UserId
) : ICommand<bool>;
