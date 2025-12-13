using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public Guid AuctionId { get; set; }
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
