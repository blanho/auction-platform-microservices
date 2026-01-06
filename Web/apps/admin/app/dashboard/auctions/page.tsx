"use client";

import { useState } from "react";
import { useAuctions } from "@repo/hooks";
import { formatCurrency, formatDateTime } from "@repo/utils";
import type { AuctionStatus } from "@repo/types";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
  Button,
  Input,
  Badge,
  Skeleton,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@repo/ui";
import { Search, Eye, Ban } from "lucide-react";

const statusColors: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-800",
  Pending: "bg-yellow-100 text-yellow-800",
  Live: "bg-green-100 text-green-800",
  Finished: "bg-blue-100 text-blue-800",
  Cancelled: "bg-red-100 text-red-800",
};

export default function AdminAuctionsPage() {
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");

  const { data: auctions, isLoading } = useAuctions({
    searchTerm: search || undefined,
    status: statusFilter === "all" ? undefined : (statusFilter as AuctionStatus),
    pageSize: 20,
  });

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold">Auctions</h1>
        <p className="mt-2 text-muted-foreground">
          View and moderate all platform auctions
        </p>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search auctions..."
            className="pl-9"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-40">
            <SelectValue placeholder="Status" />
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

      <Card>
        <CardHeader>
          <CardTitle>All Auctions ({auctions?.totalCount || 0})</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-3">
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} className="h-16" />
              ))}
            </div>
          ) : !auctions?.items.length ? (
            <p className="py-8 text-center text-muted-foreground">
              No auctions found
            </p>
          ) : (
            <div className="rounded-md border">
              <table className="w-full">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="p-3 text-left text-sm font-medium">Auction</th>
                    <th className="p-3 text-left text-sm font-medium">Seller</th>
                    <th className="p-3 text-left text-sm font-medium">Price</th>
                    <th className="p-3 text-left text-sm font-medium">Status</th>
                    <th className="p-3 text-left text-sm font-medium">End Date</th>
                    <th className="p-3 text-right text-sm font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {auctions.items.map((auction) => (
                    <tr key={auction.id} className="border-b">
                      <td className="p-3">
                        <div className="flex items-center gap-3">
                          <div
                            className="h-10 w-10 rounded bg-muted bg-cover bg-center"
                            style={{
                              backgroundImage: auction.imageUrls[0]
                                ? `url(${auction.imageUrls[0]})`
                                : undefined,
                            }}
                          />
                          <div>
                            <p className="font-medium line-clamp-1">{auction.title}</p>
                            <p className="text-xs text-muted-foreground">
                              {auction.category?.name}
                            </p>
                          </div>
                        </div>
                      </td>
                      <td className="p-3 text-sm">
                        {auction.seller?.userName || "-"}
                      </td>
                      <td className="p-3 text-sm font-medium">
                        {formatCurrency(auction.currentHighBid || auction.reservePrice)}
                      </td>
                      <td className="p-3">
                        <Badge className={statusColors[auction.status]}>
                          {auction.status}
                        </Badge>
                      </td>
                      <td className="p-3 text-sm text-muted-foreground">
                        {formatDateTime(auction.auctionEnd)}
                      </td>
                      <td className="p-3 text-right">
                        <Button variant="ghost" size="icon">
                          <Eye className="h-4 w-4" />
                        </Button>
                        {auction.status === "Live" && (
                          <Button variant="ghost" size="icon" className="text-destructive">
                            <Ban className="h-4 w-4" />
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
