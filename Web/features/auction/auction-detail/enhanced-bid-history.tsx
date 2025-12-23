"use client";

import { useEffect, useState, useRef, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { bidService } from "@/services/bid.service";
import { Bid, BidStatus } from "@/types/bid";
import { formatDistanceToNow } from "date-fns";
import { cn } from "@/lib/utils";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faDollarSign,
    faClock,
    faCrown,
    faTrophy,
    faArrowUp,
    faRefresh,
} from "@fortawesome/free-solid-svg-icons";
import { getBidStatusStyle } from "@/constants/status";
import { formatCurrency } from "@/utils";

interface EnhancedBidHistoryProps {
    auctionId: string;
    refreshTrigger?: number;
    maxHeight?: string;
    enableRealtime?: boolean;
    isLive?: boolean;
    pollingInterval?: number;
}

const DEFAULT_MAX_HEIGHT = "400px";
const DEFAULT_POLLING_INTERVAL = 5000;

function sortBidsByTime(bids: Bid[]): Bid[] {
    return [...bids].sort(
        (a, b) => new Date(b.bidTime).getTime() - new Date(a.bidTime).getTime()
    );
}

function LoadingSkeleton() {
    return (
        <div className="space-y-3">
            {Array.from({ length: 4 }).map((_, i) => (
                <div
                    key={i}
                    className="flex items-center gap-3 p-4 rounded-xl border border-slate-200 dark:border-slate-800"
                >
                    <Skeleton className="h-12 w-12 rounded-full" />
                    <div className="flex-1 space-y-2">
                        <Skeleton className="h-4 w-28" />
                        <Skeleton className="h-3 w-36" />
                    </div>
                    <div className="text-right space-y-2">
                        <Skeleton className="h-5 w-20" />
                        <Skeleton className="h-4 w-16" />
                    </div>
                </div>
            ))}
        </div>
    );
}

function EmptyState() {
    return (
        <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex flex-col items-center justify-center py-12 text-center"
        >
            <motion.div
                initial={{ scale: 0.8 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.1, type: "spring" }}
                className="w-20 h-20 rounded-full bg-linear-to-br from-purple-100 to-blue-100 dark:from-purple-900/30 dark:to-blue-900/30 flex items-center justify-center mb-4"
            >
                <FontAwesomeIcon
                    icon={faDollarSign}
                    className="w-8 h-8 text-purple-500"
                />
            </motion.div>
            <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
                No bids yet
            </h3>
            <p className="text-sm text-slate-500 dark:text-slate-400 max-w-xs">
                Be the first to place a bid on this item and get ahead of the competition!
            </p>
        </motion.div>
    );
}

interface BidItemProps {
    bid: Bid;
    isHighest: boolean;
    isNew: boolean;
    rank: number;
}

function BidItem({ bid, isHighest, isNew, rank }: BidItemProps) {
    const config = getBidStatusStyle(bid.status as BidStatus);

    return (
        <motion.div
            initial={isNew ? { opacity: 0, x: -20, scale: 0.95 } : false}
            animate={{ opacity: 1, x: 0, scale: 1 }}
            transition={{ duration: 0.3, type: "spring" }}
            className={cn(
                "relative flex items-center gap-4 p-4 rounded-xl border transition-all",
                isHighest
                    ? "border-emerald-400 dark:border-emerald-600 bg-linear-to-r from-emerald-50 to-green-50 dark:from-emerald-950/30 dark:to-green-950/30 shadow-lg shadow-emerald-500/10"
                    : "border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900/50 hover:border-slate-300 dark:hover:border-slate-700",
                isNew && "ring-2 ring-purple-500 ring-offset-2 ring-offset-white dark:ring-offset-slate-950"
            )}
        >
            {isNew && (
                <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    className="absolute -top-2 -right-2 px-2 py-0.5 rounded-full bg-purple-500 text-white text-xs font-bold shadow-lg"
                >
                    NEW
                </motion.div>
            )}

            <div className="relative">
                <div
                    className={cn(
                        "h-12 w-12 rounded-full flex items-center justify-center text-lg font-bold",
                        isHighest
                            ? "bg-linear-to-br from-amber-400 to-orange-500 text-white shadow-lg shadow-amber-500/30"
                            : rank <= 3
                            ? "bg-linear-to-br from-purple-500 to-blue-600 text-white"
                            : "bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400"
                    )}
                >
                    {isHighest ? (
                        <FontAwesomeIcon icon={faCrown} className="w-5 h-5" />
                    ) : rank <= 3 ? (
                        <FontAwesomeIcon icon={faTrophy} className="w-5 h-5" />
                    ) : (
                        bid.bidderUsername.charAt(0).toUpperCase()
                    )}
                </div>
                {rank <= 3 && !isHighest && (
                    <span className="absolute -bottom-1 -right-1 w-5 h-5 rounded-full bg-slate-900 dark:bg-white text-white dark:text-slate-900 text-xs font-bold flex items-center justify-center">
                        {rank}
                    </span>
                )}
            </div>

            <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                    <span className="font-semibold text-slate-900 dark:text-white truncate">
                        {bid.bidderUsername}
                    </span>
                    {isHighest && (
                        <Badge className="bg-emerald-500 hover:bg-emerald-600 text-white text-xs">
                            <FontAwesomeIcon icon={faArrowUp} className="w-2.5 h-2.5 mr-1" />
                            Leading
                        </Badge>
                    )}
                </div>
                <div className="flex items-center gap-2 mt-1 text-xs text-slate-500 dark:text-slate-400">
                    <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                    <span>
                        {formatDistanceToNow(new Date(bid.bidTime), { addSuffix: true })}
                    </span>
                </div>
            </div>

            <div className="text-right">
                <motion.p
                    initial={isNew ? { scale: 1.2 } : false}
                    animate={{ scale: 1 }}
                    className={cn(
                        "font-bold text-lg",
                        isHighest
                            ? "text-emerald-600 dark:text-emerald-400"
                            : "text-slate-900 dark:text-white"
                    )}
                >
                    {formatCurrency(bid.amount)}
                </motion.p>
                <Badge
                    variant="secondary"
                    className={cn("text-xs mt-1", config.className)}
                >
                    {config.label}
                </Badge>
            </div>
        </motion.div>
    );
}

