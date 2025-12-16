"use client";

import { useRef, useCallback } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faClock,
    faFire,
    faArrowRight,
    faChevronLeft,
    faChevronRight,
    faGavel,
    faHeart,
    faEye,
} from "@fortawesome/free-solid-svg-icons";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Auction } from "@/types/auction";
import { useAuctionsQuery } from "@/hooks/queries";
import { PulsingDot } from "@/components/ui/animated";
import { FEATURED, UI, URGENCY } from "@/constants/config";
import { useInterval } from "@/hooks/use-interval";
import { getAuctionTitle } from "@/utils/auction";

interface CountdownProps {
    endTime: Date;
    variant?: "overlay" | "inline";
}

function Countdown({ endTime, variant = "overlay" }: CountdownProps) {
    const [timeLeft, setTimeLeft] = useState({
        hours: 0,
        minutes: 0,
        seconds: 0,
    });
    const [isUrgent, setIsUrgent] = useState(false);

    const calculateTimeLeft = useCallback(() => {
        const difference = endTime.getTime() - Date.now();

        if (difference > 0) {
            const hours = Math.floor(difference / (1000 * 60 * 60));
            const minutes = Math.floor(
                (difference % (1000 * 60 * 60)) / (1000 * 60)
            );
            const seconds = Math.floor((difference % (1000 * 60)) / 1000);

            setTimeLeft({ hours, minutes, seconds });
            setIsUrgent(hours < URGENCY.CRITICAL_HOURS);
        }
    }, [endTime]);

    useEffect(() => {
        calculateTimeLeft();
    }, [calculateTimeLeft]);

    useInterval(calculateTimeLeft, 1000);

    const formatTime = (value: number) => value.toString().padStart(2, "0");

    if (variant === "inline") {
        return (
            <div className="flex items-center gap-2">
                {[
                    { value: timeLeft.hours, label: "HRS" },
                    { value: timeLeft.minutes, label: "MIN" },
                    { value: timeLeft.seconds, label: "SEC" },
                ].map((item, idx) => (
                    <div key={idx} className="text-center">
                        <div
                            className={`w-12 h-12 rounded-xl flex items-center justify-center font-mono text-lg font-bold ${
                                isUrgent
                                    ? "bg-red-500/20 text-red-500"
                                    : "bg-slate-100 dark:bg-white/10 text-slate-900 dark:text-white"
                            }`}
                        >
                            {formatTime(item.value)}
                        </div>
                        <p className="text-[10px] text-slate-500 dark:text-slate-400 mt-1">
                            {item.label}
                        </p>
                    </div>
                ))}
            </div>
        );
    }

    return (
        <div
            className={`flex items-center gap-1.5 font-mono text-sm font-bold ${
                isUrgent ? "text-red-400" : "text-white"
            }`}
        >
            <FontAwesomeIcon
                icon={faClock}
                className={`w-3.5 h-3.5 ${isUrgent ? "animate-pulse" : ""}`}
            />
            <span>
                {formatTime(timeLeft.hours)}:{formatTime(timeLeft.minutes)}:
                {formatTime(timeLeft.seconds)}
            </span>
        </div>
    );
}

interface AuctionCardProps {
    auction: Auction;
    index: number;
}

