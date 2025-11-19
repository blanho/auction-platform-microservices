using FluentValidation;
using Storage.Application.DTOs;

namespace Storage.Application.Validators;

public class BatchConfirmRequestValidator : AbstractValidator<BatchConfirmRequest>
{
    public BatchConfirmRequestValidator()
    {
        RuleFor(x => x.Files)
            .NotEmpty().WithMessage("At least one file must be provided")
            .Must(files => files.Count <= 100).WithMessage("Cannot confirm more than 100 files at once");

        RuleForEach(x => x.Files)
            .SetValidator(new ConfirmUploadRequestValidator());
    }
}
