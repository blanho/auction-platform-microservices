"use client";

import { useEffect, useState, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Skeleton } from "@/components/ui/skeleton";
import { bidService } from "@/services/bid.service";
import { Bid, BidStatus } from "@/types/bid";
import { formatDistanceToNow } from "date-fns";
import { cn } from "@/lib/utils";
import { User, DollarSign, Clock } from "lucide-react";


interface BidHistoryProps {
    auctionId: string;
    refreshTrigger?: number;
    maxHeight?: string;
}

interface StatusConfig {
    label: string;
    className: string;
}

const DEFAULT_MAX_HEIGHT = "300px";
const SKELETON_COUNT = 3;

const STATUS_CONFIG: Record<BidStatus, StatusConfig> = {
    [BidStatus.Accepted]: {
        label: "Accepted",
        className: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200"
    },
    [BidStatus.AcceptedBelowReserve]: {
        label: "Below Reserve",
        className: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200"
    },
    [BidStatus.TooLow]: {
        label: "Too Low",
        className: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
    },
    [BidStatus.Pending]: {
        label: "Pending",
        className: "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200"
    },
    [BidStatus.Rejected]: {
        label: "Rejected",
        className: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200"
    }
};


function getStatusConfig(status: BidStatus): StatusConfig {
    return STATUS_CONFIG[status] || STATUS_CONFIG[BidStatus.Pending];
}

function sortBidsByTime(bids: Bid[]): Bid[] {
    return [...bids].sort((a, b) =>
        new Date(b.bidTime).getTime() - new Date(a.bidTime).getTime()
    );
}

function isHighestBid(bid: Bid, index: number): boolean {
    return index === 0 && bid.status === BidStatus.Accepted;
}

function LoadingSkeleton() {
    return (
        <div className="space-y-3">
            {Array.from({ length: SKELETON_COUNT }).map((_, i) => (
                <div key={i} className="flex items-center gap-3 p-3 rounded-lg border">
                    <Skeleton className="h-10 w-10 rounded-full" />
                    <div className="flex-1 space-y-2">
                        <Skeleton className="h-4 w-24" />
                        <Skeleton className="h-3 w-32" />
                    </div>
                    <Skeleton className="h-6 w-16" />
                </div>
            ))}
        </div>
    );
}

interface ErrorStateProps {
    message: string;
}

function ErrorState({ message }: ErrorStateProps) {
    return (
        <div className="text-center py-8 text-red-500">
            <p>{message}</p>
        </div>
    );
}

function EmptyState() {
    return (
        <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="text-center py-8 text-gray-500 dark:text-gray-400"
        >
            <motion.div
                initial={{ scale: 0.8 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.1 }}
            >
                <DollarSign className="h-10 w-10 mx-auto mb-2 opacity-50" />
            </motion.div>
            <p className="text-sm">No bids yet. Be the first to bid!</p>
        </motion.div>
    );
}

interface BidItemProps {
    bid: Bid;
    isHighest: boolean;
}

function BidItem({ bid, isHighest }: BidItemProps) {
    const config = getStatusConfig(bid.status as BidStatus);

    return (
        <motion.div
            initial={{ opacity: 0, x: -10 }}
            animate={{ opacity: 1, x: 0 }}
            whileHover={{ scale: 1.01 }}
            transition={{ duration: 0.2 }}
            className={cn(
                "flex items-center gap-3 p-3 rounded-lg border transition-colors",
                isHighest && "border-green-500 bg-green-50/50 dark:bg-green-950/20"
            )}
        >
            <div className={cn(
                "h-10 w-10 rounded-full flex items-center justify-center",
                isHighest ? "bg-green-100 dark:bg-green-900" : "bg-gray-100 dark:bg-gray-800"
            )}>
                <User className={cn("h-5 w-5", isHighest ? "text-green-600" : "text-gray-500")} />
            </div>
            <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                    <span className="font-medium text-sm truncate">{bid.bidder}</span>
                    {isHighest && (
                        <Badge variant="default" className="text-xs">
                            Highest
                        </Badge>
                    )}
                </div>
                <div className="flex items-center gap-2 text-xs text-gray-500 dark:text-gray-400">
                    <Clock className="h-3 w-3" />
                    <span>
                        {formatDistanceToNow(new Date(bid.bidTime), { addSuffix: true })}
                    </span>
                </div>
            </div>
            <div className="text-right">
                <p className="font-semibold text-sm">
                    ${bid.amount.toLocaleString()}
                </p>
                <Badge variant="secondary" className={cn("text-xs", config.className)}>
                    {config.label}
                </Badge>
            </div>
        </motion.div>
    );
}

export function BidHistory({
    auctionId,
    refreshTrigger,
    maxHeight = DEFAULT_MAX_HEIGHT
}: BidHistoryProps) {
    const [bids, setBids] = useState<Bid[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchBids = useCallback(async () => {
        setIsLoading(true);
        setError(null);
        try {
            const data = await bidService.getBidsForAuction(auctionId);
            setBids(sortBidsByTime(data));
        } catch (err) {
            console.error("Failed to fetch bids:", err);
            setError("Failed to load bid history");
        } finally {
            setIsLoading(false);
        }
    }, [auctionId]);

    useEffect(() => {
        fetchBids();
    }, [fetchBids, refreshTrigger]);

    if (isLoading) {
        return <LoadingSkeleton />;
    }

    if (error) {
        return <ErrorState message={error} />;
    }

    if (bids.length === 0) {
        return <EmptyState />;
    }

    return (
        <ScrollArea style={{ maxHeight }} className="pr-4">
            <motion.div
                initial="hidden"
                animate="visible"
                variants={{
                    hidden: { opacity: 0 },
                    visible: {
                        opacity: 1,
                        transition: { staggerChildren: 0.05 }
                    }
                }}
                className="space-y-2"
            >
                <AnimatePresence>
                    {bids.map((bid, index) => (
                        <motion.div
                            key={bid.id}
                            variants={{
                                hidden: { opacity: 0, y: 10 },
                                visible: { opacity: 1, y: 0 }
                            }}
                        >
                            <BidItem
                                bid={bid}
                                isHighest={isHighestBid(bid, index)}
                            />
                        </motion.div>
                    ))}
                </AnimatePresence>
            </motion.div>
        </ScrollArea>
    );
}
