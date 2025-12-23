"use client";

import { useState, useCallback } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faGavel,
    faBolt,
    faChevronUp,
    faChevronDown,
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/utils";

interface StickyBidBarProps {
    currentBid: number;
    minBidIncrement: number;
    currency?: string;
    isLive: boolean;
    isAuthenticated: boolean;
    onPlaceBid: (amount: number) => Promise<void>;
    onBuyNow?: () => void;
    buyNowPrice?: number;
    className?: string;
}

export function StickyBidBar({
    currentBid,
    minBidIncrement,
    currency = "USD",
    isLive,
    isAuthenticated,
    onPlaceBid,
    onBuyNow,
    buyNowPrice,
    className,
}: StickyBidBarProps) {
    const [isExpanded, setIsExpanded] = useState(false);
    const [bidAmount, setBidAmount] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const minBid = currentBid + minBidIncrement;

    const handleBidChange = useCallback(
        (value: string) => {
            setError(null);
            const numValue = parseFloat(value.replace(/[^0-9.]/g, ""));
            if (!isNaN(numValue)) {
                setBidAmount(numValue.toString());
            } else if (value === "") {
                setBidAmount("");
            }
        },
        []
    );

    const handlePlaceBid = useCallback(async () => {
        const amount = parseFloat(bidAmount);

        if (isNaN(amount) || amount < minBid) {
            setError(`Minimum bid is ${formatCurrency(minBid, currency)}`);
            return;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            await onPlaceBid(amount);
            setBidAmount("");
            setIsExpanded(false);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Failed to place bid");
        } finally {
            setIsSubmitting(false);
        }
    }, [bidAmount, minBid, currency, onPlaceBid]);

    const quickBidAmounts = [
        minBid,
        minBid + minBidIncrement,
        minBid + minBidIncrement * 2,
    ];

    if (!isLive) return null;

    return (
        <motion.div
            initial={{ y: 100 }}
            animate={{ y: 0 }}
            className={cn(
                "fixed bottom-0 left-0 right-0 z-50 md:hidden",
                "bg-white dark:bg-slate-900 border-t border-slate-200 dark:border-slate-700",
                "shadow-[0_-4px_20px_rgba(0,0,0,0.1)]",
                className
            )}
        >
            <div className="safe-area-inset-bottom">
                <AnimatePresence mode="wait">
                    {isExpanded ? (
                        <motion.div
                            key="expanded"
                            initial={{ height: 0, opacity: 0 }}
                            animate={{ height: "auto", opacity: 1 }}
                            exit={{ height: 0, opacity: 0 }}
                            transition={{ duration: 0.2 }}
                            className="p-4 space-y-4"
                        >
                            <div className="flex items-center justify-between">
                                <div>
                                    <div className="text-sm text-slate-500 dark:text-slate-400">
                                        Current Bid
                                    </div>
                                    <div className="text-xl font-bold text-slate-900 dark:text-white">
                                        {formatCurrency(currentBid, currency)}
                                    </div>
                                </div>
                                <button
                                    onClick={() => setIsExpanded(false)}
                                    className="p-2 text-slate-400 hover:text-slate-600"
                                >
                                    <FontAwesomeIcon icon={faChevronDown} className="w-5 h-5" />
                                </button>
                            </div>

                            <div className="flex gap-2">
                                {quickBidAmounts.map((amount) => (
                                    <button
                                        key={amount}
                                        onClick={() => setBidAmount(amount.toString())}
                                        className={cn(
                                            "flex-1 py-2 px-3 rounded-lg text-sm font-medium transition-colors",
                                            parseFloat(bidAmount) === amount
                                                ? "bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 border-2 border-purple-500"
                                                : "bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300 border-2 border-transparent"
                                        )}
                                    >
                                        {formatCurrency(amount, currency)}
                                    </button>
                                ))}
                            </div>

                            <div className="flex gap-2">
                                <div className="relative flex-1">
                                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500">
                                        $
                                    </span>
                                    <Input
                                        type="text"
                                        inputMode="decimal"
                                        value={bidAmount}
                                        onChange={(e) => handleBidChange(e.target.value)}
                                        placeholder={minBid.toString()}
                                        className={cn(
                                            "pl-6",
                                            error && "border-red-500 focus:ring-red-500"
                                        )}
                                    />
                                </div>
                            </div>

                            {error && (
                                <motion.p
                                    initial={{ opacity: 0, y: -5 }}
                                    animate={{ opacity: 1, y: 0 }}
                                    className="text-sm text-red-500"
                                >
                                    {error}
                                </motion.p>
                            )}

                            <div className="flex gap-2">
                                <Button
                                    onClick={handlePlaceBid}
                                    disabled={isSubmitting || !isAuthenticated}
                                    className="flex-1 bg-linear-to-r from-purple-600 to-blue-600 text-white"
                                >
                                    {isSubmitting ? (
                                        <motion.div
                                            animate={{ rotate: 360 }}
                                            transition={{ duration: 1, repeat: Infinity, ease: "linear" }}
                                        >
                                            <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                        </motion.div>
                                    ) : (
                                        <>
                                            <FontAwesomeIcon icon={faGavel} className="w-4 h-4 mr-2" />
                                            Place Bid
                                        </>
                                    )}
                                </Button>

                                {buyNowPrice && onBuyNow && (
                                    <Button
                                        onClick={onBuyNow}
                                        variant="outline"
                                        className="border-amber-500 text-amber-600 hover:bg-amber-50"
                                    >
                                        <FontAwesomeIcon icon={faBolt} className="w-4 h-4 mr-2" />
                                        Buy Now
                                    </Button>
                                )}
                            </div>

                            <p className="text-xs text-center text-slate-500 dark:text-slate-400">
                                Minimum bid: {formatCurrency(minBid, currency)}
                            </p>
                        </motion.div>
                    ) : (
                        <motion.div
                            key="collapsed"
                            initial={{ opacity: 0 }}
                            animate={{ opacity: 1 }}
                            exit={{ opacity: 0 }}
                            className="p-4 flex items-center gap-3"
                        >
                            <div className="flex-1">
                                <div className="flex items-center gap-2">
                                    <span className="text-sm text-slate-500 dark:text-slate-400">
                                        Current:
                                    </span>
                                    <span className="font-bold text-lg text-slate-900 dark:text-white">
                                        {formatCurrency(currentBid, currency)}
                                    </span>
                                </div>
                            </div>

                            <Button
                                onClick={() => setIsExpanded(true)}
                                className="bg-linear-to-r from-purple-600 to-blue-600 text-white px-6"
                            >
                                <FontAwesomeIcon icon={faGavel} className="w-4 h-4 mr-2" />
                                Bid Now
                                <FontAwesomeIcon icon={faChevronUp} className="w-3 h-3 ml-2" />
                            </Button>
                        </motion.div>
                    )}
                </AnimatePresence>
            </div>
        </motion.div>
    );
}

