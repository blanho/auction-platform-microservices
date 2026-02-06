using FluentValidation;

namespace Jobs.Application.Features.Jobs.CreateJob;

public class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.TotalItems)
            .GreaterThan(0);

        RuleFor(x => x.MaxRetryCount)
            .InclusiveBetween(0, 10);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.RequestedBy)
            .NotEmpty();
    }
}
