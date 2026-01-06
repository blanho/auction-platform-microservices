using FluentValidation;

namespace AuctionService.Application.Commands.DeleteAuction;

public class DeleteAuctionCommandValidator : AbstractValidator<DeleteAuctionCommand>
{
    public DeleteAuctionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Auction ID is required");
            
        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Requesting user ID is required for authorization");
    }
}