export function EnhancedBidHistory({
    auctionId,
    refreshTrigger,
    maxHeight = DEFAULT_MAX_HEIGHT,
    enableRealtime = true,
    isLive,
    pollingInterval = DEFAULT_POLLING_INTERVAL,
}: EnhancedBidHistoryProps) {
    const shouldPoll = enableRealtime || isLive;
    const [bids, setBids] = useState<Bid[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [newBidIds, setNewBidIds] = useState<Set<string>>(new Set());
    const [isRefreshing, setIsRefreshing] = useState(false);
    const previousBidsRef = useRef<string[]>([]);
    const isMountedRef = useRef(true);

    const fetchBids = useCallback(async (showLoading = false) => {
        if (showLoading) setIsLoading(true);
        setError(null);

        try {
            const data = await bidService.getBidsForAuction(auctionId);
            if (!isMountedRef.current) return;

            const sortedBids = sortBidsByTime(data);
            const currentBidIds = sortedBids.map((b) => b.id);
            const newIds = currentBidIds.filter(
                (id) => !previousBidsRef.current.includes(id)
            );

            if (newIds.length > 0 && previousBidsRef.current.length > 0) {
                setNewBidIds(new Set(newIds));
                setTimeout(() => {
                    if (isMountedRef.current) {
                        setNewBidIds(new Set());
                    }
                }, 3000);
            }

            previousBidsRef.current = currentBidIds;
            setBids(sortedBids);
        } catch {
            if (isMountedRef.current) {
                setError("Failed to load bid history");
            }
        } finally {
            if (isMountedRef.current) {
                setIsLoading(false);
                setIsRefreshing(false);
            }
        }
    }, [auctionId]);

    const handleRefresh = () => {
        setIsRefreshing(true);
        fetchBids(false);
    };

    useEffect(() => {
        isMountedRef.current = true;
        fetchBids(true);

        return () => {
            isMountedRef.current = false;
        };
    }, [fetchBids, refreshTrigger]);

    useEffect(() => {
        if (!shouldPoll) return;

        const interval = setInterval(() => {
            if (isMountedRef.current) {
                fetchBids(false);
            }
        }, pollingInterval);

        return () => clearInterval(interval);
    }, [shouldPoll, pollingInterval, fetchBids]);

    if (isLoading) {
        return <LoadingSkeleton />;
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center py-8 text-center">
                <p className="text-red-500 mb-4">{error}</p>
                <Button variant="outline" onClick={() => fetchBids(true)}>
                    <FontAwesomeIcon icon={faRefresh} className="w-4 h-4 mr-2" />
                    Try Again
                </Button>
            </div>
        );
    }

    if (bids.length === 0) {
        return <EmptyState />;
    }

    return (
        <div className="space-y-4">
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                    <span className="text-sm font-medium text-slate-600 dark:text-slate-400">
                        {bids.length} bid{bids.length !== 1 ? "s" : ""}
                    </span>
                    {enableRealtime && (
                        <span className="flex items-center gap-1 text-xs text-emerald-600 dark:text-emerald-400">
                            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                            Live updates
                        </span>
                    )}
                </div>
                <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleRefresh}
                    disabled={isRefreshing}
                    className="text-slate-500 hover:text-slate-900 dark:hover:text-white"
                >
                    <FontAwesomeIcon
                        icon={faRefresh}
                        className={cn("w-3.5 h-3.5 mr-1.5", isRefreshing && "animate-spin")}
                    />
                    Refresh
                </Button>
            </div>

            <ScrollArea style={{ maxHeight }} className="pr-4">
                <AnimatePresence mode="popLayout">
                    <motion.div className="space-y-3">
                        {bids.map((bid, index) => (
                            <BidItem
                                key={bid.id}
                                bid={bid}
                                isHighest={index === 0 && bid.status === BidStatus.Accepted}
                                isNew={newBidIds.has(bid.id)}
                                rank={index + 1}
                            />
                        ))}
                    </motion.div>
                </AnimatePresence>
            </ScrollArea>
        </div>
    );
}
