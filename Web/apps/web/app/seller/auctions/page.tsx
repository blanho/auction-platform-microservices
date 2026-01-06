"use client";

import Link from "next/link";
import { useState } from "react";
import { useMyAuctions } from "@repo/hooks";
import { formatCurrency, formatDateTime, formatCountdown } from "@repo/utils";
import type { AuctionStatus } from "@repo/types";
import {
  Card,
  CardContent,
  Button,
  Badge,
  Skeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@repo/ui";
import { Plus, Edit, Eye, MoreHorizontal } from "lucide-react";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-800",
  Pending: "bg-yellow-100 text-yellow-800",
  Live: "bg-green-100 text-green-800",
  Finished: "bg-blue-100 text-blue-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function SellerAuctionsPage() {
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const { data: auctions, isLoading } = useMyAuctions({
    status: statusFilter === "all" ? undefined : (statusFilter as AuctionStatus),
  });

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">My Auctions</h1>
          <p className="mt-2 text-muted-foreground">
            Manage and track your auction listings
          </p>
        </div>
        <Link href="/seller/auctions/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Create Auction
          </Button>
        </Link>
      </div>

      <div className="flex items-center gap-4">
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="Filter by status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Status</SelectItem>
            <SelectItem value="Draft">Draft</SelectItem>
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="Live">Live</SelectItem>
            <SelectItem value="Finished">Finished</SelectItem>
            <SelectItem value="Cancelled">Cancelled</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      ) : !auctions?.items.length ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <h3 className="text-lg font-semibold">No auctions found</h3>
            <p className="mt-2 text-sm text-muted-foreground">
              Create your first auction to start selling
            </p>
            <Link href="/seller/auctions/new" className="mt-4">
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Create Auction
              </Button>
            </Link>
          </CardContent>
        </Card>
      ) : (
        <div className="space-y-4">
          {auctions.items.map((auction) => (
            <Card key={auction.id}>
              <CardContent className="flex items-center gap-6 p-4">
                <div
                  className="h-20 w-20 flex-shrink-0 rounded-lg bg-muted bg-cover bg-center"
                  style={{
                    backgroundImage: auction.imageUrls[0]
                      ? `url(${auction.imageUrls[0]})`
                      : undefined,
                  }}
                />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold truncate">{auction.title}</h3>
                    <Badge className={statusColors[auction.status]}>
                      {auction.status}
                    </Badge>
                  </div>
                  <div className="mt-1 flex flex-wrap gap-4 text-sm text-muted-foreground">
                    <span>Reserve: {formatCurrency(auction.reservePrice)}</span>
                    {auction.currentHighBid && (
                      <span>Current: {formatCurrency(auction.currentHighBid)}</span>
                    )}
                    <span>{auction.bidCount} bids</span>
                    <span>{auction.viewCount} views</span>
                  </div>
                  {auction.status === "Live" && (
                    <p className="mt-1 text-sm text-primary">
                      Ends {formatCountdown(auction.auctionEnd)}
                    </p>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <Link href={`/auctions/${auction.id}`}>
                    <Button variant="ghost" size="icon">
                      <Eye className="h-4 w-4" />
                    </Button>
                  </Link>
                  {(auction.status === "Draft" || auction.status === "Pending") && (
                    <Link href={`/seller/auctions/${auction.id}/edit`}>
                      <Button variant="ghost" size="icon">
                        <Edit className="h-4 w-4" />
                      </Button>
                    </Link>
                  )}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
