using FluentValidation;

namespace AuctionService.Application.Commands.DeleteAuction;

/// <summary>
/// FluentValidation validator for DeleteAuctionCommand
/// </summary>
public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Auction ID is required");
    }
}
