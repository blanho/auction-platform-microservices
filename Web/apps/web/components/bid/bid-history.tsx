"use client";

import { useAuctionBids } from "@repo/hooks";
import { formatCurrency, formatRelativeTime } from "@repo/utils";
import { Skeleton } from "@repo/ui";

interface BidHistoryProps {
  auctionId: string;
}

export function BidHistory({ auctionId }: BidHistoryProps) {
  const { data: bids, isLoading } = useAuctionBids(auctionId);

  if (isLoading) {
    return (
      <div className="space-y-3">
        {Array.from({ length: 3 }).map((_, i) => (
          <div key={i} className="flex items-center justify-between">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-4 w-16" />
          </div>
        ))}
      </div>
    );
  }

  if (!bids?.length) {
    return (
      <p className="text-center text-sm text-muted-foreground">
        No bids yet. Be the first to bid!
      </p>
    );
  }

  return (
    <div className="max-h-64 space-y-2 overflow-y-auto">
      {bids.map((bid, index) => (
        <div
          key={bid.id}
          className={`flex items-center justify-between rounded-lg p-2 ${
            index === 0 ? "bg-primary/10" : "bg-muted/50"
          }`}
        >
          <div className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-xs font-medium">
              {bid.bidderUsername?.[0]?.toUpperCase() || "?"}
            </div>
            <div>
              <p className="text-sm font-medium">
                {bid.bidderUsername || "Anonymous"}
                {index === 0 && (
                  <span className="ml-2 text-xs text-primary">(Highest)</span>
                )}
              </p>
              <p className="text-xs text-muted-foreground">
                {formatRelativeTime(bid.bidTime)}
              </p>
            </div>
          </div>
          <p className="font-semibold">{formatCurrency(bid.amount)}</p>
        </div>
      ))}
    </div>
  );
}
