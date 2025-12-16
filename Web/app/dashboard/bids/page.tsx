"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { format } from "date-fns";
import {
    Loader2,
    ExternalLink,
    TrendingUp,
    TrendingDown,
    Clock,
    Filter,
} from "lucide-react";

import { formatCurrency, getAuctionTitle } from "@/utils";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { DashboardLayout } from "@/components/layout/dashboard-layout";
import { bidService } from "@/services/bid.service";
import { auctionService } from "@/services/auction.service";
import { Bid, BidStatus } from "@/types/bid";
import { Auction } from "@/types/auction";

interface BidWithAuction extends Bid {
    auction?: Auction;
}

const statusColors: Record<BidStatus, string> = {
    [BidStatus.Pending]: "bg-yellow-500/10 text-yellow-600",
    [BidStatus.Accepted]: "bg-green-500/10 text-green-600",
    [BidStatus.AcceptedBelowReserve]: "bg-blue-500/10 text-blue-600",
    [BidStatus.TooLow]: "bg-red-500/10 text-red-600",
    [BidStatus.Rejected]: "bg-red-500/10 text-red-600",
};

export default function MyBidsPage() {
    const { data: session, status } = useSession();
    const router = useRouter();
    const [bids, setBids] = useState<BidWithAuction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [filter, setFilter] = useState<string>("all");

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push("/auth/signin?callbackUrl=/dashboard/bids");
            return;
        }

        const fetchBids = async () => {
            if (!session?.user?.name) return;

            try {
                const bidData = await bidService.getMyBids();

                const uniqueAuctionIds = [...new Set(bidData.map(bid => bid.auctionId))];
                const auctions = await auctionService.getAuctionsByIds(uniqueAuctionIds);
                const auctionMap = new Map(auctions.map(a => [a.id, a]));

                const bidsWithAuctions = bidData.map(bid => ({
                    ...bid,
                    auction: auctionMap.get(bid.auctionId)
                }));

                setBids(bidsWithAuctions);
            } catch (error) {
            } finally {
                setIsLoading(false);
            }
        };

        if (status === "authenticated") {
            fetchBids();
        }
    }, [status, session, router]);

    const filteredBids = bids.filter((bid) => {
        if (filter === "all") return true;
        if (filter === "winning") return bid.status === BidStatus.Accepted;
        if (filter === "outbid")
            return (
                bid.status === BidStatus.TooLow ||
                bid.status === BidStatus.Rejected
            );
        if (filter === "pending") return bid.status === BidStatus.Pending;
        return true;
    });

    const stats = {
        total: bids.length,
        winning: bids.filter((b) => b.status === BidStatus.Accepted).length,
        outbid: bids.filter(
            (b) =>
                b.status === BidStatus.TooLow || b.status === BidStatus.Rejected
        ).length,
        pending: bids.filter((b) => b.status === BidStatus.Pending).length,
    };

    if (status === "loading" || isLoading) {
        return (
            <DashboardLayout
                title="My Bids"
                description="Track all your bidding activity"
            >
                <div className="flex justify-center py-12">
                    <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
                </div>
            </DashboardLayout>
        );
    }

    return (
        <DashboardLayout
            title="My Bids"
            description="Track all your bidding activity"
        >
            {/* Stats Cards */}
            <div className="grid gap-4 md:grid-cols-4 mb-8">
                <Card
                    className={`cursor-pointer transition-colors ${
                        filter === "all" ? "border-amber-500" : ""
                    }`}
                    onClick={() => setFilter("all")}
                >
                    <CardContent className="pt-6">
                        <div className="text-2xl font-bold">{stats.total}</div>
                        <p className="text-sm text-zinc-500">Total Bids</p>
                    </CardContent>
                </Card>
                <Card
                    className={`cursor-pointer transition-colors ${
                        filter === "winning" ? "border-green-500" : ""
                    }`}
                    onClick={() => setFilter("winning")}
                >
                    <CardContent className="pt-6">
                        <div className="text-2xl font-bold text-green-500">
                            {stats.winning}
                        </div>
                        <p className="text-sm text-zinc-500">Winning</p>
                    </CardContent>
                </Card>
                <Card
                    className={`cursor-pointer transition-colors ${
                        filter === "outbid" ? "border-red-500" : ""
                    }`}
                    onClick={() => setFilter("outbid")}
                >
                    <CardContent className="pt-6">
                        <div className="text-2xl font-bold text-red-500">
                            {stats.outbid}
                        </div>
                        <p className="text-sm text-zinc-500">Outbid</p>
                    </CardContent>
                </Card>
                <Card
                    className={`cursor-pointer transition-colors ${
                        filter === "pending" ? "border-yellow-500" : ""
                    }`}
                    onClick={() => setFilter("pending")}
                >
                    <CardContent className="pt-6">
                        <div className="text-2xl font-bold text-yellow-500">
                            {stats.pending}
                        </div>
                        <p className="text-sm text-zinc-500">Pending</p>
                    </CardContent>
                </Card>
            </div>

            {/* Bids Table */}
            <Card>
                <CardHeader className="flex flex-row items-center justify-between">
                    <CardTitle>Bidding History</CardTitle>
                    <Select value={filter} onValueChange={setFilter}>
                        <SelectTrigger className="w-[180px]">
                            <Filter className="h-4 w-4 mr-2" />
                            <SelectValue placeholder="Filter" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem value="all">All Bids</SelectItem>
                            <SelectItem value="winning">Winning</SelectItem>
                            <SelectItem value="outbid">Outbid</SelectItem>
                            <SelectItem value="pending">Pending</SelectItem>
                        </SelectContent>
                    </Select>
                </CardHeader>
                <CardContent>
                    {filteredBids.length === 0 ? (
                        <div className="text-center py-12">
                            <p className="text-zinc-500">No bids found</p>
                            <Button asChild className="mt-4">
                                <Link href="/auctions">Browse Auctions</Link>
                            </Button>
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead>Item</TableHead>
                                        <TableHead>Your Bid</TableHead>
                                        <TableHead>Current Bid</TableHead>
                                        <TableHead>Status</TableHead>
                                        <TableHead>Time Placed</TableHead>
                                        <TableHead className="text-right">
                                            Action
                                        </TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {filteredBids.map((bid) => (
                                        <TableRow key={bid.id}>
                                            <TableCell>
                                                <div className="flex items-center gap-3">
                                                    {bid.auction?.files?.[0]
                                                        ?.url && (
                                                        <div className="relative h-12 w-16 rounded-md overflow-hidden">
                                                            <Image
                                                                src={
                                                                    bid.auction
                                                                        .files[0]
                                                                        .url
                                                                }
                                                                alt={
                                                                    bid.auction
                                                                        ? getAuctionTitle(bid.auction)
                                                                        : "Auction"
                                                                }
                                                                fill
                                                                className="object-cover"
                                                            />
                                                        </div>
                                                    )}
                                                    <div>
                                                        <p className="font-medium">
                                                            {bid.auction
                                                                ? getAuctionTitle(bid.auction)
                                                                : "Unknown Item"}
                                                        </p>
                                                        <p className="text-xs text-zinc-500">
                                                            {bid.auction
                                                                ?.categoryName ||
                                                                ""}
                                                        </p>
                                                    </div>
                                                </div>
                                            </TableCell>
                                            <TableCell>
                                                <span className="font-semibold">
                                                    {formatCurrency(bid.amount)}
                                                </span>
                                            </TableCell>
                                            <TableCell>
                                                <div className="flex items-center gap-1">
                                                    {bid.auction
                                                        ?.currentHighBid &&
                                                    bid.auction.currentHighBid >
                                                        bid.amount ? (
                                                        <TrendingDown className="h-4 w-4 text-red-500" />
                                                    ) : (
                                                        <TrendingUp className="h-4 w-4 text-green-500" />
                                                    )}
                                                    <span>
                                                        {formatCurrency(
                                                            bid.auction?.currentHighBid ?? 0
                                                        )}
                                                    </span>
                                                </div>
                                            </TableCell>
                                            <TableCell>
                                                <Badge
                                                    className={
                                                        statusColors[bid.status]
                                                    }
                                                >
                                                    {bid.status}
                                                </Badge>
                                            </TableCell>
                                            <TableCell>
                                                <div className="flex items-center gap-1 text-zinc-500">
                                                    <Clock className="h-4 w-4" />
                                                    <span className="text-sm">
                                                        {format(
                                                            new Date(
                                                                bid.bidTime
                                                            ),
                                                            "MMM d, yyyy h:mm a"
                                                        )}
                                                    </span>
                                                </div>
                                            </TableCell>
                                            <TableCell className="text-right">
                                                <Button
                                                    variant="ghost"
                                                    size="sm"
                                                    asChild
                                                >
                                                    <Link
                                                        href={`/auctions/${bid.auctionId}`}
                                                    >
                                                        <ExternalLink className="h-4 w-4" />
                                                    </Link>
                                                </Button>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </div>
                    )}
                </CardContent>
            </Card>
        </DashboardLayout>
    );
}
