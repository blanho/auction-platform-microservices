using AuctionService.Application.Commands.CreateAuction;
using FluentValidation.TestHelper;

namespace AuctionService.UnitTests.Validators;
public class CreateAuctionCommandValidatorTests
{
    private readonly CreateAuctionCommandValidator _validator;

    public CreateAuctionCommandValidatorTests()
    {
        _validator = new CreateAuctionCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with { Title = "" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var command = CreateValidCommand() with { Title = "AB" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must be at least 3 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand() with { Title = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Short()
    {
        // Arrange
        var command = CreateValidCommand() with { Description = "Short" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must be at least 10 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Year_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand() with { Year = 1800 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void Should_Have_Error_When_Mileage_Is_Negative()
    {
        // Arrange
        var command = CreateValidCommand() with { Mileage = -1 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Mileage)
            .WithErrorMessage("Mileage must be non-negative");
    }

    [Fact]
    public void Should_Have_Error_When_AuctionEnd_Is_In_Past()
    {
        // Arrange
        var command = CreateValidCommand() with { AuctionEnd = DateTimeOffset.UtcNow.AddMinutes(-1) };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AuctionEnd);
    }

    [Fact]
    public void Should_Have_Error_When_ImageUrl_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand() with { ImageUrl = "not-a-valid-url" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ImageUrl)
            .WithErrorMessage("Image URL must be a valid URL");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ImageUrl_Is_Valid()
    {
        // Arrange
        var command = CreateValidCommand() with { ImageUrl = "https://example.com/image.jpg" };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ImageUrl_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand() with { ImageUrl = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ImageUrl);
    }

    [Fact]
    public void Should_Pass_Validation_With_Valid_Command()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Seller_Is_Empty_Or_Whitespace(string seller)
    {
        // Arrange
        var command = CreateValidCommand() with { Seller = seller };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Seller);
    }

    private static CreateAuctionCommand CreateValidCommand()
    {
        return new CreateAuctionCommand(
            Title: "2023 Tesla Model S",
            Description: "Beautiful electric vehicle in excellent condition with low mileage.",
            Make: "Tesla",
            Model: "Model S",
            Year: 2023,
            Color: "White",
            Mileage: 15000,
            ImageUrl: "https://example.com/tesla.jpg",
            ReservePrice: 50000,
            AuctionEnd: DateTimeOffset.UtcNow.AddDays(7),
            Seller: "test-seller"
        );
    }
}
