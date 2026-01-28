using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics.Api.Entities;

[Table("daily_bid_stats", Schema = "analytics")]
public class DailyBidStats
{
    [Key]
    [Column("date_key")]
    public DateOnly DateKey { get; set; }

    [Column("total_bids")]
    public long TotalBids { get; set; }

    [Column("unique_bidders")]
    public long UniqueBidders { get; set; }

    [Column("auctions_with_bids")]
    public long AuctionsWithBids { get; set; }

    [Column("total_bid_value")]
    public decimal TotalBidValue { get; set; }

    [Column("avg_bid_amount")]
    public decimal? AvgBidAmount { get; set; }

    [Column("min_bid_amount")]
    public decimal? MinBidAmount { get; set; }

    [Column("max_bid_amount")]
    public decimal? MaxBidAmount { get; set; }
}