function EndingSoonCard({ auction, index }: AuctionCardProps) {
    const [isLiked, setIsLiked] = useState(false);
    const title = getAuctionTitle(auction);
    const primaryFile = auction.files?.find((f) => f.isPrimary);
    const imageUrl =
        primaryFile?.url ||
        auction.files?.[0]?.url ||
        "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400";

    const currentBid = auction.currentHighBid || auction.reservePrice || 0;
    const endTime = new Date(auction.auctionEnd);

    const handleLikeToggle = useCallback((e: React.MouseEvent) => {
        e.preventDefault();
        setIsLiked(prev => !prev);
    }, []);

    return (
        <motion.div
            initial={{ opacity: 0, y: 30, scale: 0.95 }}
            animate={{ opacity: 1, y: 0, scale: 1 }}
            transition={{ duration: UI.ANIMATION.SLIDE_DURATION, delay: index * UI.ANIMATION.STAGGER_DELAY }}
            className="shrink-0 w-80 snap-start"
        >
            <Link href={`/auctions/${auction.id}`} className="group block">
                <div className="relative bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800 hover:border-red-400/50 dark:hover:border-red-500/50 transition-all duration-500 hover:shadow-2xl hover:shadow-red-500/10 dark:hover:shadow-red-500/20 hover:-translate-y-2">
                    <div className="absolute top-4 left-4 right-4 z-20 flex items-center justify-between">
                        <Badge className="bg-gradient-to-r from-red-500 to-orange-500 text-white px-3 py-1 text-xs border-0 shadow-lg">
                            <PulsingDot className="mr-1.5" />
                            Ending Soon
                        </Badge>
                        <button
                            onClick={handleLikeToggle}
                            className="w-9 h-9 rounded-full bg-white/90 dark:bg-slate-800/90 shadow-lg flex items-center justify-center hover:scale-110 transition-transform"
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

                    <div className="relative h-52 overflow-hidden">
                        <Image
                            src={imageUrl}
                            alt={title}
                            fill
                            className="object-cover transition-transform duration-700 group-hover:scale-110"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-gradient-to-t from-black/80 via-black/20 to-transparent" />

                        <div className="absolute bottom-4 left-4 right-4">
                            <div className="flex items-center justify-between">
                                <div className="bg-black/60 backdrop-blur-md rounded-xl px-3 py-2">
                                    <Countdown endTime={endTime} />
                                </div>
                                <div className="flex items-center gap-2 text-white/80 text-xs">
                                    <FontAwesomeIcon icon={faEye} className="w-3 h-3" />
                                    <FontAwesomeIcon icon={faGavel} className="w-3 h-3 ml-2" />
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="p-5 space-y-4">
                        <div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                                {auction.categoryName || "Auction"}
                            </p>
                            <h3 className="text-lg font-bold text-slate-900 dark:text-white truncate group-hover:text-red-500 dark:group-hover:text-red-400 transition-colors">
                                {title}
                            </h3>
                        </div>

                        <div className="flex items-end justify-between pt-2 border-t border-slate-100 dark:border-slate-800">
                            <div>
                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                                    Current Bid
                                </p>
                                <p className="text-2xl font-bold bg-gradient-to-r from-red-500 to-orange-500 bg-clip-text text-transparent">
                                    ${currentBid.toLocaleString()}
                                </p>
                            </div>
                            <Button
                                size="sm"
                                className="h-10 px-5 bg-gradient-to-r from-red-500 to-orange-500 hover:from-red-600 hover:to-orange-600 text-white font-semibold rounded-xl shadow-lg shadow-red-500/25"
                            >
                                Bid Now
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
        <div className="shrink-0 w-80 snap-start">
            <div className="bg-white dark:bg-slate-900 rounded-3xl overflow-hidden border border-slate-200 dark:border-slate-800">
                <Skeleton className="h-52 w-full bg-slate-200 dark:bg-slate-800" />
                <div className="p-5 space-y-4">
                    <Skeleton className="h-4 w-20 bg-slate-200 dark:bg-slate-800" />
                    <Skeleton className="h-6 w-full bg-slate-200 dark:bg-slate-800" />
                    <div className="flex justify-between pt-4">
                        <Skeleton className="h-10 w-24 bg-slate-200 dark:bg-slate-800" />
                        <Skeleton className="h-10 w-28 bg-slate-200 dark:bg-slate-800 rounded-xl" />
                    </div>
                </div>
            </div>
        </div>
    );
}

