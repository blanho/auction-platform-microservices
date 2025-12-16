"use client";

import { useMemo, useState, useCallback } from "react";
import Image from "next/image";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faClock,
    faFire,
    faArrowRight,
    faHeart,
    faSpinner,
    faEye,
    faGavel,
    faStar,
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import Link from "next/link";
import { motion } from "framer-motion";
import { useFeaturedAuctionsQuery } from "@/hooks/queries";
import { useCountdown, getUrgencyLevel } from "@/hooks/use-countdown";
import { Auction } from "@/types/auction";
import { PulsingDot } from "@/components/ui/animated";
import { UI, FEATURED } from "@/constants/config";
import { getAuctionTitle, getAuctionYearManufactured } from "@/utils/auction";

function AuctionCard({
    auction,
    index,
}: {
    auction: Auction;
    index: number;
}) {
    const [isLiked, setIsLiked] = useState(false);
    const timeLeft = useCountdown(auction.auctionEnd);
    const urgency = getUrgencyLevel(timeLeft);
    const isEndingSoon = urgency === "critical" || urgency === "warning";
    const title = getAuctionTitle(auction);
    const yearManufactured = getAuctionYearManufactured(auction);

    const imageUrl = useMemo(() => {
        const primaryFile = auction.files?.find((f) => f.isPrimary);
        return (
            primaryFile?.url ||
            auction.files?.[0]?.url ||
            "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400"
        );
    }, [auction.files]);

    const handleLikeClick = useCallback((e: React.MouseEvent) => {
        e.preventDefault();
        setIsLiked(prev => !prev);
    }, []);

    const formatTimeDisplay = () => {
        if (!timeLeft || timeLeft.isExpired) return "Ended";
        if (timeLeft.days > 0) return `${timeLeft.days}d ${timeLeft.hours}h`;
        if (timeLeft.hours > 0) return `${timeLeft.hours}h ${timeLeft.minutes}m`;
        return `${timeLeft.minutes}m ${timeLeft.seconds}s`;
    };

    const currentBid = auction.currentHighBid || auction.reservePrice;

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
                            {auction.isFeatured && (
                                <Badge className="bg-gradient-to-r from-purple-500 to-pink-500 text-white px-3 py-1 text-xs border-0 shadow-lg">
                                    <FontAwesomeIcon icon={faStar} className="w-3 h-3 mr-1" />
                                    Featured
                                </Badge>
                            )}
                            {isEndingSoon && (
                                <Badge className="bg-gradient-to-r from-red-500 to-orange-500 text-white px-3 py-1 text-xs border-0 shadow-lg">
                                    <PulsingDot className="mr-1.5" />
                                    Ending Soon
                                </Badge>
                            )}
                        </div>
                        <button
                            onClick={handleLikeClick}
                            className="w-10 h-10 rounded-full bg-white/90 dark:bg-slate-800/90 shadow-lg flex items-center justify-center hover:scale-110 transition-transform"
                        >
                            <FontAwesomeIcon
                                icon={faHeart}
                                className={`w-4 h-4 transition-colors ${
                                    isLiked
                                        ? "text-red-500"
                                        : "text-slate-400 dark:text-slate-500"
                                }`}
                            />
                        </button>
                    </div>

                    <div className="relative h-56 overflow-hidden bg-slate-100 dark:bg-slate-800">
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-700 group-hover:scale-110"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

                        <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between">
                            <div className="bg-black/60 backdrop-blur-md rounded-xl px-3 py-2 flex items-center gap-2">
                                <FontAwesomeIcon
                                    icon={faClock}
                                    className={`w-3.5 h-3.5 ${
                                        isEndingSoon ? "text-red-400 animate-pulse" : "text-white"
                                    }`}
                                />
                                <span
                                    className={`font-mono text-sm font-bold ${
                                        isEndingSoon ? "text-red-400" : "text-white"
                                    }`}
                                >
                                    {formatTimeDisplay()}
                                </span>
                            </div>
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
                            <h3 className="text-lg font-bold text-slate-900 dark:text-white truncate group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                {title}
                            </h3>
                            {yearManufactured && (
                                <p className="text-sm text-slate-500 dark:text-slate-400 mt-0.5">
                                    {yearManufactured}
                                </p>
                            )}
                        </div>

                        <div className="flex items-end justify-between pt-3 border-t border-slate-100 dark:border-slate-800">
                            <div>
                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                                    Current Bid
                                </p>
                                <p className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent">
                                    ${currentBid.toLocaleString()}
                                </p>
                            </div>
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

function CardSkeleton() {
    return (
        <div className="bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800">
            <Skeleton className="h-56 w-full bg-slate-200 dark:bg-slate-800" />
            <div className="p-5 space-y-4">
                <Skeleton className="h-4 w-20 bg-slate-200 dark:bg-slate-800" />
                <Skeleton className="h-6 w-full bg-slate-200 dark:bg-slate-800" />
                <div className="flex justify-between pt-4">
                    <Skeleton className="h-10 w-24 bg-slate-200 dark:bg-slate-800" />
                    <Skeleton className="h-10 w-28 bg-slate-200 dark:bg-slate-800 rounded-xl" />
                </div>
            </div>
        </div>
    );
}

export function FeaturedAuctionsSection() {
    const { data: featuredAuctions, isLoading, error } = useFeaturedAuctionsQuery(FEATURED.DEFAULT_LIMIT);

    if (isLoading) {
        return (
            <section className="relative py-24 bg-slate-50 dark:bg-slate-950 overflow-hidden">
                <div className="container mx-auto px-4">
                    <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                        {[...Array(FEATURED.DEFAULT_LIMIT)].map((_, i) => (
                            <CardSkeleton key={i} />
                        ))}
                    </div>
                </div>
            </section>
        );
    }

    if (
        error ||
        !featuredAuctions ||
        !Array.isArray(featuredAuctions) ||
        featuredAuctions.length === 0
    ) {
        return null;
    }

    return (
        <section className="relative py-24 bg-slate-50 dark:bg-slate-950 overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom,_var(--tw-gradient-stops))] from-purple-200/30 via-transparent to-transparent dark:from-purple-900/10" />
            <div className="absolute top-40 left-1/4 w-96 h-96 bg-purple-400/10 dark:bg-purple-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-40 right-1/4 w-96 h-96 bg-pink-400/10 dark:bg-pink-600/10 rounded-full blur-3xl" />

            <div className="container mx-auto px-4 relative z-10">
                <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-8 mb-12">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="max-w-xl"
                    >
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-purple-100 dark:bg-purple-500/10 border border-purple-200 dark:border-purple-500/20 mb-6">
                            <FontAwesomeIcon icon={faStar} className="w-4 h-4 text-purple-600 dark:text-purple-400" />
                            <span className="text-sm font-medium text-purple-700 dark:text-purple-300">
                                Curated Selection
                            </span>
                        </div>

                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-slate-900 dark:text-white mb-4 leading-tight">
                            Featured
                            <span className="block text-transparent bg-clip-text bg-gradient-to-r from-purple-600 via-pink-600 to-purple-600 dark:from-purple-400 dark:via-pink-400 dark:to-purple-400">
                                Auctions
                            </span>
                        </h2>

                        <p className="text-lg text-slate-600 dark:text-slate-400 leading-relaxed">
                            Premium items handpicked by our experts — exceptional quality,
                            verified sellers.
                        </p>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: UI.ANIMATION.STAGGER_DELAY * 2 }}
                    >
                        <Button
                            className="h-12 px-6 rounded-full bg-gradient-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white font-medium shadow-lg shadow-purple-500/25"
                            asChild
                        >
                            <Link href="/auctions">
                                View All Auctions
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                            </Link>
                        </Button>
                    </motion.div>
                </div>

                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {featuredAuctions.map((auction, index) => (
                        <AuctionCard key={auction.id} auction={auction} index={index} />
                    ))}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: UI.ANIMATION.STAGGER_DELAY * 4 }}
                    className="mt-12 flex flex-wrap justify-center gap-6 text-center"
                >
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-slate-900 dark:text-white">
                            {featuredAuctions.length}+
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Featured Items
                        </p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-purple-600 dark:text-purple-400">
                            100%
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Verified Sellers
                        </p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
                            24h
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Buyer Protection
                        </p>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
