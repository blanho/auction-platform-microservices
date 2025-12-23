using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators;

public class BroadcastNotificationDtoValidator : AbstractValidator<BroadcastNotificationDto>
{
    public BroadcastNotificationDtoValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid notification type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");

        When(x => !string.IsNullOrEmpty(x.TargetRole), () =>
        {
            RuleFor(x => x.TargetRole)
                .MaximumLength(50).WithMessage("Target role must not exceed 50 characters");
        });
    }
}