export function EndingSoonSection() {
    const { data, isLoading } = useAuctionsQuery({
        status: "Live",
        orderBy: "auctionEnd",
        descending: false,
        pageSize: FEATURED.DEFAULT_LIMIT,
    });
    const auctions = data?.items ?? [];
    const scrollRef = useRef<HTMLDivElement>(null);

    const scroll = useCallback((direction: "left" | "right") => {
        if (scrollRef.current) {
            scrollRef.current.scrollBy({
                left: direction === "left" ? -UI.SCROLL_AMOUNT : UI.SCROLL_AMOUNT,
                behavior: "smooth",
            });
        }
    }, []);

    if (!isLoading && auctions.length === 0) {
        return null;
    }

    return (
        <section className="relative py-24 bg-gradient-to-b from-slate-50 via-red-50/30 to-slate-50 dark:from-slate-950 dark:via-red-950/10 dark:to-slate-950 overflow-hidden">
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_center,_var(--tw-gradient-stops))] from-red-200/20 via-transparent to-transparent dark:from-red-900/10" />
            <div className="absolute top-20 right-1/4 w-72 h-72 bg-orange-400/10 dark:bg-orange-600/10 rounded-full blur-3xl" />
            <div className="absolute bottom-20 left-1/4 w-72 h-72 bg-red-400/10 dark:bg-red-600/10 rounded-full blur-3xl" />

            <div className="container mx-auto px-4 relative z-10">
                <div className="flex flex-col lg:flex-row lg:items-end lg:justify-between gap-8 mb-12">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="max-w-xl"
                    >
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-red-100 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 mb-6">
                            <FontAwesomeIcon
                                icon={faFire}
                                className="w-4 h-4 text-red-500 animate-pulse"
                            />
                            <span className="text-sm font-medium text-red-600 dark:text-red-400">
                                Act Fast
                            </span>
                        </div>

                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-slate-900 dark:text-white mb-4 leading-tight">
                            Ending
                            <span className="block text-transparent bg-clip-text bg-gradient-to-r from-red-500 via-orange-500 to-amber-500">
                                Very Soon
                            </span>
                        </h2>

                        <p className="text-lg text-slate-600 dark:text-slate-400 leading-relaxed">
                            These auctions are about to close. Place your final bids before
                            time runs out!
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
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-red-50 dark:hover:bg-red-950/50 hover:border-red-300 dark:hover:border-red-700 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4" />
                            </Button>
                            <Button
                                variant="outline"
                                size="icon"
                                onClick={() => scroll("right")}
                                className="w-12 h-12 rounded-full border-slate-300 dark:border-slate-700 bg-white dark:bg-slate-900 hover:bg-red-50 dark:hover:bg-red-950/50 hover:border-red-300 dark:hover:border-red-700 text-slate-700 dark:text-white"
                            >
                                <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4" />
                            </Button>
                        </div>

                        <Button
                            className="h-12 px-6 rounded-full bg-gradient-to-r from-red-500 to-orange-500 hover:from-red-600 hover:to-orange-600 text-white font-medium shadow-lg shadow-red-500/25"
                            asChild
                        >
                            <Link href="/auctions?sort=ending-soon">
                                View All
                                <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
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
                        ? Array.from({ length: UI.SKELETON.LIST_COUNT }).map((_, i) => <CardSkeleton key={i} />)
                        : auctions.map((auction, index) => (
                              <EndingSoonCard
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
                    className="mt-12 text-center"
                >
                    <div className="inline-flex items-center gap-3 px-6 py-3 rounded-2xl bg-white/80 dark:bg-slate-900/80 border border-slate-200 dark:border-slate-800 shadow-sm backdrop-blur-sm">
                        <FontAwesomeIcon
                            icon={faClock}
                            className="w-5 h-5 text-red-500 animate-pulse"
                        />
                        <span className="text-slate-700 dark:text-slate-300 font-medium">
                            Auctions refresh every minute
                        </span>
                    </div>
                </motion.div>
            </div>
        </section>
    );
}
