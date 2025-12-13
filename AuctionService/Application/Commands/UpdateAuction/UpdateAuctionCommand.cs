using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.UpdateAuction;

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
