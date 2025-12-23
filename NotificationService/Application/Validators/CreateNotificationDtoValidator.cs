using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators;

public class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(256).WithMessage("User ID must not exceed 256 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid notification type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");

        When(x => !string.IsNullOrEmpty(x.Data), () =>
        {
            RuleFor(x => x.Data)
                .MaximumLength(10000).WithMessage("Data must not exceed 10000 characters");
        });
    }
}
