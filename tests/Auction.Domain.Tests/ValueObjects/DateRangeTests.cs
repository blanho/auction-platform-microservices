using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.Domain.ValueObjects;

namespace Auction.Domain.Tests.ValueObjects;

public class DateRangeTests
{
    private static readonly DateTimeOffset BaseDate = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidRange_ShouldCreateDateRange()
    {
        var start = BaseDate;
        var end = BaseDate.AddDays(7);

        var range = DateRange.Create(start, end);

        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Fact]
    public void Create_WhenEndBeforeStart_ShouldThrowDomainInvariantException()
    {
        var act = () => DateRange.Create(BaseDate, BaseDate.AddDays(-1));

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*end must be after start*");
    }

    [Fact]
    public void Create_WhenEndEqualsStart_ShouldThrowDomainInvariantException()
    {
        var act = () => DateRange.Create(BaseDate, BaseDate);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*end must be after start*");
    }

    [Fact]
    public void FromDuration_WithPositiveDuration_ShouldCreateDateRange()
    {
        var duration = TimeSpan.FromHours(48);

        var range = DateRange.FromDuration(BaseDate, duration);

        range.Start.Should().Be(BaseDate);
        range.End.Should().Be(BaseDate.Add(duration));
    }

    [Fact]
    public void FromDuration_WithZeroDuration_ShouldThrowDomainInvariantException()
    {
        var act = () => DateRange.FromDuration(BaseDate, TimeSpan.Zero);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Duration must be positive*");
    }

    [Fact]
    public void FromDuration_WithNegativeDuration_ShouldThrowDomainInvariantException()
    {
        var act = () => DateRange.FromDuration(BaseDate, TimeSpan.FromHours(-1));

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Duration must be positive*");
    }

