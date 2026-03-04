using BuildingBlocks.Domain.Constants;

namespace Notification.Application.Features.Notifications.CreateNotification;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("UserId"));

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Type"))
            .MaximumLength(50).WithMessage(ValidationConstants.Messages.MaxLength("Type", 50));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Title"))
            .MaximumLength(200).WithMessage(ValidationConstants.Messages.MaxLength("Title", 200));

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(ValidationConstants.Messages.Required("Message"))
            .MaximumLength(2000).WithMessage(ValidationConstants.Messages.MaxLength("Message", 2000));
    }
}
