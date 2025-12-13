"use client";

import { useMemo } from "react";
import Image from "next/image";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Clock, Flame, ArrowRight, Heart, Loader2, Eye, Users } from "lucide-react";
import Link from "next/link";
import { useFeaturedAuctions } from "@/hooks/use-auctions";
import { useCountdown, getUrgencyLevel } from "@/hooks/use-countdown";
import { Auction } from "@/types/auction";
import { AnimatedSection, StaggerContainer, StaggerItem } from "@/components/ui/animated";
import { useState } from "react";

function AuctionCard({ auction, featured = false }: { auction: Auction; featured?: boolean }) {
    const [isLiked, setIsLiked] = useState(false);
    const timeLeft = useCountdown(auction.auctionEnd);
    const urgency = getUrgencyLevel(timeLeft);
    const isEndingSoon = urgency === "critical" || urgency === "warning";

    const imageUrl = useMemo(() => {
        const primaryFile = auction.files?.find(f => f.isPrimary);
        return primaryFile?.url || auction.files?.[0]?.url || "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=400";
    }, [auction.files]);

    const formatTimeDisplay = () => {
        if (!timeLeft || timeLeft.isExpired) return "Ended";
        if (timeLeft.days > 0) return `${timeLeft.days}d ${timeLeft.hours}h`;
        if (timeLeft.hours > 0) return `${timeLeft.hours}h ${timeLeft.minutes}m`;
        return `${timeLeft.minutes}m ${timeLeft.seconds}s`;
    };

    const currentBid = auction.currentHighBid || auction.reservePrice;

    return (
        <Card className={`group overflow-hidden border-slate-200 dark:border-slate-700 hover:shadow-2xl hover:shadow-purple-500/10 transition-all duration-300 hover:-translate-y-2 ${featured ? "lg:col-span-2 lg:row-span-2" : ""}`}>
            <div className={`relative ${featured ? "h-64 lg:h-80" : "h-48"} overflow-hidden bg-slate-100 dark:bg-slate-800`}>
                <Image
                    src={imageUrl}
                    alt={`${auction.make} ${auction.model}`}
                    fill
                    className="object-cover transition-transform duration-500 group-hover:scale-105"
                    unoptimized={imageUrl.includes("unsplash")}
                />

                <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

                <div className="absolute top-3 left-3 flex flex-wrap gap-2">
                    {auction.isFeatured && (
                        <Badge className="bg-gradient-to-r from-purple-500 to-pink-500 text-white border-0 shadow-lg">
                            <Flame className="w-3 h-3 mr-1" />
                            Featured
                        </Badge>
                    )}
                    {isEndingSoon && (
                        <Badge className="bg-red-500 text-white border-0 shadow-lg">
                            <Clock className="w-3 h-3 mr-1" />
                            Ending Soon
                        </Badge>
                    )}
                </div>

                <button
                    onClick={(e) => { e.preventDefault(); setIsLiked(!isLiked); }}
                    className="absolute top-3 right-3 p-2.5 rounded-full bg-white/90 dark:bg-slate-800/90 shadow-lg hover:scale-110 transition-transform"
                >
                    <Heart className={`w-5 h-5 transition-colors ${isLiked ? "fill-red-500 text-red-500" : "text-slate-600 dark:text-slate-400"}`} />
                </button>

                <div className="absolute bottom-3 left-3 right-3 flex justify-between items-center opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                    <div className="flex items-center gap-3 text-white/90">
                        <div className="flex items-center gap-1">
                            <Eye className="w-4 h-4" />
                            <span className="text-sm">{"bidCount" in auction ? String(auction.bidCount) : "12"}</span>
                        </div>
                        <div className="flex items-center gap-1">
                            <Users className="w-4 h-4" />
                            <span className="text-sm">{"bidCount" in auction ? String(auction.bidCount) : "5"} bids</span>
                        </div>
                    </div>
                </div>
            </div>

            <CardContent className="p-5 space-y-4">
                <div>
                    <p className="text-xs text-slate-500 dark:text-slate-400 uppercase tracking-wider mb-1">
                        {auction.categoryName || "Uncategorized"}
                    </p>
                    <h3 className={`font-bold text-slate-900 dark:text-white group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors line-clamp-1 ${featured ? "text-xl" : "text-base"}`}>
                        {auction.make} {auction.model}
                    </h3>
                </div>

                <div className="flex items-center justify-between">
                    <div>
                        <p className="text-xs text-slate-500 dark:text-slate-400">Current Bid</p>
                        <p className={`font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent ${featured ? "text-2xl" : "text-lg"}`}>
                            ${currentBid.toLocaleString()}
                        </p>
                    </div>
                    <div className="text-right">
                        <p className="text-xs text-slate-500 dark:text-slate-400">Time Left</p>
                        <p className={`font-semibold ${isEndingSoon ? "text-red-500" : "text-slate-900 dark:text-white"}`}>
                            {formatTimeDisplay()}
                        </p>
                    </div>
                </div>

                <Button
                    className={`w-full bg-slate-900 dark:bg-white hover:bg-purple-600 dark:hover:bg-purple-500 text-white dark:text-slate-900 dark:hover:text-white transition-colors ${featured ? "h-12" : "h-10"}`}
                    asChild
                >
                    <Link href={`/auctions/${auction.id}`}>
                        Place Bid
                        <ArrowRight className="ml-2 w-4 h-4" />
                    </Link>
                </Button>
            </CardContent>
        </Card>
    );
}

export function FeaturedAuctionsSection() {
    const { data: featuredAuctions, isLoading, error } = useFeaturedAuctions(6);

    if (isLoading) {
        return (
            <section className="py-20 bg-white dark:bg-slate-950">
                <div className="container mx-auto px-4">
                    <div className="flex items-center justify-center py-12">
                        <Loader2 className="w-8 h-8 animate-spin text-purple-600" />
                    </div>
                </div>
            </section>
        );
    }

    if (error || !featuredAuctions || !Array.isArray(featuredAuctions) || featuredAuctions.length === 0) {
        return null;
    }

    return (
        <AnimatedSection className="py-20 bg-slate-50 dark:bg-slate-900/50">
            <div className="container mx-auto px-4">
                <div className="flex flex-col md:flex-row md:items-end md:justify-between gap-6 mb-12">
                    <div>
                        <p className="text-sm font-semibold text-purple-600 dark:text-purple-400 mb-3 uppercase tracking-wider">
                            Curated Selection
                        </p>
                        <h2 className="text-3xl md:text-4xl lg:text-5xl font-bold text-slate-900 dark:text-white">
                            Featured Auctions
                        </h2>
                        <p className="text-lg text-slate-600 dark:text-slate-400 mt-3 max-w-lg">
                            Premium items handpicked by our experts — exceptional quality, verified sellers.
                        </p>
                    </div>
                    <Button variant="outline" className="self-start md:self-auto h-12 px-6 border-2 rounded-full hover:bg-purple-600 hover:text-white hover:border-purple-600 transition-all" asChild>
                        <Link href="/auctions">
                            View All Auctions
                            <ArrowRight className="ml-2 w-4 h-4" />
                        </Link>
                    </Button>
                </div>

                <StaggerContainer className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {featuredAuctions.map((auction, index) => (
                        <StaggerItem key={auction.id}>
                            <AuctionCard auction={auction} featured={index === 0} />
                        </StaggerItem>
                    ))}
                </StaggerContainer>
            </div>
        </AnimatedSection>
    );
}
