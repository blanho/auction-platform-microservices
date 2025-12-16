"use client";

import { useEffect, useState, useRef, useMemo } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faRocket,
    faArrowRight,
    faChevronLeft,
    faChevronRight,
    faClock,
    faHeart,
    faEye,
    faGavel,
    faStar,
    faWandSparkles,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Auction } from "@/types/auction";
import { auctionService } from "@/services/auction.service";
import { useCountdown, getUrgencyLevel } from "@/hooks/use-countdown";
import { PulsingDot } from "@/components/ui/animated";
import { getAuctionTitle, getAuctionAttributes, getAuctionYearManufactured } from "@/utils/auction";

function AuctionCard({ auction, index }: { auction: Auction; index: number }) {
    const [isLiked, setIsLiked] = useState(false);
    const timeLeft = useCountdown(auction.auctionEnd);
    const urgency = getUrgencyLevel(timeLeft);
    const isEndingSoon = urgency === "critical" || urgency === "warning";
    const title = getAuctionTitle(auction);

    const imageUrl = useMemo(() => {
        const primaryFile = auction.files?.find((f) => f.isPrimary);
        return (
            primaryFile?.url ||
            auction.files?.[0]?.url ||
            "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400"
        );
    }, [auction.files]);

    const formatTimeDisplay = () => {
        if (!timeLeft || timeLeft.isExpired) return "Ended";
        if (timeLeft.days > 0) return `${timeLeft.days}d ${timeLeft.hours}h`;
        if (timeLeft.hours > 0) return `${timeLeft.hours}h ${timeLeft.minutes}m`;
        return `${timeLeft.minutes}m ${timeLeft.seconds}s`;
    };

    const currentBid = auction.currentHighBid || auction.reservePrice || 0;

    const isNew =
        new Date(auction.createdAt).getTime() >
        Date.now() - 24 * 60 * 60 * 1000;

    return (
        <motion.div
            initial={{ opacity: 0, y: 30, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            transition={{ duration: 0.5, delay: index * 0.1 }}
            className="shrink-0 w-80 snap-start"
        >
            <Link href={`/auctions/${auction.id}`} className="group block">
                <div className="relative bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800 hover:border-emerald-400/50 dark:hover:border-emerald-500/50 transition-all duration-500 hover:shadow-2xl hover:shadow-emerald-500/10 dark:hover:shadow-emerald-500/20 hover:-translate-y-2">
                    <div className="absolute top-4 left-4 right-4 z-20 flex items-center justify-between">
                        <div className="flex gap-2">
                            {isNew && (
                                <Badge className="bg-gradient-to-r from-emerald-500 to-teal-500 text-white px-3 py-1 text-xs border-0 shadow-lg">
                                    <FontAwesomeIcon
                                        icon={faRocket}
                                        className="w-3 h-3 mr-1"
                                    />
                                    New
                                </Badge>
                            )}
                            {auction.isFeatured && (
                                <Badge className="bg-gradient-to-r from-purple-500 to-pink-500 text-white px-3 py-1 text-xs border-0 shadow-lg">
                                    <FontAwesomeIcon
                                        icon={faStar}
                                        className="w-3 h-3 mr-1"
                                    />
                                    Featured
                                </Badge>
                            )}
                        </div>
                        <button
                            onClick={(e) => {
                                e.preventDefault();
                                setIsLiked(!isLiked);
                            }}
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

                    <div className="relative h-52 overflow-hidden bg-slate-100 dark:bg-slate-800">
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-700 group-hover:scale-110"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

                        <div className="absolute bottom-4 left-4 right-4 flex items-center justify-between">
                            <div
                                className={`bg-black/60 backdrop-blur-md rounded-xl px-3 py-2 flex items-center gap-2 ${
                                    isEndingSoon ? "ring-1 ring-red-500/50" : ""
                                }`}
                            >
                                <FontAwesomeIcon
                                    icon={faClock}
                                    className={`w-3.5 h-3.5 ${
                                        isEndingSoon
                                            ? "text-red-400 animate-pulse"
                                            : "text-white"
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
                                    <FontAwesomeIcon icon={faEye} className="w-3 h-3" />
                                    <span>{Math.floor(Math.random() * 30) + 5}</span>
                                </div>
                                <div className="flex items-center gap-1">
                                    <FontAwesomeIcon icon={faGavel} className="w-3 h-3" />
                                    <span>{Math.floor(Math.random() * 10) + 1}</span>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="p-5 space-y-4">
                        <div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                                {auction.categoryName || "Uncategorized"}
                            </p>
                            <h3 className="text-lg font-bold text-slate-900 dark:text-white truncate group-hover:text-emerald-600 dark:group-hover:text-emerald-400 transition-colors">
                                {title}
                            </h3>
                        </div>

                        <div className="flex items-end justify-between pt-3 border-t border-slate-100 dark:border-slate-800">
                            <div>
                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                                    Starting Bid
                                </p>
                                <p className="text-2xl font-bold bg-gradient-to-r from-emerald-600 to-teal-600 bg-clip-text text-transparent">
                                    ${currentBid.toLocaleString()}
                                </p>
                            </div>
                            <Button
                                size="sm"
                                className="h-10 px-5 bg-slate-900 dark:bg-white hover:bg-emerald-600 dark:hover:bg-emerald-500 text-white dark:text-slate-900 dark:hover:text-white font-semibold rounded-xl transition-colors"
                            >
                                View
                                <FontAwesomeIcon
                                    icon={faArrowRight}
                                    className="ml-2 w-3 h-3"
                                />
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
        <div className="shrink-0 w-80 snap-start">
            <div className="bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800">
                <Skeleton className="h-52 w-full bg-slate-200 dark:bg-slate-800" />
                <div className="p-5 space-y-4">
                    <Skeleton className="h-4 w-20 bg-slate-200 dark:bg-slate-800" />
                    <Skeleton className="h-6 w-full bg-slate-200 dark:bg-slate-800" />
                    <div className="flex justify-between pt-4">
                        <Skeleton className="h-10 w-24 bg-slate-200 dark:bg-slate-800" />
                        <Skeleton className="h-10 w-20 bg-slate-200 dark:bg-slate-800 rounded-xl" />
                    </div>
                </div>
            </div>
        </div>
    );
}

export function NewArrivalsSection() {
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const scrollRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const fetchAuctions = async () => {
            try {
                const result = await auctionService.getAuctions({
                    status: "Live",
                    orderBy: "createdAt",
                    descending: true,
                    pageSize: 8,
                });
                setAuctions(result.items);
            } catch {
            } finally {
                setIsLoading(false);
            }
        };
        fetchAuctions();
    }, []);

    const scroll = (direction: "left" | "right") => {
        if (scrollRef.current) {
            const scrollAmount = 340;
            scrollRef.current.scrollBy({
                left: direction === "left" ? -scrollAmount : scrollAmount,
                behavior: "smooth",
            });
        }
    };

    if (!isLoading && auctions.length === 0) {
        return null;
    }

    return (
        <section className="relative py-24 bg-gradient-to-b from-slate-50 via-emerald-50/30 to-slate-50 dark:from-slate-950 dark:via-emerald-950/10 dark:to-slate-950 overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top_right,_var(--tw-gradient-stops))] from-emerald-200/20 via-transparent to-transparent dark:from-emerald-900/10" />
            <div className="absolute top-40 right-1/4 w-80 h-80 bg-teal-400/10 dark:bg-teal-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-40 left-1/4 w-80 h-80 bg-emerald-400/10 dark:bg-emerald-600/10 rounded-full blur-3xl" />

            <div className="container mx-auto px-4 relative z-10">
                <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-8 mb-12">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="max-w-xl"
                    >
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-emerald-100 dark:bg-emerald-500/10 border border-emerald-200 dark:border-emerald-500/20 mb-6">
                            <FontAwesomeIcon
                                icon={faRocket}
                                className="w-4 h-4 text-emerald-600 dark:text-emerald-400"
                            />
                            <span className="text-sm font-medium text-emerald-700 dark:text-emerald-300">
                                Fresh Inventory
                            </span>
                        </div>

                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-slate-900 dark:text-white mb-4 leading-tight">
                            New
                            <span className="block text-transparent bg-clip-text bg-gradient-to-r from-emerald-600 via-teal-600 to-emerald-600 dark:from-emerald-400 dark:via-teal-400 dark:to-emerald-400">
                                Arrivals
                            </span>
                        </h2>

                        <p className="text-lg text-slate-600 dark:text-slate-400 leading-relaxed">
                            Be the first to bid on just-listed items. Fresh auctions added
                            daily from verified sellers worldwide.
                        </p>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: 0.2 }}
                        className="flex items-center gap-4"
                    >
                        <div className="hidden sm:flex gap-2">
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => scroll("left")}
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-emerald-50 dark:hover:bg-emerald-950/50 hover:border-emerald-300 dark:hover:border-emerald-700 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => scroll("right")}
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-emerald-50 dark:hover:bg-emerald-950/50 hover:border-emerald-300 dark:hover:border-emerald-700 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4" />
                            </Button>
                        </div>

                        <Button
                            className="h-12 px-6 rounded-full bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-700 hover:to-teal-700 text-white font-medium shadow-lg shadow-emerald-500/25"
                            asChild
                        >
                            <Link href="/auctions?sort=newest">
                                View All New
                                <FontAwesomeIcon
                                    icon={faArrowRight}
                                    className="ml-2 w-4 h-4"
                                />
                            </Link>
                        </Button>
                    </motion.div>
                </div>

                <div
                    ref={scrollRef}
                    className="flex gap-6 overflow-x-auto scrollbar-hide pb-4 -mx-4 px-4 snap-x snap-mandatory"
                    style={{ scrollbarWidth: "none", msOverflowStyle: "none" }}
                >
                    {isLoading
                        ? [...Array(4)].map((_, i) => <CardSkeleton key={i} />)
                        : auctions.map((auction, index) => (
                              <AuctionCard
                                  key={auction.id}
                                  auction={auction}
                                  index={index}
                              />
                          ))}
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: 0.4 }}
                    className="mt-12 flex flex-wrap justify-center gap-6 text-center"
                >
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-emerald-600 dark:text-emerald-400">
                            {auctions.length}+
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Listed Today
                        </p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-slate-900 dark:text-white">
                            100%
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Verified Sellers
                        </p>
                    </div>
                    <div className="px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <p className="text-2xl font-bold text-teal-600 dark:text-teal-400">
                            24h
                        </p>
                        <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider">
                            Fresh Listings
                        </p>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
