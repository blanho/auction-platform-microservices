"use client";

import Link from "next/link";
import Image from "next/image";
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Auction } from "@/types/auction";
import { formatDistance } from "date-fns";
import { AuctionActions } from "./auction-actions";

interface AuctionCardWithActionsProps {
  auction: Auction;
  onActionComplete?: () => void;
}

export function AuctionCardWithActions({
  auction,
  onActionComplete
}: AuctionCardWithActionsProps) {
  const getStatusColor = (status: string) => {
    const statusUpper = status.toUpperCase();
    switch (statusUpper) {
      case "LIVE":
        return "bg-green-500";
      case "FINISHED":
        return "bg-gray-500";
      case "RESERVENOTMET":
      case "RESERVE_NOT_MET":
        return "bg-yellow-500";
      case "CANCELLED":
        return "bg-red-500";
      case "INACTIVE":
        return "bg-orange-500";
      default:
        return "bg-gray-500";
    }
  };

  const getTimeRemaining = () => {
    if (!auction.auctionEnd) return "Date not set";
    const endDate = new Date(auction.auctionEnd);
    if (isNaN(endDate.getTime())) return "Invalid date";
    return formatDistance(endDate, new Date(), { addSuffix: true });
  };

  const getImageUrl = () => {
    const primaryFile = auction.files?.find(f => f.isPrimary);
    return primaryFile?.url || auction.files?.[0]?.url;
  };

  const imageUrl = getImageUrl();
  const timeRemaining = getTimeRemaining();

  return (
    <Card className="overflow-hidden transition-all hover:shadow-lg">
      <CardHeader className="p-0">
        <div className="relative aspect-video w-full overflow-hidden bg-muted">
          {imageUrl ? (
            <Image
              src={imageUrl}
              alt={auction.title || "Auction item"}
              fill
              className="object-cover"
            />
          ) : (
            <div className="flex h-full items-center justify-center text-muted-foreground">
              No image
            </div>
          )}
          <div className="absolute right-2 top-2 flex items-center gap-2">
            <Badge className={getStatusColor(auction.status)}>
              {auction.status}
            </Badge>
          </div>
          <div className="absolute left-2 top-2">
            <AuctionActions auction={auction} onActionComplete={onActionComplete} />
          </div>
        </div>
      </CardHeader>
      <CardContent className="p-4">
        <h3 className="mb-2 text-lg font-semibold line-clamp-1">
          {auction.title || "Untitled"}
        </h3>
        <div className="space-y-1 text-sm">
          {auction.make && (
            <p className="text-muted-foreground">
              {auction.year} {auction.make} {auction.model}
            </p>
          )}
          {auction.description && (
            <p className="text-muted-foreground text-xs line-clamp-2">
              {auction.description}
            </p>
          )}
          <p className="font-semibold">
            Current Bid: ${auction.currentHighBid?.toLocaleString() || "0"}
          </p>
          {auction.reservePrice && (
            <p className="text-xs text-muted-foreground">
              Reserve: ${auction.reservePrice.toLocaleString()}
            </p>
          )}
          {auction.auctionEnd && (
            <p className="text-xs text-muted-foreground">
              Ends {timeRemaining}
            </p>
          )}
        </div>
      </CardContent>
      <CardFooter className="p-4 pt-0">
        <Button asChild className="w-full">
          <Link href={`/auctions/${auction.id}`}>View Details</Link>
        </Button>
      </CardFooter>
    </Card>
  );
}
