using Analytics.Api.Models;
using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Analytics.Api.Validators;

public class CreateSettingDtoValidator : AbstractValidator<CreateSettingDto>
{
    public CreateSettingDtoValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Setting key"))
            .MaximumLength(ValidationConstants.StringLength.Standard).WithMessage(ValidationConstants.Messages.MaxLength("Setting key", ValidationConstants.StringLength.Standard))
            .Matches(@"^[a-zA-Z][a-zA-Z0-9._-]*$")
            .WithMessage(ValidationConstants.Messages.InvalidFormat("Setting key"));

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Setting value"))
            .MaximumLength(ValidationConstants.StringLength.ExtraLarge).WithMessage(ValidationConstants.Messages.MaxLength("Setting value", ValidationConstants.StringLength.ExtraLarge));

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(ValidationConstants.StringLength.Reason).WithMessage(ValidationConstants.Messages.MaxLength("Description", ValidationConstants.StringLength.Reason));
        });

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage(ValidationConstants.Messages.InvalidEnumValue("setting category"));
    }
}

public class UpdateSettingDtoValidator : AbstractValidator<UpdateSettingDto>
{
    public UpdateSettingDtoValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Setting value"))
            .MaximumLength(ValidationConstants.StringLength.ExtraLarge).WithMessage(ValidationConstants.Messages.MaxLength("Setting value", ValidationConstants.StringLength.ExtraLarge));
    }
}

public class SettingKeyValueValidator : AbstractValidator<SettingKeyValue>
{
    public SettingKeyValueValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Setting key"))
            .MaximumLength(ValidationConstants.StringLength.Standard).WithMessage(ValidationConstants.Messages.MaxLength("Setting key", ValidationConstants.StringLength.Standard));

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Setting value"))
            .MaximumLength(ValidationConstants.StringLength.ExtraLarge).WithMessage(ValidationConstants.Messages.MaxLength("Setting value", ValidationConstants.StringLength.ExtraLarge));
    }
}
