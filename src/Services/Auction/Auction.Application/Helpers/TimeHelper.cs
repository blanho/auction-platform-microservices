namespace Auctions.Application.Helpers;

public static class TimeHelper
{
    public static string GetTimeRemainingText(TimeSpan threshold)
    {
        if (threshold.TotalHours >= 1)
            return $"{(int)threshold.TotalHours} hour";
        return $"{(int)threshold.TotalMinutes} minutes";
    }
}
