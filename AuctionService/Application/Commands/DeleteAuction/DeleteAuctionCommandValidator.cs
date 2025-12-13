using FluentValidation;

namespace AuctionService.Application.Commands.DeleteAuction;

public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Auction ID is required");
    }
}
