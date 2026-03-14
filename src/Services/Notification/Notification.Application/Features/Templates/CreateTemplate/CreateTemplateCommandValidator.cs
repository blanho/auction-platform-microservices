using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Notification.Application.Features.Templates.CreateTemplate;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Template key"))
            .MaximumLength(100).WithMessage(ValidationConstants.Messages.MaxLength("Template key", 100));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Template name"))
            .MaximumLength(200).WithMessage(ValidationConstants.Messages.MaxLength("Template name", 200));

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Subject"))
            .MaximumLength(500).WithMessage(ValidationConstants.Messages.MaxLength("Subject", 500));

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Body"))
            .MaximumLength(10000).WithMessage(ValidationConstants.Messages.MaxLength("Body", 10000));

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage(ValidationConstants.Messages.MaxLength("Description", 1000));
        });

        When(x => !string.IsNullOrWhiteSpace(x.SmsBody), () =>
        {
            RuleFor(x => x.SmsBody)
                .MaximumLength(160).WithMessage(ValidationConstants.Messages.MaxLength("SMS body", 160));
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushTitle), () =>
        {
            RuleFor(x => x.PushTitle)
                .MaximumLength(100).WithMessage(ValidationConstants.Messages.MaxLength("Push title", 100));
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushBody), () =>
        {
            RuleFor(x => x.PushBody)
                .MaximumLength(500).WithMessage(ValidationConstants.Messages.MaxLength("Push body", 500));
        });
    }
}
