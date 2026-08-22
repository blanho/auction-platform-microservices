using Analytics.Domain.Constants;

namespace Analytics.Application.Helpers;

public static class AnalyticsHelper
{
    public static decimal CalculatePercentageChange(decimal previousValue, decimal currentValue)
    {
        if (previousValue == 0)
            return currentValue > 0 ? 100 : 0;

        return ((currentValue - previousValue) / previousValue) * 100;
    }

    public static (DateTimeOffset startDate, DateTimeOffset endDate) GetDateRange(string timeRange)
    {
        var now = DateTimeOffset.UtcNow;

        return timeRange.ToLowerInvariant() switch
        {
            AnalyticsPeriods.SevenDays or AnalyticsPeriods.Week => (now.AddDays(-7), now),
            AnalyticsPeriods.ThirtyDays or AnalyticsPeriods.Month => (now.AddDays(-30), now),
            AnalyticsPeriods.NinetyDays or AnalyticsPeriods.Quarter => (now.AddDays(-90), now),
            AnalyticsPeriods.OneYear or AnalyticsPeriods.Year => (now.AddYears(-1), now),
            _ => (now.AddDays(-30), now)
        };
    }

    public static (DateTimeOffset previousStart, DateTimeOffset previousEnd) GetPreviousPeriod(
        DateTimeOffset startDate,
        DateTimeOffset endDate)
    {
        var periodLength = endDate - startDate;
        return (startDate - periodLength, startDate);
    }
}
