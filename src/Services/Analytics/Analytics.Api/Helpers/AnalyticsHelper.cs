namespace Analytics.Api.Helpers;

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
            "7d" or "week" => (now.AddDays(-7), now),
            "30d" or "month" => (now.AddDays(-30), now),
            "90d" or "quarter" => (now.AddDays(-90), now),
            "1y" or "year" => (now.AddYears(-1), now),
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
