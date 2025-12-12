"use client";

import { useEffect, useState, useCallback } from "react";
import { useSession } from "next-auth/react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import {
    Loader2,
    Heart,
    Clock,
    Trash2,
    ExternalLink,
    Grid3X3,
    List,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ToggleGroup, ToggleGroupItem } from "@/components/ui/toggle-group";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
    AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { DashboardLayout } from "@/components/layout/dashboard-layout";
import { PlaceBidDialog } from "@/features/bid/place-bid-dialog";

import { Auction, AuctionStatus } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { toast } from "sonner";

import { ROUTES, MESSAGES } from "@/constants";
import {
    formatCurrency,
    getTimeRemaining,
    watchlistStorage,
    getStatusColorSafe,
    getAuctionImageUrl,
} from "@/utils";

// ============================================================================
// Types
// ============================================================================
type ViewMode = "grid" | "list";

interface TimeLeft {
    hours: number;
    minutes: number;
    seconds: number;
}

// ============================================================================
// Constants
// ============================================================================
const PLACEHOLDER_IMAGE = "/placeholder-car.jpg";

// ============================================================================
// Helper Components
// ============================================================================
function CountdownTimer({ endTime }: { endTime: Date }) {
    const [timeLeft, setTimeLeft] = useState<TimeLeft>({ hours: 0, minutes: 0, seconds: 0 });

    useEffect(() => {
        const calculateTimeLeft = () => {
            const remaining = getTimeRemaining(endTime);
            setTimeLeft({
                hours: remaining.days * 24 + remaining.hours,
                minutes: remaining.minutes,
                seconds: remaining.seconds,
            });
        };

        calculateTimeLeft();
        const timer = setInterval(calculateTimeLeft, 1000);

        return () => clearInterval(timer);
    }, [endTime]);

    const formatTime = (value: number) => value.toString().padStart(2, "0");

    return (
        <span className="font-mono">
            {formatTime(timeLeft.hours)}:{formatTime(timeLeft.minutes)}:{formatTime(timeLeft.seconds)}
        </span>
    );
}

function EmptyWatchlist() {
    return (
        <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
                <Heart className="h-16 w-16 text-zinc-300 mb-4" />
                <h3 className="text-xl font-semibold mb-2">{MESSAGES.EMPTY.WATCHLIST}</h3>
                <p className="text-zinc-500 mb-4 text-center">
                    Start adding items to track their progress
                </p>
                <Button asChild>
                    <Link href={ROUTES.AUCTIONS.LIST}>Browse Auctions</Link>
                </Button>
            </CardContent>
        </Card>
    );
}

interface RemoveDialogProps {
    onConfirm: () => void;
    trigger: React.ReactNode;
}

