namespace Payment.Application.Helpers;

public static class DateTimeHelper
{
    public static DateTimeOffset GetPeriodStartDate(string period)
    {
        var now = DateTimeOffset.UtcNow;
        return period.ToLower() switch
        {
            "day" => now.AddDays(-1),
            "week" => now.AddDays(-7),
            "month" => now.AddMonths(-1),
            "year" => now.AddYears(-1),
            _ => now.AddMonths(-1)
        };
    }
}
