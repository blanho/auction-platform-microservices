using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateAuction;

/// <summary>
/// Command to update an existing auction
/// </summary>
public record UpdateAuctionCommand(
    Guid Id,
    string? Title,
    string? Description,
    string? Make,
    string? Model,
    int? Year,
    string? Color,
    int? Mileage
) : ICommand<bool>;
