namespace Search.Infrastructure.Services;

public static class BidStateScript
{
    public const string Source = """
        if (ctx._source.status == params.soldStatus || ctx._source.status == params.finishedStatus ||
            (ctx._source.lastBidEventTicks != null &&
                (params.ticks < ctx._source.lastBidEventTicks ||
                 (params.ticks == ctx._source.lastBidEventTicks &&
                    (ctx._source.lastBidEventIsRetraction == true ||
                     (!params.isRetraction && params.price <= ctx._source.currentPrice)))))) {
            ctx.op = 'noop';
        } else {
            ctx._source.currentPrice = params.hasPrice ? params.price : ctx._source.reservePrice;
            ctx._source.lastBidEventTicks = params.ticks;
            ctx._source.lastBidEventIsRetraction = params.isRetraction;
            ctx._source.lastSyncedAt = params.syncedAt;
        }
        """;
}
