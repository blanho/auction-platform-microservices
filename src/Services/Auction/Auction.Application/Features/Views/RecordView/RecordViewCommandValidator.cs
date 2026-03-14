using FluentValidation;

namespace Auctions.Application.Features.Views.RecordView;

public class RecordViewCommandValidator : AbstractValidator<RecordViewCommand>
{
    public RecordViewCommandValidator()
    {
        RuleFor(x => x.AuctionId).NotEmpty();
    }
}
