using Common.Core.Helpers;
using FluentAssertions;

namespace AuctionService.UnitTests.Core;

public class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsFailedResult()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_WithValue_ReturnsSuccessfulResultWithValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Failure_WithValue_ReturnsFailedResultWithNoValue()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void GetValueOrThrow_WhenSuccess_ReturnsValue()
    {
        // Arrange
        var value = "test value";
        var result = Result.Success(value);

        // Act
        var actualValue = result.GetValueOrThrow();

        // Assert
        actualValue.Should().Be(value);
    }

    [Fact]
    public void GetValueOrThrow_WhenFailure_ThrowsException()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var action = () => result.GetValueOrThrow();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Test error message*");
    }

    [Fact]
    public void GetValueOrDefault_WhenSuccess_ReturnsValue()
    {
        // Arrange
        var value = "test value";
        var result = Result.Success(value);

        // Act
        var actualValue = result.GetValueOrDefault("default");

        // Assert
        actualValue.Should().Be(value);
    }

    [Fact]
    public void GetValueOrDefault_WhenFailure_ReturnsDefaultValue()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var actualValue = result.GetValueOrDefault("default");

        // Assert
        actualValue.Should().Be("default");
    }

    [Fact]
    public void Bind_WhenSuccess_ExecutesNextOperation()
    {
        // Arrange
        var result = Result.Success(5);
        Func<int, Result<int>> doubleValue = x => Result.Success(x * 2);

        // Act
        var finalResult = result.Bind(doubleValue);

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be(10);
    }

    [Fact]
    public void Bind_WhenFailure_DoesNotExecuteNextOperation()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error");
        var result = Result.Failure<int>(error);
        var wasCalled = false;
        Func<int, Result<int>> doubleValue = x =>
        {
            wasCalled = true;
            return Result.Success(x * 2);
        };

        // Act
        var finalResult = result.Bind(doubleValue);

        // Assert
        wasCalled.Should().BeFalse();
        finalResult.IsFailure.Should().BeTrue();
        finalResult.Error.Should().Be(error);
    }

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        // Arrange
        var result = Result.Success(5);
        Func<int, string> toString = x => x.ToString();

        // Act
        var finalResult = result.Map(toString);

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be("5");
    }

    [Fact]
    public void Map_WhenFailure_PropagatesError()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error");
        var result = Result.Failure<int>(error);
        Func<int, string> toString = x => x.ToString();

        // Act
        var finalResult = result.Map(toString);

        // Assert
        finalResult.IsFailure.Should().BeTrue();
        finalResult.Error.Should().Be(error);
    }

    [Fact]
    public void Tap_WhenSuccess_ExecutesSideEffect()
    {
        // Arrange
        var result = Result.Success(5);
        var capturedValue = 0;
        Action<int> captureValue = x => capturedValue = x;

        // Act
        var finalResult = result.Tap(captureValue);

        // Assert
        capturedValue.Should().Be(5);
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be(5);
    }

    [Fact]
    public void Tap_WhenFailure_DoesNotExecuteSideEffect()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error");
        var result = Result.Failure<int>(error);
        var wasCalled = false;
        Action<int> sideEffect = x => wasCalled = true;

        // Act
        var finalResult = result.Tap(sideEffect);

        // Assert
        wasCalled.Should().BeFalse();
        finalResult.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Match_WhenSuccess_ExecutesOnSuccessFunc()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: error => $"Failure: {error.Message}"
        );

        // Assert
        output.Should().Be("Success: 5");
    }

    [Fact]
    public void Match_WhenFailure_ExecutesOnFailureFunc()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Test error message");
        var result = Result.Failure<int>(error);

        // Act
        var output = result.Match(
            onSuccess: value => $"Success: {value}",
            onFailure: err => $"Failure: {err.Message}"
        );

        // Assert
        output.Should().Be("Failure: Test error message");
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        // Arrange & Act
        Result<string> result = "test value";

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
    }

    [Fact]
    public void FromValue_WhenNotNull_ReturnsSuccess()
    {
        // Arrange
        var value = "test";
        var error = Error.Create("Test.Error", "Value was null");

        // Act
        var result = Result.FromValue(value, error);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void FromValue_WhenNull_ReturnsFailure()
    {
        // Arrange
        string? value = null;
        var error = Error.Create("Test.Error", "Value was null");

        // Act
        var result = Result.FromValue(value, error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Create_WhenConditionTrue_ReturnsSuccess()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Condition was false");

        // Act
        var result = Result.Create(true, error);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WhenConditionFalse_ReturnsFailure()
    {
        // Arrange
        var error = Error.Create("Test.Error", "Condition was false");

        // Act
        var result = Result.Create(false, error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}

public class ValidationErrorTests
{
    [Fact]
    public void WithErrors_CreatesValidationErrorWithErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Title", new[] { "Title is required" } },
            { "Description", new[] { "Description is too short", "Description is required" } }
        };

        // Act
        var validationError = ValidationError.WithErrors(errors);

        // Assert
        validationError.Code.Should().Be("Validation.Failed");
        validationError.Message.Should().Be("One or more validation errors occurred.");
        validationError.Errors.Should().HaveCount(2);
        validationError.Errors["Title"].Should().Contain("Title is required");
        validationError.Errors["Description"].Should().HaveCount(2);
    }

    [Fact]
    public void ValidationError_InheritsFromError()
    {
        // Arrange
        var errors = new Dictionary<string, string[]> { { "Field", new[] { "Error" } } };

        // Act
        var validationError = ValidationError.WithErrors(errors);

        // Assert
        validationError.Should().BeAssignableTo<Error>();
    }
}
