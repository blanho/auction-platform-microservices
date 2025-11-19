using FluentValidation;

namespace Bidding.Application.Validators;

public class PlaceBidDtoValidator : AbstractValidator<DTOs.PlaceBidDto>
{
    public PlaceBidDtoValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue / 100).WithMessage("Bid amount is too large");
    }
}

public class CreateAutoBidDtoValidator : AbstractValidator<DTOs.CreateAutoBidDto>
{
    public CreateAutoBidDtoValidator()
    {
        RuleFor(x => x.AuctionId)
            .NotEmpty().WithMessage("Auction ID is required");

        RuleFor(x => x.MaxAmount)
            .GreaterThan(0).WithMessage("Maximum bid amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue / 100).WithMessage("Maximum bid amount is too large");
    }
}

public class UpdateAutoBidDtoValidator : AbstractValidator<DTOs.UpdateAutoBidDto>
{
    public UpdateAutoBidDtoValidator()
    {
        RuleFor(x => x.MaxAmount)
            .GreaterThan(0).WithMessage("Maximum bid amount must be greater than 0")
            .LessThanOrEqualTo(decimal.MaxValue / 100).WithMessage("Maximum bid amount is too large");
    }
}

