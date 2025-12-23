using FluentValidation;
using StorageService.Application.DTOs;

namespace StorageService.Application.Validators;

public class FileConfirmRequestValidator : AbstractValidator<FileConfirmRequest>
{
    public FileConfirmRequestValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required");

        RuleFor(x => x.OwnerService)
            .NotEmpty().WithMessage("Owner service is required")
            .MaximumLength(100).WithMessage("Owner service must not exceed 100 characters");

        When(x => !string.IsNullOrEmpty(x.OwnerId), () =>
        {
            RuleFor(x => x.OwnerId)
                .MaximumLength(100).WithMessage("Owner ID must not exceed 100 characters");
        });
    }
}
