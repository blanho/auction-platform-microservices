"use client";

import Image from "next/image";
import { motion } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faWandSparkles, faClock, faEye, faArrowRight, faHeart } from "@fortawesome/free-solid-svg-icons";
import Link from "next/link";
import { useSession } from "next-auth/react";
import { CTA_CONTENT } from "@/constants/landing";
import { AnimatedSection } from "@/components/ui/animated";

const recommendedAuctions = [
    {
        id: "r1",
        title: "Similar to your recent view",
        imageUrl: "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=300",
        currentBid: 450,
    },
    {
        id: "r2",
        title: "Based on your interests",
        imageUrl: "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=300",
        currentBid: 890,
    },
    {
        id: "r3",
        title: "You might like this",
        imageUrl: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=300",
        currentBid: 320,
    },
];

const recentlyViewed = [
    {
        id: "v1",
        title: "Vintage Camera",
        imageUrl: "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=300",
        currentBid: 280,
        timeLeft: "2h 15m",
    },
    {
        id: "v2",
        title: "Designer Watch",
        imageUrl: "https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=300",
        currentBid: 1200,
        timeLeft: "5h 30m",
    },
];

const watchlistItems = [
    {
        id: "w1",
        title: "Art Deco Lamp",
        imageUrl: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300",
        currentBid: 650,
        hasNewBid: true,
    },
];

function SmallAuctionCard({
    item,
    showBadge
}: {
    item: { id: string; title: string; imageUrl: string; currentBid: number; timeLeft?: string; hasNewBid?: boolean };
    showBadge?: string;
}) {
    return (
        <Card className="group overflow-hidden hover:shadow-lg transition-all duration-300 cursor-pointer">
            <div className="relative h-32 overflow-hidden">
                <Image
                    src={item.imageUrl}
                    alt={item.title}
                    fill
                    className="object-cover group-hover:scale-110 transition-transform duration-300"
                    sizes="(max-width: 768px) 100vw, 33vw"
                />
                {showBadge && (
                    <Badge className="absolute top-2 left-2 bg-purple-500 text-white text-xs">
                        {showBadge}
                    </Badge>
                )}
                {item.hasNewBid && (
                    <Badge className="absolute top-2 right-2 bg-orange-500 text-white text-xs animate-pulse">
                        New Bid!
                    </Badge>
                )}
            </div>
            <CardContent className="p-3">
                <h4 className="text-sm font-medium text-slate-900 dark:text-white truncate">
                    {item.title}
                </h4>
                <div className="flex items-center justify-between mt-1">
                    <span className="text-sm font-bold text-purple-600 dark:text-purple-400">
                        ${item.currentBid.toLocaleString()}
                    </span>
                    {item.timeLeft && (
                        <span className="text-xs text-slate-500 flex items-center gap-1">
                            <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                            {item.timeLeft}
                        </span>
                    )}
                </div>
            </CardContent>
        </Card>
    );
}

