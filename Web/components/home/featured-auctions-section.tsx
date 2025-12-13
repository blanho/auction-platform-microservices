"use client";

import { useState, useEffect, useMemo } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Clock, Flame, ArrowRight, Heart, Loader2 } from "lucide-react";
import Link from "next/link";
import { useFeaturedAuctions } from "@/hooks/use-auctions";
import { Auction } from "@/types/auction";

function AuctionCard({ auction }: { auction: Auction }) {
    const [timeLeft, setTimeLeft] = useState("");
    const [intensity, setIntensity] = useState(0);
    const [isLiked, setIsLiked] = useState(false);
    const [isEndingSoon, setIsEndingSoon] = useState(false);

    const endTime = useMemo(() => new Date(auction.auctionEnd), [auction.auctionEnd]);

    const imageUrl = useMemo(() => {
        const primaryFile = auction.files?.find(f => f.isPrimary);
        const firstFile = auction.files?.[0];
        return primaryFile?.url || firstFile?.url || "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400";
    }, [auction.files]);

    useEffect(() => {
        const updateTime = () => {
            const diff = endTime.getTime() - Date.now();
            if (diff <= 0) {
                setTimeLeft("Ended");
                setIsEndingSoon(false);
                return;
            }

            const hours = Math.floor(diff / (1000 * 60 * 60));
            const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
            const seconds = Math.floor((diff % (1000 * 60)) / 1000);

            if (hours > 0) {
                setTimeLeft(`${hours}h ${minutes}m`);
            } else if (minutes > 0) {
                setTimeLeft(`${minutes}m ${seconds}s`);
            } else {
                setTimeLeft(`${seconds}s`);
            }

            setIsEndingSoon(diff < 60 * 60 * 1000);

            const timeIntensity = Math.max(0, 100 - (diff / (1000 * 60 * 60 * 6)) * 100);
            setIntensity(timeIntensity);
        };

        updateTime();
        const timer = setInterval(updateTime, 1000);
        return () => clearInterval(timer);
    }, [endTime]);

    const currentBid = auction.currentHighBid || auction.reservePrice;

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            whileHover={{ y: -8 }}
            transition={{ duration: 0.3 }}
        >
            <Card className="group overflow-hidden border-slate-200 dark:border-slate-700 hover:shadow-xl hover:shadow-purple-500/10 transition-all duration-300">
                {/* Image container */}
                <div className="relative h-48 overflow-hidden bg-slate-100 dark:bg-slate-800">
                    <Image
                        src={imageUrl}
                        alt={`${auction.make} ${auction.model}`}
                        fill
                        className="object-cover group-hover:scale-110 transition-transform duration-500"
                        unoptimized={imageUrl.includes("unsplash")}
                    />

                    {/* Overlay gradient */}
                    <div className="absolute inset-0 bg-gradient-to-t from-black/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />

                    {/* Badges */}
                    <div className="absolute top-3 left-3 flex gap-2">
                        {auction.isFeatured && (
                            <Badge className="bg-purple-500 text-white">
                                <Flame className="w-3 h-3 mr-1" />
                                Featured
                            </Badge>
                        )}
                        {isEndingSoon && (
                            <Badge variant="destructive">
                                <Clock className="w-3 h-3 mr-1" />
                                Ending Soon
                            </Badge>
                        )}
                    </div>

                    {/* Like button */}
                    <button
                        onClick={() => setIsLiked(!isLiked)}
                        className="absolute top-3 right-3 p-2 rounded-full bg-white/90 dark:bg-slate-800/90 hover:bg-white dark:hover:bg-slate-800 transition-colors"
                    >
                        <Heart
                            className={`w-5 h-5 transition-colors ${isLiked ? "fill-red-500 text-red-500" : "text-slate-600 dark:text-slate-400"
                                }`}
                        />
                    </button>
                </div>

                <CardContent className="p-4 space-y-4">
                    {/* Title */}
                    <h3 className="font-semibold text-slate-900 dark:text-white truncate group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                        {auction.make} {auction.model}
                    </h3>

                    {/* Price and time */}
                    <div className="flex items-center justify-between">
                        <div>
                            <div className="text-xs text-slate-500 dark:text-slate-400">Current Bid</div>
                            <div className="text-lg font-bold text-purple-600 dark:text-purple-400">
                                ${currentBid.toLocaleString()}
                            </div>
                        </div>
                        <div className="text-right">
                            <div className="text-xs text-slate-500 dark:text-slate-400">Time Left</div>
                            <div className={`font-semibold ${isEndingSoon ? "text-red-500" : "text-slate-900 dark:text-white"
                                }`}>
                                {timeLeft}
                            </div>
                        </div>
                    </div>

                    {/* Intensity bar */}
                    <div className="space-y-1">
                        <div className="flex items-center justify-between text-xs">
                            <span className="text-slate-500 dark:text-slate-400">
                                {auction.categoryName || "Uncategorized"}
                            </span>
                            <span className="text-slate-500 dark:text-slate-400">
                                {intensity > 70 ? "🔥 Ending soon" : intensity > 40 ? "📈 Active" : "✨ New"}
                            </span>
                        </div>
                        <Progress
                            value={intensity}
                            className="h-1.5"
                        />
                    </div>

                    {/* Bid button */}
                    <Button
                        className="w-full group/btn bg-slate-900 dark:bg-slate-100 hover:bg-purple-600 dark:hover:bg-purple-500 text-white dark:text-slate-900 dark:hover:text-white transition-colors"
                        asChild
                    >
                        <Link href={`/auctions/${auction.id}`}>
                            Place Bid
                            <ArrowRight className="ml-2 w-4 h-4 group-hover/btn:translate-x-1 transition-transform" />
                        </Link>
                    </Button>
                </CardContent>
            </Card>
        </motion.div>
    );
}

export function FeaturedAuctionsSection() {
    const { data: featuredAuctions, isLoading, error } = useFeaturedAuctions(6);

    if (isLoading) {
        return (
            <section className="py-20 bg-slate-50 dark:bg-slate-900/50">
                <div className="container mx-auto px-4">
                    <div className="flex items-center justify-center py-12">
                        <Loader2 className="w-8 h-8 animate-spin text-purple-600" />
                    </div>
                </div>
            </section>
        );
    }

    if (error || !featuredAuctions || !Array.isArray(featuredAuctions) || featuredAuctions.length === 0) {
        if (error) {
            console.error("[FeaturedAuctionsSection] Error fetching auctions:", error);
        }
        return null;
    }

    return (
        <section className="py-20 bg-slate-50 dark:bg-slate-900/50">
            <div className="container mx-auto px-4">
                {/* Header */}
                <div className="flex flex-col md:flex-row md:items-end md:justify-between gap-4 mb-12">
                    <div>
                        <Badge variant="outline" className="mb-4">
                            <Flame className="w-4 h-4 mr-2 text-orange-500" />
                            Featured Auctions
                        </Badge>
                        <h2 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white">
                            Trending Right Now
                        </h2>
                        <p className="text-slate-600 dark:text-slate-400 mt-2">
                            Don&apos;t miss out on these popular items
                        </p>
                    </div>
                    <Button variant="outline" asChild>
                        <Link href="/auctions">
                            View All Auctions
                            <ArrowRight className="ml-2 w-4 h-4" />
                        </Link>
                    </Button>
                </div>

                {/* Grid */}
                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {featuredAuctions.map((auction) => (
                        <AuctionCard key={auction.id} auction={auction} />
                    ))}
                </div>
            </div>
        </section>
    );
}
