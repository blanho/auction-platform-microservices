using BuildingBlocks.Domain.Constants;

namespace Payment.Application.Features.Orders.QueueOrderReportGeneration;

public class QueueOrderReportCommandValidator : AbstractValidator<QueueOrderReportCommand>
{
    public QueueOrderReportCommandValidator()
    {
        RuleFor(x => x.RequestedBy)
            .NotEmpty()
            .WithMessage(ValidationConstants.Messages.Required("Requested by"));

        RuleFor(x => x.ReportType)
            .IsInEnum()
            .WithMessage(ValidationConstants.Messages.InvalidEnumValue("Report type"));

        RuleFor(x => x.Format)
            .IsInEnum()
            .WithMessage(ValidationConstants.Messages.InvalidEnumValue("Report format"));

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage(ValidationConstants.Messages.Invalid("Date range"));

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .When(x => x.StartDate.HasValue)
            .WithMessage(ValidationConstants.Messages.Invalid("Start date"));
    }
}
