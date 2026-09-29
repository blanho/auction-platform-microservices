using FluentValidation;

namespace Jobs.Application.Features.Jobs.CreateJob;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .MaximumLength(JobDefaults.Validation.CorrelationIdMaxLength);

        RuleFor(x => x.TotalItems)
            .GreaterThan(0);

        RuleFor(x => x.MaxRetryCount)
            .InclusiveBetween(0, JobDefaults.Validation.MaxRetryCountUpperBound);

        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.TotalItems).Equal(x => x.Items.Count)
            .WithMessage("TotalItems must match the number of job items.");
        RuleFor(x => x.Items).Must(items => items.Select(i => i.SequenceNumber).Distinct().Count() == items.Count)
            .WithMessage("Item sequence numbers must be unique.");

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.RequestedBy)
            .NotEmpty();
    }
}
