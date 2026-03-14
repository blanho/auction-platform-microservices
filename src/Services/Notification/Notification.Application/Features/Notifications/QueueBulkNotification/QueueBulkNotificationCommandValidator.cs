using BuildingBlocks.Domain.Constants;
using FluentValidation;
using Notification.Domain.Constants;

namespace Notification.Application.Features.Notifications.QueueBulkNotification;

public class QueueBulkNotificationCommandValidator : AbstractValidator<QueueBulkNotificationCommand>
{
    public QueueBulkNotificationCommandValidator()
    {
        RuleFor(x => x.RequestedBy)
            .NotEmpty();

        RuleFor(x => x.TemplateKey)
            .NotEmpty()
            .MaximumLength(ValidationConstants.StringLength.Standard);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(ValidationConstants.StringLength.Medium);

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.Channels)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("notification channel"));

        RuleFor(x => x.Recipients)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.MustContainAtLeastOne("recipient"));

        RuleFor(x => x.Recipients.Count)
            .LessThanOrEqualTo(NotificationDefaults.Bulk.MaxRecipients)
            .WithMessage(ValidationConstants.Messages.MustNotExceed("recipients", NotificationDefaults.Bulk.MaxRecipients));

        RuleFor(x => x.BatchSize)
            .InclusiveBetween(NotificationDefaults.Bulk.MinBatchSize, NotificationDefaults.Bulk.MaxBatchSize)
            .WithMessage(ValidationConstants.Messages.OutOfRange("Batch size", NotificationDefaults.Bulk.MinBatchSize, NotificationDefaults.Bulk.MaxBatchSize));

        RuleForEach(x => x.Recipients).ChildRules(recipient =>
        {
            recipient.RuleFor(r => r.UserId).NotEmpty();
            recipient.RuleFor(r => r.Email)
                .NotEmpty()
                .EmailAddress()
                .When(r => !string.IsNullOrEmpty(r.Email));
        });
    }
}
