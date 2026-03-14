using System.ComponentModel.DataAnnotations;

namespace Auctions.Application.DTOs.Auctions;

public class ExtendAuctionDto
{
    [Required]
    [Range(1, 10080)]
    public int ExtensionMinutes { get; set; }
}

public class ExtendAuctionResponseDto
{
    public Guid AuctionId { get; set; }
    public DateTimeOffset NewAuctionEnd { get; set; }
}
