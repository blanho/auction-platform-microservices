using BuildingBlocks.Application.Abstractions.Providers;

namespace BuildingBlocks.Application.Implementations;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
