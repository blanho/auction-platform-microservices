"use client";

import { useAuction, useAuctionUpdates, useAuth } from "@repo/hooks";
import { formatCurrency } from "@repo/utils";
import type { AuctionDto } from "@repo/types";
import { Card, CardContent, CardHeader, CardTitle, Badge, Separator } from "@repo/ui";
import { BidForm } from "./bid-form";
import { BidHistory } from "./bid-history";
import { CountdownTimer } from "./countdown-timer";

interface BidSectionProps {
  auctionId: string;
  initialData: AuctionDto;
}

export function BidSection({ auctionId, initialData }: BidSectionProps) {
  const { data: auction } = useAuction(auctionId, {
    initialData,
    refetchInterval: 30000,
  });

  const { user } = useAuth();

  useAuctionUpdates(auctionId);

  const currentAuction = auction || initialData;
  const isLive = currentAuction.status === "Live";
  const isOwner = user?.id === currentAuction.seller?.id;

  return (
    <div className="space-y-4">
      <Card>
        <CardHeader className="pb-3">
          <div className="flex items-center justify-between">
            <CardTitle className="text-lg">Current Bid</CardTitle>
            <Badge variant={isLive ? "default" : "secondary"}>
              {currentAuction.status}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="text-center">
            <p className="text-3xl font-bold text-primary">
              {formatCurrency(currentAuction.currentHighBid || currentAuction.reservePrice)}
            </p>
            {currentAuction.currentHighBid && (
              <p className="text-sm text-muted-foreground">
                {currentAuction.bidCount} bid{currentAuction.bidCount !== 1 ? "s" : ""}
              </p>
            )}
            {!currentAuction.currentHighBid && (
              <p className="text-sm text-muted-foreground">Starting bid</p>
            )}
          </div>

          <Separator />

          <div className="text-center">
            <p className="text-sm text-muted-foreground">Time Remaining</p>
            <CountdownTimer endTime={currentAuction.auctionEnd} />
          </div>

          {currentAuction.buyNowPrice && isLive && (
            <>
              <Separator />
              <div className="text-center">
                <p className="text-sm text-muted-foreground">Buy Now Price</p>
                <p className="text-xl font-bold">{formatCurrency(currentAuction.buyNowPrice)}</p>
              </div>
            </>
          )}
        </CardContent>
      </Card>

      {isLive && !isOwner && (
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-lg">Place a Bid</CardTitle>
          </CardHeader>
          <CardContent>
            <BidForm
              auctionId={auctionId}
              currentBid={currentAuction.currentHighBid}
              reservePrice={currentAuction.reservePrice}
              bidIncrement={currentAuction.bidIncrement || 1}
            />
          </CardContent>
        </Card>
      )}

      {isOwner && (
        <Card>
          <CardContent className="py-4 text-center text-muted-foreground">
            You cannot bid on your own auction
          </CardContent>
        </Card>
      )}

      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="text-lg">Bid History</CardTitle>
        </CardHeader>
        <CardContent>
          <BidHistory auctionId={auctionId} />
        </CardContent>
      </Card>
    </div>
  );
}
