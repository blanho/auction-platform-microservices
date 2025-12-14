"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { motion } from "framer-motion";
import { Clock, Flame } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Auction } from "@/types/auction";
import { auctionService } from "@/services/auction.service";

interface CountdownProps {
    endTime: Date;
}

function Countdown({ endTime }: CountdownProps) {
    const [timeLeft, setTimeLeft] = useState({
        hours: 0,
        minutes: 0,
        seconds: 0,
    });
    const [isUrgent, setIsUrgent] = useState(false);

    useEffect(() => {
        const calculateTimeLeft = () => {
            const difference = endTime.getTime() - Date.now();

            if (difference > 0) {
                const hours = Math.floor(difference / (1000 * 60 * 60));
                const minutes = Math.floor(
                    (difference % (1000 * 60 * 60)) / (1000 * 60)
                );
                const seconds = Math.floor((difference % (1000 * 60)) / 1000);

                setTimeLeft({ hours, minutes, seconds });
                setIsUrgent(hours < 1);
            }
        };

        calculateTimeLeft();
        const timer = setInterval(calculateTimeLeft, 1000);
        return () => clearInterval(timer);
    }, [endTime]);

    const formatTime = (value: number) => value.toString().padStart(2, "0");

    return (
        <div
            className={`flex items-center gap-1.5 font-mono text-sm font-bold ${
                isUrgent ? "text-red-500" : "text-slate-900 dark:text-white"
            }`}
        >
            <Clock className={`w-4 h-4 ${isUrgent ? "animate-pulse" : ""}`} />
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
    const primaryFile = auction.files?.find((f) => f.isPrimary);
    const imageUrl =
        primaryFile?.url ||
        auction.files?.[0]?.url ||
        "https://images.unsplash.com/photo-1503376780353-7e6692767b70?w=400";

    const currentBid = auction.currentHighBid || auction.reservePrice || 0;
    const endTime = new Date(auction.auctionEnd);

    return (
        <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: index * 0.1 }}
        >
            <Link href={`/auctions/${auction.id}`} className="group block">
                <div className="relative bg-white dark:bg-slate-800 rounded-2xl overflow-hidden border border-slate-200 dark:border-slate-700 hover:border-amber-400 dark:hover:border-amber-400 transition-all duration-300 hover:shadow-xl">
                    {/* Urgent Badge */}
                    <div className="absolute top-3 left-3 z-10">
                        <Badge className="bg-red-500 text-white px-2 py-0.5 text-xs animate-pulse">
                            <Flame className="w-3 h-3 mr-1" />
                            Ending Soon
                        </Badge>
                    </div>

                    {/* Image */}
                    <div className="relative h-40 overflow-hidden">
                        <Image
                            src={imageUrl}
                            alt={auction.title}
                            fill
                            className="object-cover group-hover:scale-105 transition-transform duration-500"
                            unoptimized={imageUrl.includes("unsplash")}
                        />
                        <div className="absolute inset-0 bg-linear-to-t from-black/60 to-transparent" />

                        {/* Countdown overlay */}
                        <div className="absolute bottom-3 right-3 bg-black/70 backdrop-blur-sm rounded-lg px-3 py-1.5">
                            <Countdown endTime={endTime} />
                        </div>
                    </div>

                    {/* Content */}
                    <div className="p-4">
                        <h3 className="font-semibold text-slate-900 dark:text-white truncate group-hover:text-amber-500 transition-colors">
                            {auction.year} {auction.make} {auction.model}
                        </h3>
                        <div className="mt-2 flex items-center justify-between">
                            <div>
                                <div className="text-xs text-slate-500 dark:text-slate-400">
                                    Current Bid
                                </div>
                                <div className="text-lg font-bold text-amber-500">
                                    ${currentBid.toLocaleString()}
                                </div>
                            </div>
                            <Button
                                size="sm"
                                className="bg-amber-400 hover:bg-amber-500 text-slate-900 font-semibold"
                            >
                                Bid Now
                            </Button>
                        </div>
                    </div>
                </div>
            </Link>
        </motion.div>
    );
}

export function EndingSoonSection() {
    const [auctions, setAuctions] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        const fetchAuctions = async () => {
            try {
                const result = await auctionService.getAuctions({
                    status: "Live",
                    orderBy: "auctionEnd",
                    descending: false,
                    pageSize: 4,
                });
                setAuctions(result.items);
            } catch (error) {
                console.error("Failed to fetch ending soon auctions:", error);
            } finally {
                setIsLoading(false);
            }
        };
        fetchAuctions();
    }, []);

    if (isLoading) {
        return (
            <section className="py-16 bg-white dark:bg-slate-950">
                <div className="container mx-auto px-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                        {[...Array(4)].map((_, i) => (
                            <div
                                key={i}
                                className="h-64 bg-slate-200 dark:bg-slate-800 rounded-2xl animate-pulse"
                            />
                        ))}
                    </div>
                </div>
            </section>
        );
    }

    if (auctions.length === 0) {
        return null;
    }

    return (
        <section className="py-16 bg-white dark:bg-slate-950">
            <div className="container mx-auto px-4">
                <div className="flex items-center justify-between mb-8">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-red-500/10 flex items-center justify-center">
                            <Flame className="w-5 h-5 text-red-500" />
                        </div>
                        <div>
                            <h2 className="text-3xl font-bold text-slate-900 dark:text-white">
                                Ending Soon
                            </h2>
                            <p className="text-slate-600 dark:text-slate-400">
                                Don&apos;t miss these auctions!
                            </p>
                        </div>
                    </div>
                    <Button variant="outline" asChild>
                        <Link href="/auctions?sort=ending-soon">View All</Link>
                    </Button>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    {auctions.map((auction, index) => (
                        <EndingSoonCard
                            key={auction.id}
                            auction={auction}
                            index={index}
                        />
                    ))}
                </div>
            </div>
        </section>
    );
}
