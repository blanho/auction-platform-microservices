using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics.Api.Entities;

[Table("daily_revenue_stats", Schema = "analytics")]
public class DailyRevenueStats
{
    [Key]
    [Column("date_key")]
    public DateOnly DateKey { get; set; }

    [Key]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("transaction_count")]
    public long TransactionCount { get; set; }

    [Column("unique_buyers")]
    public long UniqueBuyers { get; set; }

    [Column("unique_sellers")]
    public long UniqueSellers { get; set; }

    [Column("total_revenue")]
    public decimal TotalRevenue { get; set; }

    [Column("avg_transaction_amount")]
    public decimal? AvgTransactionAmount { get; set; }

    [Column("refunded_amount")]
    public decimal? RefundedAmount { get; set; }

    [Column("refund_count")]
    public long RefundCount { get; set; }
}
