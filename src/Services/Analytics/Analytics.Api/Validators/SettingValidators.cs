using Analytics.Api.Models;
using FluentValidation;

namespace Analytics.Api.Validators;

public class CreateSettingDtoValidator : AbstractValidator<CreateSettingDto>
{
    public CreateSettingDtoValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required")
            .MaximumLength(100).WithMessage("Setting key cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9._-]*$")
            .WithMessage("Setting key must start with a letter and contain only alphanumeric characters, dots, underscores, or hyphens");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Setting value is required")
            .MaximumLength(5000).WithMessage("Setting value cannot exceed 5000 characters");

        When(x => !string.IsNullOrEmpty(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
        });

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid setting category");
    }
}

public class UpdateSettingDtoValidator : AbstractValidator<UpdateSettingDto>
{
    public UpdateSettingDtoValidator()
    {
        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Setting value is required")
            .MaximumLength(5000).WithMessage("Setting value cannot exceed 5000 characters");
    }
}

public class SettingKeyValueValidator : AbstractValidator<SettingKeyValue>
{
    public SettingKeyValueValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required")
            .MaximumLength(100).WithMessage("Setting key cannot exceed 100 characters");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Setting value is required")
            .MaximumLength(5000).WithMessage("Setting value cannot exceed 5000 characters");
    }
}
