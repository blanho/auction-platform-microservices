using BuildingBlocks.Domain.Constants;
using FluentValidation;

namespace Notification.Application.Features.Templates.UpdateTemplate;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Template ID"));

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Template name"))
            .MaximumLength(NotificationDefaults.Template.NameMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Template name", NotificationDefaults.Template.NameMaxLength));

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Subject"))
            .MaximumLength(NotificationDefaults.Template.SubjectMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Subject", NotificationDefaults.Template.SubjectMaxLength));

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Body"))
            .MaximumLength(NotificationDefaults.Template.BodyMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Body", NotificationDefaults.Template.BodyMaxLength));

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(NotificationDefaults.Template.DescriptionMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Description", NotificationDefaults.Template.DescriptionMaxLength));
        });

        When(x => !string.IsNullOrWhiteSpace(x.SmsBody), () =>
        {
            RuleFor(x => x.SmsBody)
                .MaximumLength(NotificationDefaults.Template.SmsBodyMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("SMS body", NotificationDefaults.Template.SmsBodyMaxLength));
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushTitle), () =>
        {
            RuleFor(x => x.PushTitle)
                .MaximumLength(NotificationDefaults.Template.PushTitleMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Push title", NotificationDefaults.Template.PushTitleMaxLength));
        });

        When(x => !string.IsNullOrWhiteSpace(x.PushBody), () =>
        {
            RuleFor(x => x.PushBody)
                .MaximumLength(NotificationDefaults.Template.PushBodyMaxLength).WithMessage(ValidationConstants.Messages.MaxLength("Push body", NotificationDefaults.Template.PushBodyMaxLength));
        });
    }
}
