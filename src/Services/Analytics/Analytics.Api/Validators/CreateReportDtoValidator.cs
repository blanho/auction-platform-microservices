using Analytics.Api.Models;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Analytics.Api.Validators;

public class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportedUsername)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reported username"))
            .MaximumLength(ValidationConstants.StringLength.Username).WithMessage(ValidationConstants.Messages.MaxLength("Reported username", ValidationConstants.StringLength.Username));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("report type"));

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Reason"))
            .MinimumLength(ValidationConstants.MinLength.Name).WithMessage(ValidationConstants.Messages.MinLength("Reason", ValidationConstants.MinLength.Name))
            .MaximumLength(ValidationConstants.StringLength.Reason).WithMessage(ValidationConstants.Messages.MaxLength("Reason", ValidationConstants.StringLength.Reason));

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(ValidationConstants.StringLength.Large).WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Large));
        });
    }
}

public class UpdateReportStatusDtoValidator : AbstractValidator<UpdateReportStatusDto>
{
    public UpdateReportStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("report status"));

        When(x => !string.IsNullOrEmpty(x.Resolution), () =>
        {
            RuleFor(x => x.Resolution)
                .MaximumLength(ValidationConstants.StringLength.Large).WithMessage(ValidationConstants.Messages.MaxLength("Resolution", ValidationConstants.StringLength.Large));
        });
    }
}