function RemoveDialog({ onConfirm, trigger }: RemoveDialogProps) {
    return (
        <AlertDialog>
            <AlertDialogTrigger asChild>{trigger}</AlertDialogTrigger>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Remove from watchlist?</AlertDialogTitle>
                    <AlertDialogDescription>
                        This item will be removed from your watchlist.
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel>Cancel</AlertDialogCancel>
                    <AlertDialogAction
                        onClick={onConfirm}
                        className="bg-red-500 hover:bg-red-600"
                    >
                        Remove
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}

// ============================================================================
// Auction Card Components
// ============================================================================
interface AuctionCardProps {
    auction: Auction;
    onRemove: (id: string) => void;
}

function AuctionGridCard({ auction, onRemove }: AuctionCardProps) {
    const imageUrl = getAuctionImageUrl(auction.files, PLACEHOLDER_IMAGE);
    const isLive = auction.status === AuctionStatus.Live;

    return (
        <Card className="overflow-hidden group hover:shadow-lg transition-shadow">
            <div className="relative h-48">
                <Image
                    src={imageUrl}
                    alt={auction.title}
                    fill
                    className="object-cover"
                    sizes="(max-width: 768px) 100vw, 33vw"
                />
                <div className="absolute top-3 right-3">
                    <RemoveDialog
                        onConfirm={() => onRemove(auction.id)}
                        trigger={
                            <Button
                                variant="secondary"
                                size="icon"
                                className="h-8 w-8 bg-white/90 hover:bg-white"
                            >
                                <Heart className="h-4 w-4 fill-red-500 text-red-500" />
                            </Button>
                        }
                    />
                </div>
                {isLive && (
                    <div className="absolute bottom-3 left-3">
                        <Badge className={getStatusColorSafe(auction.status)}>Live</Badge>
                    </div>
                )}
            </div>
            <CardContent className="p-4">
                <h3 className="font-semibold truncate mb-2">
                    {auction.year} {auction.make} {auction.model}
                </h3>
                <div className="flex items-center justify-between mb-3">
                    <div>
                        <p className="text-xs text-zinc-500">Current Bid</p>
                        <p className="text-lg font-bold text-amber-500">
                            {formatCurrency(auction.currentHighBid)}
                        </p>
                    </div>
                    <div className="text-right">
                        <p className="text-xs text-zinc-500">Ends in</p>
                        <div className="flex items-center gap-1 text-sm">
                            <Clock className="h-4 w-4" />
                            <CountdownTimer endTime={new Date(auction.auctionEnd)} />
                        </div>
                    </div>
                </div>
                <Button className="w-full bg-amber-500 hover:bg-amber-600" asChild>
                    <Link href={ROUTES.AUCTIONS.DETAIL(auction.id)}>View Auction</Link>
                </Button>
            </CardContent>
        </Card>
    );
}

function AuctionListCard({ auction, onRemove }: AuctionCardProps) {
    const imageUrl = getAuctionImageUrl(auction.files, PLACEHOLDER_IMAGE);

    return (
        <Card className="overflow-hidden">
            <div className="flex flex-col sm:flex-row">
                <div className="relative w-full sm:w-48 h-32">
                    <Image
                        src={imageUrl}
                        alt={auction.title}
                        fill
                        className="object-cover"
                        sizes="(max-width: 640px) 100vw, 192px"
                    />
                </div>
                <CardContent className="flex-1 p-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                    <div>
                        <h3 className="font-semibold">
                            {auction.year} {auction.make} {auction.model}
                        </h3>
                        <p className="text-sm text-zinc-500">{auction.categoryName}</p>
                        <div className="flex items-center gap-4 mt-2">
                            <div>
                                <p className="text-xs text-zinc-500">Current Bid</p>
                                <p className="font-bold text-amber-500">
                                    {formatCurrency(auction.currentHighBid)}
                                </p>
                            </div>
                            <div>
                                <p className="text-xs text-zinc-500">Ends in</p>
                                <div className="flex items-center gap-1 text-sm">
                                    <Clock className="h-4 w-4" />
                                    <CountdownTimer endTime={new Date(auction.auctionEnd)} />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="flex gap-2">
                        <Button variant="outline" size="icon" asChild>
                            <Link href={ROUTES.AUCTIONS.DETAIL(auction.id)}>
                                <ExternalLink className="h-4 w-4" />
                            </Link>
                        </Button>
                        <RemoveDialog
                            onConfirm={() => onRemove(auction.id)}
                            trigger={
                                <Button
                                    variant="outline"
                                    size="icon"
                                    className="text-red-500 hover:text-red-600"
                                >
                                    <Trash2 className="h-4 w-4" />
                                </Button>
                            }
                        />
                        <PlaceBidDialog
                            auctionId={auction.id}
                            currentHighBid={auction.currentHighBid || 0}
                            reservePrice={auction.reservePrice}
                        />
                    </div>
                </CardContent>
            </div>
        </Card>
    );
}

// ============================================================================
// Main Component
// ============================================================================
export default function WatchlistPage() {
    const { status } = useSession();
    const router = useRouter();
    const [watchlist, setWatchlist] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [viewMode, setViewMode] = useState<ViewMode>("grid");

    const fetchWatchlistAuctions = useCallback(async () => {
        const watchlistIds = watchlistStorage.get();
        
        if (watchlistIds.length === 0) {
            setWatchlist([]);
            setIsLoading(false);
            return;
        }

        try {
            const auctionPromises = watchlistIds.map(id =>
                auctionService.getAuctionById(id).catch(() => null)
            );
            const results = await Promise.all(auctionPromises);
            const validAuctions = results.filter((auction): auction is Auction => auction !== null);
            setWatchlist(validAuctions);
        } catch (error) {
            console.error("Failed to fetch watchlist auctions:", error);
            toast.error(MESSAGES.ERROR.GENERIC);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        if (status === "unauthenticated") {
            router.push(`${ROUTES.AUTH.LOGIN}?callbackUrl=${ROUTES.DASHBOARD.WATCHLIST}`);
            return;
        }

        if (status === "authenticated") {
            fetchWatchlistAuctions();
        }
    }, [status, router, fetchWatchlistAuctions]);

    const handleRemoveFromWatchlist = useCallback((id: string) => {
        watchlistStorage.remove(id);
        setWatchlist(prev => prev.filter(item => item.id !== id));
        toast.success(MESSAGES.SUCCESS.WATCHLIST_REMOVED);
    }, []);

    const handleViewModeChange = useCallback((value: string) => {
        if (value) {
            setViewMode(value as ViewMode);
        }
    }, []);

    // Loading state
    if (status === "loading" || isLoading) {
        return (
            <DashboardLayout title="Watchlist" description="Items you're tracking">
                <div className="flex justify-center py-12">
                    <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
                </div>
            </DashboardLayout>
        );
    }

    // Render content
    const renderAuctionList = () => {
        if (watchlist.length === 0) {
            return <EmptyWatchlist />;
        }

        if (viewMode === "grid") {
            return (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {watchlist.map(auction => (
                        <AuctionGridCard
                            key={auction.id}
                            auction={auction}
                            onRemove={handleRemoveFromWatchlist}
                        />
                    ))}
                </div>
            );
        }

        return (
            <div className="space-y-4">
                {watchlist.map(auction => (
                    <AuctionListCard
                        key={auction.id}
                        auction={auction}
                        onRemove={handleRemoveFromWatchlist}
                    />
                ))}
            </div>
        );
    };

    return (
        <DashboardLayout
            title="Watchlist"
            description={`${watchlist.length} items you're tracking`}
        >
            {/* View Toggle */}
            <div className="flex justify-between items-center mb-6">
                <div className="flex items-center gap-2">
                    <Heart className="h-5 w-5 text-red-500" />
                    <span className="text-zinc-600 dark:text-zinc-400">
                        {watchlist.length} items saved
                    </span>
                </div>
                <ToggleGroup
                    type="single"
                    value={viewMode}
                    onValueChange={handleViewModeChange}
                >
                    <ToggleGroupItem value="grid" aria-label="Grid view">
                        <Grid3X3 className="h-4 w-4" />
                    </ToggleGroupItem>
                    <ToggleGroupItem value="list" aria-label="List view">
                        <List className="h-4 w-4" />
                    </ToggleGroupItem>
                </ToggleGroup>
            </div>

            {renderAuctionList()}
        </DashboardLayout>
    );
}
