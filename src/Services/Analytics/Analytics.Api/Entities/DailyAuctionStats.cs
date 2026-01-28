using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics.Api.Entities;

[Table("daily_auction_stats", Schema = "analytics")]
public class DailyAuctionStats
{
    [Key]
    [Column("date_key")]
    public DateOnly DateKey { get; set; }

    [Key]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("event_count")]
    public long EventCount { get; set; }

    [Column("unique_sellers")]
    public long UniqueSellers { get; set; }

    [Column("unique_winners")]
    public long UniqueWinners { get; set; }

    [Column("total_revenue")]
    public decimal? TotalRevenue { get; set; }

    [Column("avg_sale_price")]
    public decimal? AvgSalePrice { get; set; }

    [Column("min_sale_price")]
    public decimal? MinSalePrice { get; set; }

    [Column("max_sale_price")]
    public decimal? MaxSalePrice { get; set; }

    [Column("sold_count")]
    public long SoldCount { get; set; }

    [Column("unsold_count")]
    public long UnsoldCount { get; set; }
}
