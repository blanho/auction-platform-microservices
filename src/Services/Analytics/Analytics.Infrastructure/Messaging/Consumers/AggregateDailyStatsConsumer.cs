using Analytics.Domain.Constants;
using Analytics.Infrastructure.Persistence;
using AnalyticsService.Contracts.Commands;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Messaging.Consumers;

public sealed class AggregateDailyStatsConsumer(AnalyticsDbContext db) : IConsumer<AggregateDailyStatsCommand>
{
    public const int MaxDaysPerItem = 31;

    public async Task Consume(ConsumeContext<AggregateDailyStatsCommand> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;
        ct.ThrowIfCancellationRequested();
        if (message.ParentJobId is null || message.ParentJobItemId is null)
            throw new ArgumentException("A parent job and item are required.");

        string? error = null;
        if (message.RequestedBy == Guid.Empty || message.StartDate == default ||
            message.EndDate < message.StartDate ||
            message.EndDate.DayNumber - message.StartDate.DayNumber >= MaxDaysPerItem ||
            message.EndDate > DateOnly.FromDateTime(DateTime.UtcNow))
            error = "Provide a requester and an inclusive range of 1–31 UTC dates, ending no later than today.";

        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(ct) : null;
        if (error is null)
        {
            await db.Database.ExecuteSqlRawAsync("""
                LOCK TABLE analytics.daily_auction_stats, analytics.daily_bid_stats,
                    analytics.daily_revenue_stats IN SHARE ROW EXCLUSIVE MODE
                """, ct);
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                DELETE FROM analytics.daily_auction_stats WHERE date_key BETWEEN {message.StartDate} AND {message.EndDate};
                DELETE FROM analytics.daily_bid_stats WHERE date_key BETWEEN {message.StartDate} AND {message.EndDate};
                DELETE FROM analytics.daily_revenue_stats WHERE date_key BETWEEN {message.StartDate} AND {message.EndDate};
                """, ct);
            await RebuildAsync(message.StartDate, message.EndDate, ct);
        }

        await context.Publish(new ReportJobItemResultCommand
        {
            JobId = message.ParentJobId.Value,
            JobItemId = message.ParentJobItemId.Value,
            Attempt = message.Attempt,
            IsSuccess = error is null,
            IsFinalFailure = error is not null,
            ErrorMessage = error
        }, ct);
        await db.SaveChangesAsync(ct);
        if (transaction is not null) await transaction.CommitAsync(ct);
    }

    private Task<int> RebuildAsync(DateOnly start, DateOnly end, CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"""
            WITH auction_stats AS (
                INSERT INTO analytics.daily_auction_stats
                    (date_key, event_type, event_count, unique_sellers, unique_winners,
                     total_revenue, avg_sale_price, min_sale_price, max_sale_price, sold_count, unsold_count)
                SELECT "DateKey", "EventType", count(*),
                    count(DISTINCT NULLIF("SellerId", '00000000-0000-0000-0000-000000000000'::uuid)),
                    count(DISTINCT NULLIF("WinnerId", '00000000-0000-0000-0000-000000000000'::uuid)),
                    coalesce(sum("FinalPrice") FILTER (WHERE "Sold" AND "EventType" = {AnalyticsEventTypes.Finished}), 0),
                    avg("FinalPrice") FILTER (WHERE "Sold" AND "EventType" = {AnalyticsEventTypes.Finished}),
                    min("FinalPrice") FILTER (WHERE "Sold" AND "EventType" = {AnalyticsEventTypes.Finished}),
                    max("FinalPrice") FILTER (WHERE "Sold" AND "EventType" = {AnalyticsEventTypes.Finished}),
                    count(*) FILTER (WHERE "Sold" AND "EventType" = {AnalyticsEventTypes.Finished}),
                    count(*) FILTER (WHERE NOT "Sold" AND "EventType" = {AnalyticsEventTypes.Finished})
                FROM analytics.fact_auctions WHERE "DateKey" BETWEEN {start} AND {end}
                GROUP BY "DateKey", "EventType"
            ), bid_stats AS (
                INSERT INTO analytics.daily_bid_stats
                    (date_key, total_bids, unique_bidders, auctions_with_bids,
                     total_bid_value, avg_bid_amount, min_bid_amount, max_bid_amount)
                SELECT "DateKey", count(*), count(DISTINCT "BidderId"), count(DISTINCT "AuctionId"),
                    sum("BidAmount"), avg("BidAmount"), min("BidAmount"), max("BidAmount")
                FROM analytics.fact_bids WHERE "DateKey" BETWEEN {start} AND {end}
                GROUP BY "DateKey"
            )
            INSERT INTO analytics.daily_revenue_stats
                (date_key, event_type, transaction_count, unique_buyers, unique_sellers,
                 total_revenue, avg_transaction_amount, refunded_amount, refund_count)
            SELECT "DateKey", "EventType", count(*), count(DISTINCT "BuyerId"),
                count(DISTINCT NULLIF("SellerId", '00000000-0000-0000-0000-000000000000'::uuid)),
                coalesce(sum("TotalAmount") FILTER (WHERE "EventType" = {AnalyticsEventTypes.PaymentCompleted} AND "IsPaid" AND NOT "IsRefunded"), 0),
                avg("TotalAmount") FILTER (WHERE "EventType" = {AnalyticsEventTypes.PaymentCompleted} AND "IsPaid" AND NOT "IsRefunded"),
                coalesce(sum("TotalAmount") FILTER (WHERE "IsRefunded"), 0),
                count(*) FILTER (WHERE "IsRefunded")
            FROM analytics.fact_payments WHERE "DateKey" BETWEEN {start} AND {end}
            GROUP BY "DateKey", "EventType"
            """, ct);
}
