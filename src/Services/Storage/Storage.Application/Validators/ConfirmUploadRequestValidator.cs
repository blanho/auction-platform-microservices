using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Storage.Application.DTOs;

namespace Storage.Application.Validators;

public class ConfirmUploadRequestValidator : AbstractValidator<ConfirmUploadRequest>
{
    public ConfirmUploadRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("File ID"));

        When(x => !string.IsNullOrEmpty(x.OwnerId), () =>
        {
            RuleFor(x => x.OwnerId)
                .MaximumLength(ValidationConstants.StringLength.Standard)
                .WithMessage(ValidationConstants.Messages.MaxLength("Owner ID", ValidationConstants.StringLength.Standard));
        });

        When(x => !string.IsNullOrEmpty(x.Checksum), () =>
        {
            RuleFor(x => x.Checksum)
                .MaximumLength(256).WithMessage(ValidationConstants.Messages.MaxLength("Checksum", 256));
        });
    }
}
