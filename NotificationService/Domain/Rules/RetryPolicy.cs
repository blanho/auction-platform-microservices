using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Rules;

public static class RetryPolicy
{
    private static readonly Dictionary<ChannelType, int> MaxRetries = new()
    {
        { ChannelType.Email, 3 },
        { ChannelType.Sms, 2 },
        { ChannelType.Push, 2 },
        { ChannelType.InApp, 1 }
    };

    private static readonly Dictionary<ChannelType, TimeSpan[]> RetryDelays = new()
    {
        { ChannelType.Email, new[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30) } },
        { ChannelType.Sms, new[] { TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5) } },
        { ChannelType.Push, new[] { TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2) } },
        { ChannelType.InApp, new[] { TimeSpan.FromSeconds(5) } }
    };

    public static int GetMaxRetries(ChannelType channel)
    {
        return MaxRetries.TryGetValue(channel, out var retries) ? retries : 1;
    }

    public static TimeSpan GetRetryDelay(ChannelType channel, int attemptNumber)
    {
        if (!RetryDelays.TryGetValue(channel, out var delays))
            return TimeSpan.FromMinutes(1);

        var index = Math.Min(attemptNumber, delays.Length - 1);
        return delays[index];
    }

    public static bool ShouldRetry(ChannelType channel, int currentAttempt, string? errorMessage)
    {
        if (currentAttempt >= GetMaxRetries(channel))
            return false;

        if (IsPermanentError(errorMessage))
            return false;

        return true;
    }

    private static bool IsPermanentError(string? errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return false;

        var permanentErrors = new[]
        {
            "invalid email",
            "invalid phone",
            "unsubscribed",
            "blocked",
            "invalid recipient",
            "mailbox not found"
        };

        return permanentErrors.Any(e => 
            errorMessage.Contains(e, StringComparison.OrdinalIgnoreCase));
    }
}
