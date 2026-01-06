using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateAuction;

public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Condition,
    int? YearManufactured,
    Dictionary<string, string>? Attributes,
    /// <summary>
    /// The ID of the user making the update request. Used for ownership verification.
    /// </summary>
    Guid RequestingUserId,
    /// <summary>
    /// Whether the requesting user has admin privileges to bypass ownership check.
    /// </summary>
    bool IsAdmin = false
) : ICommand<bool>;
