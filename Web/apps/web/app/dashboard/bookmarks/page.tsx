"use client";

import Link from "next/link";
import { useBookmarks, useToggleBookmark } from "@repo/hooks";
import { formatCurrency, formatCountdown } from "@repo/utils";
import { Card, CardContent, Button, Skeleton, Badge } from "@repo/ui";
import { Bookmark, X } from "lucide-react";
import Image from "next/image";

export default function BookmarksPage() {
  const { data: bookmarks, isLoading } = useBookmarks();
  const { mutate: toggleBookmark } = useToggleBookmark();

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Bookmarks</h1>
        <p className="mt-2 text-muted-foreground">
          Auctions you&apos;re watching
        </p>
      </div>

      {isLoading ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-64" />
          ))}
        </div>
      ) : !bookmarks?.length ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Bookmark className="h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No bookmarks yet</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Save auctions you&apos;re interested in to track them here
            </p>
            <Link href="/auctions" className="mt-4">
              <Button>Browse Auctions</Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {bookmarks.map((auction) => (
            <Card key={auction.id} className="overflow-hidden">
              <div className="relative aspect-video">
                <Image
                  src={auction.imageUrls[0] || "/placeholder.jpg"}
                  alt={auction.title}
                  fill
                  className="object-cover"
                />
                <Button
                  variant="destructive"
                  size="icon"
                  className="absolute right-2 top-2 h-8 w-8"
                  onClick={() => toggleBookmark(auction.id)}
                >
                  <X className="h-4 w-4" />
                </Button>
                <Badge className="absolute left-2 top-2" variant="secondary">
                  {formatCountdown(auction.auctionEnd)}
                </Badge>
              </div>
              <CardContent className="p-4">
                <Link href={`/auctions/${auction.id}`}>
                  <h3 className="line-clamp-1 font-semibold hover:underline">
                    {auction.title}
                  </h3>
                </Link>
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
                  <Link href={`/auctions/${auction.id}`}>
                    <Button size="sm">View</Button>
                  </Link>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
