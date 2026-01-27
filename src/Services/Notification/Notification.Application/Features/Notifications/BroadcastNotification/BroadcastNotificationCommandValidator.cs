namespace Notification.Application.Features.Notifications.BroadcastNotification;

public class BroadcastNotificationCommandValidator : AbstractValidator<BroadcastNotificationCommand>
{
    public BroadcastNotificationCommandValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required")
            .MaximumLength(50).WithMessage("Type must not exceed 50 characters");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");
    }
}
