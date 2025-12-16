#nullable enable
using Common.Domain.Entities;

namespace AuctionService.Domain.Entities;

public class AuctionQuestion : BaseEntity
{
    public Guid AuctionId { get; set; }
    public Auction? Auction { get; set; }
    public Guid AskerId { get; set; }
    public string AskerUsername { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public DateTimeOffset? AnsweredAt { get; set; }
    public bool IsPublic { get; set; } = true;
}
