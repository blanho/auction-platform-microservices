#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class SavedSearch : BaseEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SearchQuery { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Condition { get; set; }
    public bool NotifyOnNewMatch { get; set; } = true;
    public NotificationFrequency NotificationFrequency { get; set; } = NotificationFrequency.Instant;
    public DateTimeOffset? LastNotifiedAt { get; set; }
}

public enum NotificationFrequency
{
    Instant = 0,
    Daily = 1,
    Weekly = 2
}
