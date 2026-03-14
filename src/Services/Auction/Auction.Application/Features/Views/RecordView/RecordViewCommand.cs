namespace Auctions.Application.Features.Views.RecordView;

public record RecordViewCommand(
    Guid AuctionId,
    string? UserId,
    string? IpAddress) : ICommand<bool>;
