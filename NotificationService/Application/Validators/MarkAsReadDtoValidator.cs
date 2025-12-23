using FluentValidation;
using NotificationService.Application.DTOs;

namespace NotificationService.Application.Validators;

public class MarkAsReadDtoValidator : AbstractValidator<MarkAsReadDto>
{
    public MarkAsReadDtoValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty().WithMessage("Notification ID is required");
    }
}
