"use client";

import { useState, useEffect, useMemo } from "react";
import { motion } from "framer-motion";
import Image from "next/image";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Flame, Clock, ArrowRight, Play } from "lucide-react";
import Link from "next/link";
import { useFeaturedAuctions } from "@/hooks/use-auctions";

export function HeroSection() {
    const [timeLeft, setTimeLeft] = useState({
        hours: 0,
        minutes: 0,
        seconds: 0,
    });
    const [isUrgent, setIsUrgent] = useState(false);
    const { data: featuredAuctions } = useFeaturedAuctions(1);

    const heroAuction = featuredAuctions?.[0];

    const endTime = useMemo(() => {
        if (!heroAuction) return null;
        return new Date(heroAuction.auctionEnd);
    }, [heroAuction]);

    const imageUrl = useMemo(() => {
        if (!heroAuction) return "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800";
        const primaryFile = heroAuction.files?.find(f => f.isPrimary);
        const firstFile = heroAuction.files?.[0];
        return primaryFile?.url || firstFile?.url || "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800";
    }, [heroAuction]);

    useEffect(() => {
        if (!endTime) return;

        const calculateTimeLeft = () => {
            const difference = endTime.getTime() - Date.now();

            if (difference > 0) {
                const hours = Math.floor(difference / (1000 * 60 * 60));
                const minutes = Math.floor((difference % (1000 * 60 * 60)) / (1000 * 60));
                const seconds = Math.floor((difference % (1000 * 60)) / 1000);

                setTimeLeft({ hours, minutes, seconds });
                setIsUrgent(difference < 10000); // Last 10 seconds
            }
        };

        calculateTimeLeft();
        const timer = setInterval(calculateTimeLeft, 1000);
        return () => clearInterval(timer);
    }, [endTime]);

    const formatTime = (value: number) => value.toString().padStart(2, "0");

    const currentBid = heroAuction?.currentHighBid || heroAuction?.reservePrice || 0;
    const title = heroAuction ? `${heroAuction.make} ${heroAuction.model}` : "Featured Auction";

    return (
        <section className="relative overflow-hidden bg-gradient-to-br from-slate-50 via-white to-purple-50 dark:from-slate-950 dark:via-slate-900 dark:to-purple-950">
            {/* Background decorations */}
            <div className="absolute inset-0 overflow-hidden">
                <div className="absolute -top-40 -right-40 w-80 h-80 bg-purple-200/30 dark:bg-purple-800/20 rounded-full blur-3xl" />
                <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-blue-200/30 dark:bg-blue-800/20 rounded-full blur-3xl" />
            </div>

            <div className="container relative mx-auto px-4 py-16 lg:py-24">
                <div className="grid lg:grid-cols-2 gap-12 items-center">
                    {/* Left content */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.6 }}
                        className="space-y-8"
                    >
                        <div className="space-y-4">
                            <Badge variant="secondary" className="px-4 py-1.5 text-sm font-medium">
                                <Flame className="w-4 h-4 mr-2 text-orange-500" />
                                Live Auctions Now
                            </Badge>

                            <h1 className="text-4xl sm:text-5xl lg:text-6xl font-bold tracking-tight">
                                <span className="bg-gradient-to-r from-slate-900 via-purple-900 to-slate-900 dark:from-white dark:via-purple-200 dark:to-white bg-clip-text text-transparent">
                                    Bid. Win. Save.
                                </span>
                                <br />
                                <span className="text-slate-700 dark:text-slate-300">
                                    Join Live Auctions
                                </span>
                                <br />
                                <span className="text-purple-600 dark:text-purple-400">
                                    in Real Time.
                                </span>
                            </h1>

                            <p className="text-lg text-slate-600 dark:text-slate-400 max-w-xl">
                                Exclusive deals up to <span className="font-semibold text-orange-500">70% off</span>.
                                Only verified sellers. Secure payments guaranteed.
                            </p>
                        </div>

                        <div className="flex flex-col sm:flex-row gap-4">
                            <Button
                                size="lg"
                                className="group bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white shadow-lg shadow-purple-500/25 hover:shadow-purple-500/40 transition-all duration-300"
                                asChild
                            >
                                <Link href="/auctions">
                                    Start Bidding
                                    <ArrowRight className="ml-2 w-5 h-5 group-hover:translate-x-1 transition-transform" />
                                </Link>
                            </Button>

                            <Button
                                size="lg"
                                variant="outline"
                                className="group border-2"
                                asChild
                            >
                                <Link href="#how-it-works">
                                    <Play className="mr-2 w-5 h-5" />
                                    How it works
                                </Link>
                            </Button>
                        </div>

                        {/* Stats */}
                        <div className="flex gap-8 pt-4">
                            <div>
                                <div className="text-3xl font-bold text-slate-900 dark:text-white">50K+</div>
                                <div className="text-sm text-slate-600 dark:text-slate-400">Active Bidders</div>
                            </div>
                            <div>
                                <div className="text-3xl font-bold text-slate-900 dark:text-white">12K+</div>
                                <div className="text-sm text-slate-600 dark:text-slate-400">Items Sold</div>
                            </div>
                            <div>
                                <div className="text-3xl font-bold text-slate-900 dark:text-white">98%</div>
                                <div className="text-sm text-slate-600 dark:text-slate-400">Satisfaction</div>
                            </div>
                        </div>
                    </motion.div>

                    {/* Right content - Featured Auction Card */}
                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: 0.6, delay: 0.2 }}
                        className="relative"
                    >
                        <div className="relative bg-white dark:bg-slate-800 rounded-3xl shadow-2xl shadow-purple-500/10 overflow-hidden border border-slate-200 dark:border-slate-700">
                            {/* Live badge */}
                            <div className="absolute top-4 left-4 z-10">
                                <Badge className="bg-red-500 text-white px-3 py-1 animate-pulse">
                                    <span className="w-2 h-2 bg-white rounded-full mr-2 inline-block" />
                                    LIVE
                                </Badge>
                            </div>

                            {/* Image */}
                            <div className="relative h-64 lg:h-80 overflow-hidden">
                                <Image
                                    src={imageUrl}
                                    alt={title}
                                    fill
                                    className="object-cover hover:scale-105 transition-transform duration-500"
                                    unoptimized={imageUrl.includes("unsplash")}
                                />
                                <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent" />
                            </div>

                            {/* Content */}
                            <div className="p-6 space-y-4">
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    {title}
                                </h3>

                                {/* Timer */}
                                <div className="flex items-center gap-2">
                                    <Clock className="w-5 h-5 text-orange-500" />
                                    <span className="text-sm text-slate-600 dark:text-slate-400">Ends in</span>
                                    <motion.div
                                        animate={isUrgent ? { scale: [1, 1.05, 1] } : {}}
                                        transition={{ repeat: Infinity, duration: 0.5 }}
                                        className={`font-mono text-lg font-bold ${isUrgent ? "text-red-500" : "text-slate-900 dark:text-white"
                                            }`}
                                    >
                                        {formatTime(timeLeft.hours)}:{formatTime(timeLeft.minutes)}:{formatTime(timeLeft.seconds)}
                                    </motion.div>
                                </div>

                                {/* Stats row */}
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="text-sm text-slate-600 dark:text-slate-400">Current Bid</div>
                                        <div className="text-2xl font-bold text-purple-600 dark:text-purple-400">
                                            ${currentBid.toLocaleString()}
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <div className="text-sm text-slate-600 dark:text-slate-400">Category</div>
                                        <div className="text-lg font-semibold text-slate-900 dark:text-white">
                                            {heroAuction?.categoryName || "Featured"}
                                        </div>
                                    </div>
                                </div>

                                {/* Bid button */}
                                <Button
                                    className="w-full bg-gradient-to-r from-orange-500 to-red-500 hover:from-orange-600 hover:to-red-600 text-white font-semibold py-6 text-lg group"
                                    asChild
                                >
                                    <Link href={heroAuction ? `/auctions/${heroAuction.id}` : "/auctions"}>
                                        Place Bid Now
                                        <motion.span
                                            animate={{ x: [0, 4, 0] }}
                                            transition={{ repeat: Infinity, duration: 1.5 }}
                                            className="ml-2"
                                        >
                                            →
                                        </motion.span>
                                    </Link>
                                </Button>
                            </div>
                        </div>

                        {/* Floating elements */}
                        {heroAuction?.isFeatured && (
                            <motion.div
                                animate={{ y: [0, -10, 0] }}
                                transition={{ repeat: Infinity, duration: 3 }}
                                className="absolute -top-4 -right-4 bg-green-500 text-white px-4 py-2 rounded-full text-sm font-semibold shadow-lg"
                            >
                                🔥 Featured
                            </motion.div>
                        )}
                    </motion.div>
                </div>
            </div>
        </section>
    );
}
