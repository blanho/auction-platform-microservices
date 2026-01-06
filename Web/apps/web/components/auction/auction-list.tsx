"use client";

import Link from "next/link";
import Image from "next/image";
import { useAuctions } from "@repo/hooks";
import { formatCurrency, formatCountdown } from "@repo/utils";
import { Card, CardContent, Badge, Skeleton, Button } from "@repo/ui";
import type { AuctionStatus } from "@repo/types";

interface AuctionListProps {
  page?: number;
  categoryId?: string;
  sortBy?: string;
  minPrice?: number;
  maxPrice?: number;
  searchTerm?: string;
}

export function AuctionList({
  page = 1,
  categoryId,
  sortBy,
  minPrice,
  maxPrice,
  searchTerm,
}: AuctionListProps) {
  const { data, isLoading, error } = useAuctions({
    page,
    pageSize: 12,
    categoryId,
    sortBy: sortBy as "endingSoon" | "newlyListed" | "priceAsc" | "priceDesc" | undefined,
    minPrice,
    maxPrice,
    searchTerm,
    status: "Live" as AuctionStatus,
  });

  if (isLoading) {
    return <AuctionListSkeleton />;
  }

  if (error) {
    return (
      <div className="py-12 text-center">
        <p className="text-muted-foreground">Failed to load auctions</p>
        <Button variant="outline" className="mt-4" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </div>
    );
  }

  if (!data?.items.length) {
    return (
      <div className="py-12 text-center">
        <p className="text-muted-foreground">No auctions found</p>
      </div>
    );
  }

  return (
    <div>
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {data.items.map((auction) => (
          <Link key={auction.id} href={`/auctions/${auction.id}`}>
            <Card className="overflow-hidden transition-shadow hover:shadow-lg">
              <div className="relative aspect-square">
                <Image
                  src={auction.imageUrls[0] || "/placeholder.jpg"}
                  alt={auction.title}
                  fill
                  className="object-cover"
                />
                <Badge className="absolute right-2 top-2" variant="secondary">
                  {formatCountdown(auction.auctionEnd)}
                </Badge>
              </div>
              <CardContent className="p-4">
                <h3 className="line-clamp-1 font-semibold">{auction.title}</h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  {auction.category?.name}
                </p>
                <div className="mt-3 flex items-center justify-between">
                  <div>
                    <p className="text-xs text-muted-foreground">Current Bid</p>
                    <p className="font-bold text-primary">
                      {formatCurrency(auction.currentHighBid || auction.reservePrice)}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-xs text-muted-foreground">Bids</p>
                    <p className="font-medium">{auction.bidCount}</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      {data.totalPages > 1 && (
        <div className="mt-8 flex justify-center gap-2">
          {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((pageNum) => (
            <Link
              key={pageNum}
              href={`/auctions?page=${pageNum}${categoryId ? `&category=${categoryId}` : ""}${sortBy ? `&sortBy=${sortBy}` : ""}`}
            >
              <Button
                variant={pageNum === page ? "default" : "outline"}
                size="sm"
              >
                {pageNum}
              </Button>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

function AuctionListSkeleton() {
  return (
    <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: 6 }).map((_, i) => (
        <Card key={i} className="overflow-hidden">
          <Skeleton className="aspect-square" />
          <CardContent className="p-4">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="mt-2 h-3 w-1/2" />
            <div className="mt-3 flex justify-between">
              <Skeleton className="h-8 w-20" />
              <Skeleton className="h-8 w-12" />
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
