"use client";

import { useMemo, useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faGavel,
    faTruck,
    faArrowRight,
    faEye,
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import {
    CountdownBadge,
    StatusBadge,
    WatchlistIconButton,
    PriceDisplay,
    getStatusConfig,
} from "@/components/common";
import { Auction, ITEM_CONDITION_LABELS, ItemCondition, ShippingType } from "@/types/auction";
import { SearchItem } from "@/types/search";
import { cn } from "@/lib/utils";
import { useCountdown, getUrgencyLevel } from "@/hooks/use-countdown";
import { UI } from "@/constants/config";
import {
    getAuctionTitle,
    getAuctionAttributes,
    getAuctionCondition,
    getAuctionYearManufactured,
} from "@/utils/auction";
import { useWatchlist } from "@/context/watchlist.context";
import type { AuctionCardProps, AuctionCardColorScheme } from "./types";

const COLOR_SCHEMES: Record<AuctionCardColorScheme, { hover: string; gradient: string; shadow: string }> = {
    purple: {
        hover: "hover:border-purple-400/50 dark:hover:border-purple-500/50",
        gradient: "from-purple-600 to-pink-600",
        shadow: "hover:shadow-purple-500/10 dark:hover:shadow-purple-500/20",
    },
    emerald: {
        hover: "hover:border-emerald-400/50 dark:hover:border-emerald-500/50",
        gradient: "from-emerald-600 to-teal-600",
        shadow: "hover:shadow-emerald-500/10 dark:hover:shadow-emerald-500/20",
    },
};

function getImageUrl(auction: Auction | SearchItem): string | undefined {
    if ("imageUrl" in auction && auction.imageUrl) {
        return auction.imageUrl;
    }
    if ("files" in auction && auction.files) {
        const primaryFile = auction.files.find((f) => f.isPrimary);
        return primaryFile?.url || auction.files[0]?.url;
    }
    return undefined;
}

function formatTimeDisplay(timeLeft: ReturnType<typeof useCountdown>): string {
    if (!timeLeft || timeLeft.isExpired) return "Ended";
    if (timeLeft.days > 0) return `${timeLeft.days}d ${timeLeft.hours}h`;
    if (timeLeft.hours > 0) return `${timeLeft.hours}h ${timeLeft.minutes}m`;
    if (timeLeft.minutes > 0) return `${timeLeft.minutes}m ${timeLeft.seconds}s`;
    return `${timeLeft.seconds}s`;
}

function CompactCard({
    auction,
    imageUrl,
    title,
    currentBid,
    timeLeft,
    isUrgent,
}: {
    auction: Auction | SearchItem;
    imageUrl: string | undefined;
    title: string;
    currentBid: number;
    timeLeft: ReturnType<typeof useCountdown>;
    isUrgent: boolean;
}) {
    return (
        <Link href={`/auctions/${auction.id}`}>
            <motion.div
                whileHover={{ scale: 1.02 }}
                className="flex gap-3 p-3 bg-white dark:bg-slate-900 rounded-xl border border-slate-200/80 dark:border-slate-800/80 hover:shadow-md transition-all"
            >
                <div className="relative w-20 h-20 rounded-lg overflow-hidden flex-shrink-0">
                    {imageUrl ? (
                        <Image src={imageUrl} alt={title} fill className="object-cover" />
                    ) : (
                        <div className="w-full h-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center">
                            <FontAwesomeIcon icon={faGavel} className="w-6 h-6 text-slate-400" />
                        </div>
                    )}
                </div>
                <div className="flex-1 min-w-0">
                    <h3 className="font-medium text-sm line-clamp-1 text-slate-900 dark:text-white">
                        {title}
                    </h3>
                    <PriceDisplay
                        amount={currentBid}
                        variant="default"
                        className="[&_p]:text-lg [&_p]:text-purple-600 [&_p]:dark:text-purple-400"
                    />
                    {timeLeft && (
                        <CountdownBadge
                            timeText={formatTimeDisplay(timeLeft)}
                            isUrgent={isUrgent}
                            variant="minimal"
                        />
                    )}
                </div>
            </motion.div>
        </Link>
    );
}

function CarouselCard({
    auction,
    imageUrl,
    title,
    currentBid,
    timeLeft,
    isUrgent,
    isNew,
    index,
    colorScheme,
    showWatchlistButton,
    isWatched,
    onWatchlistClick,
}: {
    auction: Auction;
    imageUrl: string;
    title: string;
    currentBid: number;
    timeLeft: ReturnType<typeof useCountdown>;
    isUrgent: boolean;
    isNew: boolean;
    index: number;
    colorScheme: AuctionCardColorScheme;
    showWatchlistButton: boolean;
    isWatched: boolean;
    onWatchlistClick: (e: React.MouseEvent) => void;
}) {
    const colors = COLOR_SCHEMES[colorScheme];

    return (
        <motion.div
            initial={{ opacity: 0, y: 30, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            transition={{ duration: 0.5, delay: index * 0.1 }}
            className="shrink-0 w-80 snap-start"
        >
            <Link href={`/auctions/${auction.id}`} className="group block">
                <div
                    className={cn(
                        "relative bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800",
                        colors.hover,
                        colors.shadow,
                        "transition-all duration-500 hover:shadow-2xl hover:-translate-y-2"
                    )}
                >
                    <div className="absolute top-4 left-4 right-4 z-20 flex items-center justify-between">
                        <div className="flex gap-2">
                            {isNew && <StatusBadge type="new" />}
                            {auction.isFeatured && <StatusBadge type="featured" />}
                        </div>
                        {showWatchlistButton && (
                            <WatchlistIconButton
                                isWatched={isWatched}
                                onClick={onWatchlistClick}
                                variant="circle"
                            />
                        )}
                    </div>

                    <div className="relative h-52 overflow-hidden bg-slate-100 dark:bg-slate-800">
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-700 group-hover:scale-110"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-linear-to-t from-black/80 via-black/20 to-transparent" />

                        <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between">
                            <CountdownBadge
                                timeText={formatTimeDisplay(timeLeft)}
                                isUrgent={isUrgent}
                                variant="inline"
                            />
                            <div className="flex items-center gap-3 text-white/80 text-xs">
                                <div className="flex items-center gap-1">
                                    <FontAwesomeIcon icon={faGavel} className="w-3 h-3" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="p-5 space-y-4">
                        <div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                                {auction.categoryName || "Uncategorized"}
                            </p>
                            <h3
                                className={cn(
                                    "text-lg font-bold text-slate-900 dark:text-white truncate transition-colors",
                                    colorScheme === "emerald"
                                        ? "group-hover:text-emerald-600 dark:group-hover:text-emerald-400"
                                        : "group-hover:text-purple-600 dark:group-hover:text-purple-400"
                                )}
                            >
                                {title}
                            </h3>
                        </div>

                        <div className="flex items-end justify-between pt-3 border-t border-slate-100 dark:border-slate-800">
                            <PriceDisplay
                                amount={currentBid}
                                label="Current Bid"
                                variant="gradient"
                                gradientColors={cn("bg-linear-to-r", colors.gradient)}
                            />
                            <Button
                                size="sm"
                                className={cn(
                                    "h-10 px-5 bg-slate-900 dark:bg-white text-white dark:text-slate-900 font-semibold rounded-xl transition-colors",
                                    colorScheme === "emerald"
                                        ? "hover:bg-emerald-600 dark:hover:bg-emerald-500"
                                        : "hover:bg-purple-600 dark:hover:bg-purple-500",
                                    "dark:hover:text-white"
                                )}
                            >
                                View
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-3 h-3" />
                            </Button>
                        </div>
                    </div>
                </div>
            </Link>
        </motion.div>
    );
}

function FeaturedCard({
    auction,
    imageUrl,
    title,
    currentBid,
    yearManufactured,
    timeLeft,
    isUrgent,
    index,
    showWatchlistButton,
    isWatched,
    onWatchlistClick,
}: {
    auction: Auction;
    imageUrl: string;
    title: string;
    currentBid: number;
    yearManufactured: number | null;
    timeLeft: ReturnType<typeof useCountdown>;
    isUrgent: boolean;
    index: number;
    showWatchlistButton: boolean;
    isWatched: boolean;
    onWatchlistClick: (e: React.MouseEvent) => void;
}) {
    return (
        <motion.div
            initial={{ opacity: 0, y: 30, scale: 0.95 }}
            whileInView={{ opacity: 1, y: 0, scale: 1 }}
            viewport={{ once: true }}
            transition={{ duration: UI.ANIMATION.SLIDE_DURATION, delay: index * UI.ANIMATION.STAGGER_DELAY }}
        >
            <Link href={`/auctions/${auction.id}`} className="group block">
                <div className="relative bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800 hover:border-purple-400/50 dark:hover:border-purple-500/50 transition-all duration-500 hover:shadow-2xl hover:shadow-purple-500/10 dark:hover:shadow-purple-500/20 hover:-translate-y-2">
                    <div className="absolute top-4 left-4 right-4 z-20 flex items-center justify-between">
                        <div className="flex gap-2">
                            {auction.isFeatured && <StatusBadge type="featured" />}
                            {isUrgent && <StatusBadge type="ending-soon" />}
                        </div>
                        {showWatchlistButton && (
                            <WatchlistIconButton
                                isWatched={isWatched}
                                onClick={onWatchlistClick}
                                variant="circle"
                            />
                        )}
                    </div>

                    <div className="relative h-56 overflow-hidden bg-slate-100 dark:bg-slate-800">
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-700 group-hover:scale-110"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-linear-to-t from-black/80 via-black/20 to-transparent" />

                        <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between">
                            <CountdownBadge
                                timeText={formatTimeDisplay(timeLeft)}
                                isUrgent={isUrgent}
                                variant="inline"
                            />
                        </div>
                    </div>

                    <div className="p-5 space-y-4">
                        <div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                                {auction.categoryName || "Uncategorized"}
                            </p>
                            <h3 className="text-lg font-bold text-slate-900 dark:text-white truncate group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                {title}
                            </h3>
                            {yearManufactured && (
                                <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">{yearManufactured}</p>
                            )}
                        </div>

                        <div className="flex items-end justify-between pt-3 border-t border-slate-100 dark:border-slate-800">
                            <PriceDisplay
                                amount={currentBid}
                                label="Current Bid"
                                variant="gradient"
                                gradientColors="bg-linear-to-r from-purple-600 to-pink-600"
                            />
                            <Button
                                size="sm"
                                className="h-10 px-5 bg-slate-900 dark:bg-white hover:bg-purple-600 dark:hover:bg-purple-500 text-white dark:text-slate-900 dark:hover:text-white font-semibold rounded-xl transition-colors"
                            >
                                Place Bid
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-3 h-3" />
                            </Button>
                        </div>
                    </div>
                </div>
            </Link>
        </motion.div>
    );
}

function DefaultCard({
    auction,
    imageUrl,
    title,
    currentBid,
    attributes,
    yearManufactured,
    condition,
    shippingType,
    hasReserve,
    hasBuyNow,
    status,
    timeLeft,
    isUrgent,
    showWatchlistButton,
    isWatched,
    onWatchlistClick,
}: {
    auction: Auction | SearchItem;
    imageUrl: string | undefined;
    title: string;
    currentBid: number;
    attributes: Record<string, string>;
    yearManufactured: number | null;
    condition: string | null;
    shippingType: string | null;
    hasReserve: boolean;
    hasBuyNow: boolean;
    status: string;
    timeLeft: ReturnType<typeof useCountdown>;
    isUrgent: boolean;
    showWatchlistButton: boolean;
    isWatched: boolean;
    onWatchlistClick: (e: React.MouseEvent) => void;
}) {
    const timeText = timeLeft ? formatTimeDisplay(timeLeft) : null;
    const statusConfig = getStatusConfig(status);

    return (
        <Link href={`/auctions/${auction.id}`} className="block">
            <motion.div
                whileHover={{ y: -4 }}
                transition={{ duration: 0.2 }}
                className="group relative bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800/80 overflow-hidden shadow-sm hover:shadow-xl transition-all duration-300"
            >
                <div className="relative aspect-4/3 overflow-hidden bg-slate-100 dark:bg-slate-800">
                    {imageUrl ? (
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-500 group-hover:scale-110"
                        />
                    ) : (
                        <div className="flex h-full items-center justify-center">
                            <FontAwesomeIcon icon={faGavel} className="w-12 h-12 text-slate-300 dark:text-slate-600" />
                        </div>
                    )}

                    <div className="absolute inset-0 bg-linear-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

                    <div className="absolute top-3 left-3 flex flex-wrap gap-2">
                        <StatusBadge type="status" status={status} />
                        {"isFeatured" in auction && auction.isFeatured && (
                            <StatusBadge type="hot" />
                        )}
                    </div>

                    {showWatchlistButton && (
                        <WatchlistIconButton
                            isWatched={isWatched}
                            onClick={onWatchlistClick}
                            variant="square"
                            className="absolute top-3 right-3"
                        />
                    )}

                    {timeText && (
                        <CountdownBadge
                            timeText={timeText}
                            isUrgent={isUrgent}
                            variant="overlay"
                            className="absolute bottom-3 left-3"
                        />
                    )}

                    <div className="absolute bottom-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-medium bg-white/90 dark:bg-slate-800/90 text-slate-700 dark:text-slate-200 backdrop-blur-md shadow-lg">
                            <FontAwesomeIcon icon={faEye} className="w-3 h-3" />
                            View
                        </span>
                    </div>
                </div>

                <div className="p-4 space-y-3">
                    <div>
                        <h3 className="font-semibold text-slate-900 dark:text-white line-clamp-1 group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                            {title}
                        </h3>
                        {Object.keys(attributes).length > 0 && (
                            <p className="text-sm text-slate-500 dark:text-slate-400 line-clamp-1">
                                {yearManufactured && `${yearManufactured} `}
                                {Object.values(attributes).join(" ")}
                            </p>
                        )}
                    </div>

                    <div className="flex items-end justify-between gap-2">
                        <PriceDisplay
                            amount={currentBid}
                            label={statusConfig.text === "Ended" ? "Final Price" : "Current Bid"}
                            variant="default"
                        />

                        {hasBuyNow && statusConfig.text !== "Ended" && (
                            <div className="text-right">
                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">Buy Now</p>
                                <p className="text-sm font-semibold text-green-600 dark:text-green-400">
                                    ${"buyNowPrice" in auction ? (auction.buyNowPrice || 0).toLocaleString() : "0"}
                                </p>
                            </div>
                        )}
                    </div>

                    <div className="flex items-center gap-2 flex-wrap">
                        {condition && (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400">
                                {ITEM_CONDITION_LABELS[condition as ItemCondition] || condition}
                            </span>
                        )}
                        {shippingType === ShippingType.Free && (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-medium bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400">
                                <FontAwesomeIcon icon={faTruck} className="w-3 h-3" />
                                Free Shipping
                            </span>
                        )}
                        {hasReserve && (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400">
                                Reserve
                            </span>
                        )}
                    </div>
                </div>

                <div className="absolute inset-x-0 bottom-0 h-1 bg-linear-to-r from-purple-500 to-blue-500 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left" />
            </motion.div>
        </Link>
    );
}

export function AuctionCard({
    auction,
    variant = "default",
    colorScheme = "purple",
    index = 0,
    showWatchlistButton = true,
    isWatched: externalisWatched,
    onWatchlistToggle,
}: AuctionCardProps) {
    const { isInWatchlist, toggleWatchlist } = useWatchlist();
    const isWatched = externalisWatched ?? isInWatchlist(auction.id);

    const endDate = "auctionEnd" in auction ? auction.auctionEnd : null;
    const timeLeft = useCountdown(endDate);
    const urgency = getUrgencyLevel(timeLeft);
    const isUrgent = urgency === "critical" || urgency === "warning";

    const imageUrl = useMemo(() => {
        const url = getImageUrl(auction);
        return url || "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400";
    }, [auction]);

    const title = getAuctionTitle(auction);
    const attributes = getAuctionAttributes(auction);
    const yearManufactured = getAuctionYearManufactured(auction) ?? null;
    const condition = getAuctionCondition(auction) ?? null;

    const currentBid =
        "currentHighBid" in auction ? auction.currentHighBid : "price" in auction ? (auction as SearchItem).price : 0;
    const hasReserve = Boolean("reservePrice" in auction && auction.reservePrice && auction.reservePrice > 0);
    const hasBuyNow = Boolean("buyNowPrice" in auction && auction.buyNowPrice && auction.buyNowPrice > 0);
    const shippingType = ("shippingType" in auction ? auction.shippingType : null) ?? null;
    const isNew = useMemo(() => {
        if (!("createdAt" in auction)) return false;
        const createdTime = new Date(auction.createdAt).getTime();
        const now = typeof window !== "undefined" ? window.performance.timeOrigin + window.performance.now() : 0;
        return createdTime > now - 24 * 60 * 60 * 1000;
    }, [auction]);

    const handleWatchlistClick = useCallback(
        (e: React.MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
            if (onWatchlistToggle) {
                onWatchlistToggle(auction.id);
            } else {
                toggleWatchlist(auction.id);
            }
        },
        [auction.id, onWatchlistToggle, toggleWatchlist]
    );

    if (variant === "compact") {
        return (
            <CompactCard
                auction={auction}
                imageUrl={imageUrl}
                title={title}
                currentBid={currentBid || 0}
                timeLeft={timeLeft}
                isUrgent={isUrgent}
            />
        );
    }

    if (variant === "carousel") {
        return (
            <CarouselCard
                auction={auction as Auction}
                imageUrl={imageUrl}
                title={title}
                currentBid={currentBid || 0}
                timeLeft={timeLeft}
                isUrgent={isUrgent}
                isNew={isNew}
                index={index}
                colorScheme={colorScheme}
                showWatchlistButton={showWatchlistButton}
                isWatched={isWatched}
                onWatchlistClick={handleWatchlistClick}
            />
        );
    }

    if (variant === "featured") {
        return (
            <FeaturedCard
                auction={auction as Auction}
                imageUrl={imageUrl}
                title={title}
                currentBid={currentBid || 0}
                yearManufactured={yearManufactured}
                timeLeft={timeLeft}
                isUrgent={isUrgent}
                index={index}
                showWatchlistButton={showWatchlistButton}
                isWatched={isWatched}
                onWatchlistClick={handleWatchlistClick}
            />
        );
    }

    return (
        <DefaultCard
            auction={auction}
            imageUrl={imageUrl}
            title={title}
            currentBid={currentBid || 0}
            attributes={attributes}
            yearManufactured={yearManufactured}
            condition={condition}
            shippingType={shippingType}
            hasReserve={hasReserve}
            hasBuyNow={hasBuyNow}
            status={auction.status}
            timeLeft={timeLeft}
            isUrgent={isUrgent}
            showWatchlistButton={showWatchlistButton}
            isWatched={isWatched}
            onWatchlistClick={handleWatchlistClick}
        />
    );
}
