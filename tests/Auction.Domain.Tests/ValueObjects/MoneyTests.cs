using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;

namespace Auction.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        var money = new Money(100.50m, "USD");

        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldNormalizeCurrencyToUpperCase()
    {
        var money = new Money(50m, "usd");

        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_ShouldRoundAmountToTwoDecimalPlaces()
    {
        var money = new Money(99.999m, "USD");

        money.Amount.Should().Be(100.00m);
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrowDomainInvariantException()
    {
        var act = () => new Money(-1m, "USD");

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*negative*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrNullCurrency_ShouldThrowDomainInvariantException(string? currency)
    {
        var act = () => new Money(10m, currency!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Currency*required*");
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("ABC")]
    [InlineData("RUB")]
    public void Constructor_WithUnsupportedCurrency_ShouldThrowDomainInvariantException(string currency)
    {
        var act = () => new Money(10m, currency);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*not supported*");
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("AUD")]
    [InlineData("CAD")]
    [InlineData("CHF")]
    [InlineData("CNY")]
    [InlineData("SEK")]
    [InlineData("NOK")]
    public void Constructor_WithSupportedCurrency_ShouldSucceed(string currency)
    {
        var money = new Money(10m, currency);

        money.Currency.Should().Be(currency);
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        var money = Money.Zero("EUR");

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Of_ShouldCreateMoneyWithGivenAmountAndCurrency()
    {
        var money = Money.Of(250.75m, "GBP");

        money.Amount.Should().Be(250.75m);
        money.Currency.Should().Be("GBP");
    }

    [Fact]
    public void IsZero_WhenAmountIsZero_ShouldReturnTrue()
    {
        var money = Money.Zero();

        money.IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsZero_WhenAmountIsNotZero_ShouldReturnFalse()
    {
        var money = Money.Of(1m);

        money.IsZero().Should().BeFalse();
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(50.25m, "USD");

        var result = a.Add(b);

        result.Amount.Should().Be(150.25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowDomainInvariantException()
    {
        var usd = Money.Of(100m, "USD");
        var eur = Money.Of(50m, "EUR");

        var act = () => usd.Add(eur);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(30m, "USD");

        var result = a.Subtract(b);

        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_WhenResultWouldBeNegative_ShouldThrowDomainInvariantException()
    {
        var a = Money.Of(10m, "USD");
        var b = Money.Of(20m, "USD");

        var act = () => a.Subtract(b);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*negative money*");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldThrowDomainInvariantException()
    {
        var usd = Money.Of(100m, "USD");
        var eur = Money.Of(50m, "EUR");

        var act = () => usd.Subtract(eur);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*different currencies*");
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnProduct()
    {
        var money = Money.Of(50m, "USD");

        var result = money.Multiply(3m);

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Multiply_WithZeroFactor_ShouldReturnZero()
    {
        var money = Money.Of(50m, "USD");

        var result = money.Multiply(0m);

        result.Amount.Should().Be(0m);
    }

    [Fact]
    public void Multiply_WithNegativeFactor_ShouldThrowDomainInvariantException()
    {
        var money = Money.Of(50m, "USD");

        var act = () => money.Multiply(-2m);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*negative factor*");
    }

    [Theory]
    [InlineData(10, 100, 10)]
    [InlineData(50, 200, 100)]
    [InlineData(0, 500, 0)]
    [InlineData(100, 80, 80)]
    public void Percentage_WithValidPercent_ShouldReturnCorrectAmount(
        decimal percent, decimal amount, decimal expected)
    {
        var money = Money.Of(amount, "USD");

        var result = money.Percentage(percent);

        result.Amount.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Percentage_WithInvalidPercent_ShouldThrowDomainInvariantException(decimal percent)
    {
        var money = Money.Of(100m, "USD");

        var act = () => money.Percentage(percent);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Percentage*between 0 and 100*");
    }

    [Fact]
    public void IsGreaterThan_WhenAmountIsGreater_ShouldReturnTrue()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(50m, "USD");

        a.IsGreaterThan(b).Should().BeTrue();
    }

    [Fact]
    public void IsGreaterThan_WhenAmountIsEqual_ShouldReturnFalse()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(100m, "USD");

        a.IsGreaterThan(b).Should().BeFalse();
    }

    [Fact]
    public void IsLessThan_WhenAmountIsLess_ShouldReturnTrue()
    {
        var a = Money.Of(50m, "USD");
        var b = Money.Of(100m, "USD");

        a.IsLessThan(b).Should().BeTrue();
    }

    [Fact]
    public void OperatorPlus_ShouldAddMoneyValues()
    {
        var a = Money.Of(30m, "USD");
        var b = Money.Of(20m, "USD");

        var result = a + b;

        result.Amount.Should().Be(50m);
    }

    [Fact]
    public void OperatorMinus_ShouldSubtractMoneyValues()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(25m, "USD");

        var result = a - b;

        result.Amount.Should().Be(75m);
    }

    [Fact]
    public void OperatorMultiply_ShouldMultiplyByFactor()
    {
        var money = Money.Of(40m, "USD");

        var result = money * 2.5m;

        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void OperatorGreaterThan_ShouldCompareCorrectly()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(50m, "USD");

        (a > b).Should().BeTrue();
        (b > a).Should().BeFalse();
    }

    [Fact]
    public void OperatorLessThan_ShouldCompareCorrectly()
    {
        var a = Money.Of(50m, "USD");
        var b = Money.Of(100m, "USD");

        (a < b).Should().BeTrue();
        (b < a).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturn1()
    {
        var money = Money.Of(100m, "USD");

        money.CompareTo(null).Should().Be(1);
    }

    [Fact]
    public void CompareTo_WithSameAmount_ShouldReturn0()
    {
        var a = Money.Of(100m, "USD");
        var b = Money.Of(100m, "USD");

        a.CompareTo(b).Should().Be(0);
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "USD");

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_WithDifferentAmount_ShouldNotBeEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(200m, "USD");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_WithDifferentCurrency_ShouldNotBeEqual()
    {
        var a = new Money(100m, "USD");
        var b = new Money(100m, "EUR");

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var money = new Money(99.99m, "USD");

        money.ToString().Should().Be("99.99 USD");
    }
}
