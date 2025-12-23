using FluentValidation;
using StorageService.Application.DTOs;

namespace StorageService.Application.Validators;

public class ConfirmUploadRequestValidator : AbstractValidator<ConfirmUploadRequest>
{
    public ConfirmUploadRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required");

        When(x => !string.IsNullOrEmpty(x.OwnerId), () =>
        {
            RuleFor(x => x.OwnerId)
                .MaximumLength(100).WithMessage("Owner ID must not exceed 100 characters");
        });

        When(x => !string.IsNullOrEmpty(x.Checksum), () =>
        {
            RuleFor(x => x.Checksum)
                .MaximumLength(256).WithMessage("Checksum must not exceed 256 characters");
        });
    }
}