interface PullToRefreshProps {
    onRefresh: () => Promise<void>;
    children: React.ReactNode;
    threshold?: number;
    className?: string;
}

export function PullToRefresh({
    onRefresh,
    children,
    threshold = 80,
    className,
}: PullToRefreshProps) {
    const [pullDistance, setPullDistance] = useState(0);
    const [isRefreshing, setIsRefreshing] = useState(false);
    const [startY, setStartY] = useState(0);

    const handleTouchStart = useCallback((e: React.TouchEvent) => {
        if (window.scrollY === 0) {
            setStartY(e.touches[0].clientY);
        }
    }, []);

    const handleTouchMove = useCallback(
        (e: React.TouchEvent) => {
            if (startY === 0 || isRefreshing) return;

            const currentY = e.touches[0].clientY;
            const distance = Math.max(0, currentY - startY);

            if (window.scrollY === 0 && distance > 0) {
                setPullDistance(Math.min(distance * 0.5, threshold * 1.5));
            }
        },
        [startY, isRefreshing, threshold]
    );

    const handleTouchEnd = useCallback(async () => {
        if (pullDistance >= threshold && !isRefreshing) {
            setIsRefreshing(true);
            setPullDistance(threshold * 0.5);

            try {
                await onRefresh();
            } finally {
                setIsRefreshing(false);
                setPullDistance(0);
            }
        } else {
            setPullDistance(0);
        }
        setStartY(0);
    }, [pullDistance, threshold, isRefreshing, onRefresh]);

    const progress = Math.min(pullDistance / threshold, 1);

    return (
        <div
            className={cn("relative", className)}
            onTouchStart={handleTouchStart}
            onTouchMove={handleTouchMove}
            onTouchEnd={handleTouchEnd}
        >
            <AnimatePresence>
                {(pullDistance > 0 || isRefreshing) && (
                    <motion.div
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        style={{ height: pullDistance }}
                        className="absolute top-0 left-0 right-0 flex items-center justify-center overflow-hidden bg-slate-50 dark:bg-slate-900"
                    >
                        <motion.div
                            animate={{
                                rotate: isRefreshing ? 360 : progress * 180,
                                scale: isRefreshing ? 1 : 0.8 + progress * 0.2,
                            }}
                            transition={
                                isRefreshing
                                    ? { duration: 1, repeat: Infinity, ease: "linear" }
                                    : { duration: 0 }
                            }
                            className={cn(
                                "w-8 h-8 rounded-full border-2 border-t-transparent",
                                progress >= 1 || isRefreshing
                                    ? "border-purple-600"
                                    : "border-slate-300 dark:border-slate-600"
                            )}
                        />
                    </motion.div>
                )}
            </AnimatePresence>

            <motion.div
                animate={{ y: pullDistance }}
                transition={{ type: "spring", stiffness: 400, damping: 30 }}
            >
                {children}
            </motion.div>
        </div>
    );
}
