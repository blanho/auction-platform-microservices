using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Xunit;

namespace BuildingBlocks.Application.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task InvalidRequest_CollectsEachValidatorsFailuresOnceAndSkipsTheHandler()
    {
        var titleRequired = new InlineValidator<string>();
        titleRequired.RuleFor(title => title).NotEmpty().WithMessage("Title is required.");

        var titleLength = new InlineValidator<string>();
        titleLength.RuleFor(title => title).MinimumLength(3).WithMessage("Title is too short.");

        var behavior = new ValidationBehavior<string, Result<int>>([titleRequired, titleLength]);
        var result = await behavior.Handle("", () => throw new InvalidOperationException("Handler should not run."),
            CancellationToken.None);

        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(new[] { "Title is required.", "Title is too short." }, Assert.Single(error.Errors).Value);
    }

    [Fact]
    public async Task Validators_DoNotShareTransientContextData()
    {
        var first = new InlineValidator<string>();
        first.RuleFor(value => value).Custom((_, context) => context.RootContextData["Marker"] = true);

        var second = new InlineValidator<string>();
        second.RuleFor(value => value).Custom((_, context) =>
        {
            if (context.RootContextData.ContainsKey("Marker"))
                context.AddFailure("A previous validator's context was reused.");
        });

        var behavior = new ValidationBehavior<string, Result>([first, second]);
        var result = await behavior.Handle("valid", () => Task.FromResult(Result.Success()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
