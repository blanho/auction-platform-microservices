"use client";

import { useState, useEffect, useCallback } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import {
    Plus,
    Loader2,
    LayoutGrid,
    TableIcon,
    Clock,
    Eye,
    Edit,
    Trash2,
    MoreHorizontal,
} from "lucide-react";

import { formatCurrency, getAuctionTitle } from "@/utils";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import {
    Table,
    TableBody,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
    Tabs,
    TabsList,
    TabsTrigger,
} from "@/components/ui/tabs";
import {
    ToggleGroup,
    ToggleGroupItem,
} from "@/components/ui/toggle-group";
import { DashboardLayout } from "@/components/layout/dashboard-layout";
import { auctionService } from "@/services/auction.service";
import { Auction, AuctionStatus } from "@/types/auction";

const statusConfig: Record<
    AuctionStatus,
    { label: string; color: string; bgColor: string }
> = {
    [AuctionStatus.Live]: {
        label: "Active",
        color: "text-green-600",
        bgColor: "bg-green-500/10",
    },
    [AuctionStatus.Inactive]: {
        label: "Pending",
        color: "text-yellow-600",
        bgColor: "bg-yellow-500/10",
    },
    [AuctionStatus.Finished]: {
        label: "Completed",
        color: "text-blue-600",
        bgColor: "bg-blue-500/10",
    },
    [AuctionStatus.ReserveNotMet]: {
        label: "Reserve Not Met",
        color: "text-orange-600",
        bgColor: "bg-orange-500/10",
    },
    [AuctionStatus.Cancelled]: {
        label: "Cancelled",
        color: "text-red-600",
        bgColor: "bg-red-500/10",
    },
};

function CountdownTimer({ endTime }: { endTime: Date }) {
    const [timeLeft, setTimeLeft] = useState("");

    useEffect(() => {
        const calculateTimeLeft = () => {
            const difference = endTime.getTime() - Date.now();
            if (difference <= 0) {
                setTimeLeft("Ended");
                return;
            }

            const days = Math.floor(difference / (1000 * 60 * 60 * 24));
            const hours = Math.floor(
                (difference % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60)
            );
            const minutes = Math.floor(
                (difference % (1000 * 60 * 60)) / (1000 * 60)
            );

            if (days > 0) {
                setTimeLeft(`${days}d ${hours}h`);
            } else if (hours > 0) {
                setTimeLeft(`${hours}h ${minutes}m`);
            } else {
                setTimeLeft(`${minutes}m`);
            }
        };

        calculateTimeLeft();
        const timer = setInterval(calculateTimeLeft, 60000);
        return () => clearInterval(timer);
    }, [endTime]);

    return <span>{timeLeft}</span>;
}