export function PersonalizationSection() {
    const { data: session } = useSession();

    if (!session) {
        return (
            <AnimatedSection className="py-24 bg-slate-950 relative overflow-hidden">
                <div className="absolute inset-0">
                    <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-purple-900/50 via-slate-950 to-slate-950" />
                    <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-purple-600/20 rounded-full blur-[150px]" />
                </div>
                
                <div className="container mx-auto px-4 relative z-10">
                    <div className="max-w-4xl mx-auto text-center">
                        <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-white/5 border border-white/10 backdrop-blur-sm mb-8">
                            <FontAwesomeIcon icon={faWandSparkles} className="w-4 h-4 text-purple-400" />
                            <span className="text-sm font-medium text-purple-300">{CTA_CONTENT.BADGE}</span>
                        </div>
                        
                        <h2 className="text-4xl md:text-5xl lg:text-6xl font-bold text-white mb-6 leading-tight">
                            {CTA_CONTENT.TITLE.split(".")[0]}.<br />
                            <span className="text-transparent bg-clip-text bg-gradient-to-r from-purple-400 via-pink-400 to-orange-400">
                                {CTA_CONTENT.TITLE.split(".")[1]?.trim() || "Start Winning."}
                            </span>
                        </h2>
                        <p className="text-lg md:text-xl text-slate-400 mb-10 max-w-2xl mx-auto leading-relaxed">
                            {CTA_CONTENT.DESCRIPTION}
                        </p>
                        
                        <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-10">
                            <Button
                                size="lg"
                                className="bg-white text-slate-900 hover:bg-slate-100 font-semibold px-10 h-14 text-lg rounded-xl shadow-xl shadow-white/10 transition-all"
                                asChild
                            >
                                <Link href="/auth/register">
                                    {CTA_CONTENT.BUTTON}
                                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-5 h-5" />
                                </Link>
                            </Button>
                            <Button
                                size="lg"
                                variant="outline"
                                className="border-2 border-white/20 text-white hover:bg-white/10 font-semibold px-10 h-14 text-lg rounded-xl backdrop-blur-sm"
                                asChild
                            >
                                <Link href="/auctions">
                                    Browse Auctions
                                </Link>
                            </Button>
                        </div>
                        
                        <div className="flex flex-wrap items-center justify-center gap-x-8 gap-y-4 text-slate-400">
                            {CTA_CONTENT.FEATURES.map((feature, idx) => (
                                <div key={idx} className="flex items-center gap-2">
                                    <div className="w-1.5 h-1.5 rounded-full bg-green-500"></div>
                                    <span className="text-sm">{feature}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
            </AnimatedSection>
        );
    }

    return (
        <section className="py-20 bg-white dark:bg-slate-950">
            <div className="container mx-auto px-4">
                <div className="space-y-12">
                    {/* Recommended for you */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                    >
                        <div className="flex items-center justify-between mb-6">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faWandSparkles} className="w-6 h-6 text-purple-500" />
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    Recommended for You
                                </h3>
                            </div>
                            <Button variant="ghost" size="sm" asChild>
                                <Link href="/auctions?recommended=true">
                                    View All
                                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                                </Link>
                            </Button>
                        </div>
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {recommendedAuctions.map((item) => (
                                <SmallAuctionCard key={item.id} item={item} showBadge="For You" />
                            ))}
                            <Card className="flex flex-col items-center justify-center p-6 border-dashed border-2 hover:border-purple-500 transition-colors cursor-pointer">
                                <FontAwesomeIcon icon={faWandSparkles} className="w-8 h-8 text-slate-400 mb-2" />
                                <span className="text-sm text-slate-500 text-center">
                                    Explore more recommendations
                                </span>
                            </Card>
                        </div>
                    </motion.div>

                    {/* Recently Viewed */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: 0.1 }}
                    >
                        <div className="flex items-center justify-between mb-6">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faEye} className="w-6 h-6 text-blue-500" />
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    Recently Viewed
                                </h3>
                            </div>
                            <Button variant="ghost" size="sm" asChild>
                                <Link href="/auctions?viewed=true">
                                    View History
                                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                                </Link>
                            </Button>
                        </div>
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {recentlyViewed.map((item) => (
                                <SmallAuctionCard key={item.id} item={item} />
                            ))}
                        </div>
                    </motion.div>

                    {/* Watchlist */}
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay: 0.2 }}
                    >
                        <div className="flex items-center justify-between mb-6">
                            <div className="flex items-center gap-3">
                                <FontAwesomeIcon icon={faHeart} className="w-6 h-6 text-red-500" />
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    Your Watchlist
                                </h3>
                                {watchlistItems.length > 0 && (
                                    <Badge variant="secondary">
                                        {watchlistItems.length} items
                                    </Badge>
                                )}
                            </div>
                            <Button variant="ghost" size="sm" asChild>
                                <Link href="/auctions?watchlist=true">
                                    Manage Watchlist
                                    <FontAwesomeIcon icon={faArrowRight} className="ml-2 w-4 h-4" />
                                </Link>
                            </Button>
                        </div>

                        {watchlistItems.length > 0 ? (
                            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                                {watchlistItems.map((item) => (
                                    <SmallAuctionCard key={item.id} item={item} />
                                ))}
                            </div>
                        ) : (
                            <Card className="p-8 text-center border-dashed border-2">
                                <FontAwesomeIcon icon={faHeart} className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
                                <h4 className="text-lg font-semibold text-slate-900 dark:text-white mb-2">
                                    Your watchlist is empty
                                </h4>
                                <p className="text-slate-500 dark:text-slate-400 mb-4">
                                    Save items you&apos;re interested in to track their progress
                                </p>
                                <Button variant="outline" asChild>
                                    <Link href="/auctions">
                                        Browse Auctions
                                    </Link>
                                </Button>
                            </Card>
                        )}
                    </motion.div>
                </div>
            </div>
        </section>
    );
}
