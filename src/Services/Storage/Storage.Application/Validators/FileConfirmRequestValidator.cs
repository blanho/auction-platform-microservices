using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Storage.Application.DTOs;

namespace Storage.Application.Validators;

public class FileConfirmRequestValidator : AbstractValidator<FileConfirmRequest>
{
    public FileConfirmRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("File ID"));

        RuleFor(x => x.OwnerService)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Owner service"))
            .MaximumLength(ValidationConstants.StringLength.Standard)
            .WithMessage(ValidationConstants.Messages.MaxLength("Owner service", ValidationConstants.StringLength.Standard));

        When(x => !string.IsNullOrEmpty(x.OwnerId), () =>
        {
            RuleFor(x => x.OwnerId)
                .MaximumLength(ValidationConstants.StringLength.Standard)
                .WithMessage(ValidationConstants.Messages.MaxLength("Owner ID", ValidationConstants.StringLength.Standard));
        });
    }
}
