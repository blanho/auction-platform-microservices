using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.DeleteAuction;

public record DeleteAuctionCommand(
    Guid Id,
    /// <summary>
    /// The ID of the user making the delete request. Used for ownership verification.
    /// </summary>
    Guid RequestingUserId,
    /// <summary>
    /// Whether the requesting user has admin privileges to bypass ownership check.
    /// </summary>
    bool IsAdmin = false
) : ICommand<bool>;
