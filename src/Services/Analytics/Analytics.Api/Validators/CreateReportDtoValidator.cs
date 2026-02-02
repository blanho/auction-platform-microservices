using Analytics.Api.Models;
using FluentValidation;

namespace Analytics.Api.Validators;

public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportedUsername)
            .NotEmpty().WithMessage("Reported username is required")
            .MaximumLength(256).WithMessage("Reported username cannot exceed 256 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid report type");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
        });
    }
}

public class UpdateReportStatusDtoValidator : AbstractValidator<UpdateReportStatusDto>
{
    public UpdateReportStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid report status");

        When(x => !string.IsNullOrEmpty(x.Resolution), () =>
        {
            RuleFor(x => x.Resolution)
                .MaximumLength(2000).WithMessage("Resolution cannot exceed 2000 characters");
        });
    }
}