    [Fact]
    public void Duration_ShouldReturnTimeDifference()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(3));

        range.Duration.Should().Be(TimeSpan.FromDays(3));
    }

    [Fact]
    public void TotalDays_ShouldReturnCorrectValue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        range.TotalDays.Should().Be(5.0);
    }

    [Fact]
    public void TotalHours_ShouldReturnCorrectValue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddHours(48));

        range.TotalHours.Should().Be(48.0);
    }

    [Fact]
    public void ContainsPoint_WhenPointInside_ShouldReturnTrue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(7));

        range.Contains(BaseDate.AddDays(3)).Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_WhenPointOnBoundary_ShouldReturnTrue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(7));

        range.Contains(BaseDate).Should().BeTrue();
        range.Contains(BaseDate.AddDays(7)).Should().BeTrue();
    }

    [Fact]
    public void ContainsPoint_WhenPointOutside_ShouldReturnFalse()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(7));

        range.Contains(BaseDate.AddDays(-1)).Should().BeFalse();
        range.Contains(BaseDate.AddDays(8)).Should().BeFalse();
    }

    [Fact]
    public void ContainsRange_WhenInnerRangeFitsInside_ShouldReturnTrue()
    {
        var outer = DateRange.Create(BaseDate, BaseDate.AddDays(10));
        var inner = DateRange.Create(BaseDate.AddDays(2), BaseDate.AddDays(8));

        outer.Contains(inner).Should().BeTrue();
    }

    [Fact]
    public void ContainsRange_WhenInnerRangeExceedsOuter_ShouldReturnFalse()
    {
        var outer = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var inner = DateRange.Create(BaseDate, BaseDate.AddDays(10));

        outer.Contains(inner).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WhenRangesOverlap_ShouldReturnTrue()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate.AddDays(3), BaseDate.AddDays(8));

        a.Overlaps(b).Should().BeTrue();
        b.Overlaps(a).Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WhenRangesDoNotOverlap_ShouldReturnFalse()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(3));
        var b = DateRange.Create(BaseDate.AddDays(5), BaseDate.AddDays(8));

        a.Overlaps(b).Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WhenRangesTouchAtBoundary_ShouldReturnFalse()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(3));
        var b = DateRange.Create(BaseDate.AddDays(3), BaseDate.AddDays(6));

        a.Overlaps(b).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenNowAfterEnd_ShouldReturnTrue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(1));

        range.IsExpired(BaseDate.AddDays(2)).Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenNowBeforeEnd_ShouldReturnFalse()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        range.IsExpired(BaseDate.AddDays(3)).Should().BeFalse();
    }

    [Fact]
    public void HasStarted_WhenNowAfterStart_ShouldReturnTrue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        range.HasStarted(BaseDate.AddDays(1)).Should().BeTrue();
    }

    [Fact]
    public void HasStarted_WhenNowBeforeStart_ShouldReturnFalse()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        range.HasStarted(BaseDate.AddDays(-1)).Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNowBetweenStartAndEnd_ShouldReturnTrue()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(10));

        range.IsActive(BaseDate.AddDays(5)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenNowBeforeStart_ShouldReturnFalse()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(10));

        range.IsActive(BaseDate.AddDays(-1)).Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNowAfterEnd_ShouldReturnFalse()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(10));

        range.IsActive(BaseDate.AddDays(11)).Should().BeFalse();
    }

    [Fact]
    public void ExtendBy_WithPositiveDuration_ShouldExtendEnd()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        var extended = range.ExtendBy(TimeSpan.FromDays(3));

        extended.Start.Should().Be(BaseDate);
        extended.End.Should().Be(BaseDate.AddDays(8));
    }

    [Fact]
    public void ExtendBy_WithZeroDuration_ShouldThrowDomainInvariantException()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        var act = () => range.ExtendBy(TimeSpan.Zero);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Extension must be a positive duration*");
    }

    [Fact]
    public void ShortenBy_WithValidReduction_ShouldShortenEnd()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(10));

        var shortened = range.ShortenBy(TimeSpan.FromDays(3));

        shortened.Start.Should().Be(BaseDate);
        shortened.End.Should().Be(BaseDate.AddDays(7));
    }

    [Fact]
    public void ShortenBy_WhenReductionExceedsDuration_ShouldThrowDomainInvariantException()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(2));

        var act = () => range.ShortenBy(TimeSpan.FromDays(5));

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*end before or equal to start*");
    }

    [Fact]
    public void ShortenBy_WithZeroReduction_ShouldThrowDomainInvariantException()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        var act = () => range.ShortenBy(TimeSpan.Zero);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Reduction must be a positive duration*");
    }

    [Fact]
    public void Intersection_WhenRangesOverlap_ShouldReturnOverlappingRange()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate.AddDays(3), BaseDate.AddDays(8));

        var intersection = a.Intersection(b);

        intersection.Should().NotBeNull();
        intersection!.Start.Should().Be(BaseDate.AddDays(3));
        intersection.End.Should().Be(BaseDate.AddDays(5));
    }

    [Fact]
    public void Intersection_WhenRangesDoNotOverlap_ShouldReturnNull()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(3));
        var b = DateRange.Create(BaseDate.AddDays(5), BaseDate.AddDays(8));

        a.Intersection(b).Should().BeNull();
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturn1()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        range.CompareTo(null).Should().Be(1);
    }

    [Fact]
    public void CompareTo_WithEarlierStart_ShouldReturnPositive()
    {
        var a = DateRange.Create(BaseDate.AddDays(1), BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        a.CompareTo(b).Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithSameStartDifferentEnd_ShouldCompareByEnd()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate, BaseDate.AddDays(3));

        a.CompareTo(b).Should().BePositive();
    }

    [Fact]
    public void Equality_WithSameStartAndEnd_ShouldBeEqual()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate, BaseDate.AddDays(5));

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_WithDifferentStart_ShouldNotBeEqual()
    {
        var a = DateRange.Create(BaseDate, BaseDate.AddDays(5));
        var b = DateRange.Create(BaseDate.AddDays(1), BaseDate.AddDays(5));

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var range = DateRange.Create(BaseDate, BaseDate.AddDays(1));

        range.ToString().Should().Contain("2025-06-01").And.Contain("→");
    }
}