export default function MyListingsPage() {
    const { status } = useSession();
    const router = useRouter();
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [viewMode, setViewMode] = useState<"table" | "grid">("table");
    const [activeTab, setActiveTab] = useState<string>("all");

    const fetchAuctions = useCallback(async () => {
        try {
            const response = await auctionService.getMyAuctions();
            setAuctions(response.items);
        } catch (error) {\n        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push("/auth/signin?callbackUrl=/dashboard/listings");
            return;
        }

        if (status === "authenticated") {
            fetchAuctions();
        }
    }, [status, router, fetchAuctions]);

    const filteredAuctions = auctions.filter((auction) => {
        if (activeTab === "all") return true;
        if (activeTab === "active") return auction.status === AuctionStatus.Live;
        if (activeTab === "pending")
            return auction.status === AuctionStatus.Inactive;
        if (activeTab === "completed")
            return (
                auction.status === AuctionStatus.Finished ||
                auction.status === AuctionStatus.ReserveNotMet
            );
        return true;
    });

    const stats = {
        all: auctions.length,
        active: auctions.filter((a) => a.status === AuctionStatus.Live).length,
        pending: auctions.filter((a) => a.status === AuctionStatus.Inactive)
            .length,
        completed: auctions.filter(
            (a) =>
                a.status === AuctionStatus.Finished ||
                a.status === AuctionStatus.ReserveNotMet
        ).length,
    };

    if (status === "loading" || isLoading) {
        return (
            <DashboardLayout
                title="My Listings"
                description="Manage your auction listings"
            >
                <div className="flex justify-center py-12">
                    <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
                </div>
            </DashboardLayout>
        );
    }

    return (
        <DashboardLayout
            title="My Listings"
            description="Manage your auction listings"
        >
            {/* Header Actions */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-6">
                <Tabs value={activeTab} onValueChange={setActiveTab}>
                    <TabsList>
                        <TabsTrigger value="all">
                            All ({stats.all})
                        </TabsTrigger>
                        <TabsTrigger value="active">
                            Active ({stats.active})
                        </TabsTrigger>
                        <TabsTrigger value="pending">
                            Pending ({stats.pending})
                        </TabsTrigger>
                        <TabsTrigger value="completed">
                            Completed ({stats.completed})
                        </TabsTrigger>
                    </TabsList>
                </Tabs>

                <div className="flex items-center gap-2">
                    <ToggleGroup
                        type="single"
                        value={viewMode}
                        onValueChange={(value) =>
                            value && setViewMode(value as "table" | "grid")
                        }
                    >
                        <ToggleGroupItem value="table" aria-label="Table view">
                            <TableIcon className="h-4 w-4" />
                        </ToggleGroupItem>
                        <ToggleGroupItem value="grid" aria-label="Grid view">
                            <LayoutGrid className="h-4 w-4" />
                        </ToggleGroupItem>
                    </ToggleGroup>

                    <Button
                        className="bg-amber-500 hover:bg-amber-600"
                        asChild
                    >
                        <Link href="/auctions/create">
                            <Plus className="h-4 w-4 mr-2" />
                            New Listing
                        </Link>
                    </Button>
                </div>
            </div>

            {filteredAuctions.length === 0 ? (
                <Card>
                    <CardContent className="flex flex-col items-center justify-center py-12">
                        <div className="h-16 w-16 rounded-full bg-zinc-100 dark:bg-zinc-800 flex items-center justify-center mb-4">
                            <Plus className="h-8 w-8 text-zinc-400" />
                        </div>
                        <h3 className="text-xl font-semibold mb-2">
                            No listings yet
                        </h3>
                        <p className="text-zinc-500 mb-4 text-center">
                            Create your first auction listing to start selling
                        </p>
                        <Button
                            className="bg-amber-500 hover:bg-amber-600"
                            asChild
                        >
                            <Link href="/auctions/create">Create Listing</Link>
                        </Button>
                    </CardContent>
                </Card>
            ) : viewMode === "table" ? (
                <Card>
                    <CardContent className="p-0">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="w-[300px]">
                                        Item
                                    </TableHead>
                                    <TableHead>Current Bid</TableHead>
                                    <TableHead>Status</TableHead>
                                    <TableHead>Ends In</TableHead>
                                    <TableHead className="text-right">
                                        Actions
                                    </TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {filteredAuctions.map((auction) => (
                                    <TableRow key={auction.id}>
                                        <TableCell>
                                            <div className="flex items-center gap-3">
                                                <div className="relative h-12 w-16 rounded-md overflow-hidden bg-zinc-100">
                                                    {auction.files?.[0]?.url && (
                                                        <Image
                                                            src={
                                                                auction.files[0]
                                                                    .url
                                                            }
                                                            alt={getAuctionTitle(auction)}
                                                            fill
                                                            className="object-cover"
                                                        />
                                                    )}
                                                </div>
                                                <div>
                                                    <p className="font-medium">
                                                        {getAuctionTitle(auction)}
                                                    </p>
                                                    <p className="text-xs text-zinc-500">
                                                        {auction.categoryName}
                                                    </p>
                                                </div>
                                            </div>
                                        </TableCell>
                                        <TableCell>
                                            <span className="font-semibold">
                                                {formatCurrency(
                                                    auction.currentHighBid ||
                                                    auction.reservePrice
                                                )}
                                            </span>
                                        </TableCell>
                                        <TableCell>
                                            <Badge
                                                className={`${
                                                    statusConfig[auction.status]
                                                        .bgColor
                                                } ${
                                                    statusConfig[auction.status]
                                                        .color
                                                }`}
                                            >
                                                {
                                                    statusConfig[auction.status]
                                                        .label
                                                }
                                            </Badge>
                                        </TableCell>
                                        <TableCell>
                                            <div className="flex items-center gap-1 text-zinc-500">
                                                <Clock className="h-4 w-4" />
                                                <CountdownTimer
                                                    endTime={
                                                        new Date(
                                                            auction.auctionEnd
                                                        )
                                                    }
                                                />
                                            </div>
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <DropdownMenu>
                                                <DropdownMenuTrigger asChild>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                    >
                                                        <MoreHorizontal className="h-4 w-4" />
                                                    </Button>
                                                </DropdownMenuTrigger>
                                                <DropdownMenuContent align="end">
                                                    <DropdownMenuItem asChild>
                                                        <Link
                                                            href={`/auctions/${auction.id}`}
                                                        >
                                                            <Eye className="h-4 w-4 mr-2" />
                                                            View
                                                        </Link>
                                                    </DropdownMenuItem>
                                                    <DropdownMenuItem asChild>
                                                        <Link
                                                            href={`/auctions/${auction.id}/edit`}
                                                        >
                                                            <Edit className="h-4 w-4 mr-2" />
                                                            Edit
                                                        </Link>
                                                    </DropdownMenuItem>
                                                    <DropdownMenuItem className="text-red-600">
                                                        <Trash2 className="h-4 w-4 mr-2" />
                                                        Delete
                                                    </DropdownMenuItem>
                                                </DropdownMenuContent>
                                            </DropdownMenu>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </CardContent>
                </Card>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {filteredAuctions.map((auction) => (
                        <Card
                            key={auction.id}
                            className="overflow-hidden hover:shadow-lg transition-shadow"
                        >
                            <div className="relative h-48">
                                {auction.files?.[0]?.url ? (
                                    <Image
                                        src={auction.files[0].url}
                                        alt={getAuctionTitle(auction)}
                                        fill
                                        className="object-cover"
                                    />
                                ) : (
                                    <div className="h-full bg-zinc-100 dark:bg-zinc-800" />
                                )}
                                <div className="absolute top-3 left-3">
                                    <Badge
                                        className={`${
                                            statusConfig[auction.status].bgColor
                                        } ${
                                            statusConfig[auction.status].color
                                        }`}
                                    >
                                        {statusConfig[auction.status].label}
                                    </Badge>
                                </div>
                            </div>
                            <CardContent className="p-4">
                                <h3 className="font-semibold truncate mb-2">
                                    {getAuctionTitle(auction)}
                                </h3>
                                <div className="flex items-center justify-between mb-3">
                                    <div>
                                        <p className="text-xs text-zinc-500">
                                            Current Bid
                                        </p>
                                        <p className="font-bold text-amber-500">
                                            {formatCurrency(
                                                auction.currentHighBid ||
                                                auction.reservePrice
                                            )}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        <p className="text-xs text-zinc-500">
                                            Ends in
                                        </p>
                                        <div className="flex items-center gap-1 text-sm">
                                            <Clock className="h-4 w-4" />
                                            <CountdownTimer
                                                endTime={
                                                    new Date(auction.auctionEnd)
                                                }
                                            />
                                        </div>
                                    </div>
                                </div>
                                <div className="flex gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="flex-1"
                                        asChild
                                    >
                                        <Link href={`/auctions/${auction.id}`}>
                                            View
                                        </Link>
                                    </Button>
                                    <Button
                                        size="sm"
                                        className="flex-1 bg-amber-500 hover:bg-amber-600"
                                        asChild
                                    >
                                        <Link
                                            href={`/auctions/${auction.id}/edit`}
                                        >
                                            Edit
                                        </Link>
                                    </Button>
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}
        </DashboardLayout>
    );
}
