using FluentValidation;

namespace Notification.Application.Features.Templates.UpdateTemplate;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template name is required")
            .MaximumLength(200).WithMessage("Template name must not exceed 200 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required")
            .MaximumLength(10000).WithMessage("Body must not exceed 10000 characters");

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.SmsBody), () =>
        {
            RuleFor(x => x.SmsBody)
                .MaximumLength(160).WithMessage("SMS body must not exceed 160 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushTitle), () =>
        {
            RuleFor(x => x.PushTitle)
                .MaximumLength(100).WithMessage("Push title must not exceed 100 characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushBody), () =>
        {
            RuleFor(x => x.PushBody)
                .MaximumLength(500).WithMessage("Push body must not exceed 500 characters");
        });
    }
}
