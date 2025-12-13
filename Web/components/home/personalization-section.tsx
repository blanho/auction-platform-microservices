"use client";

import Image from "next/image";
import { motion } from "framer-motion";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Sparkles, Clock, Eye, ArrowRight, Heart } from "lucide-react";
import Link from "next/link";
import { useSession } from "next-auth/react";

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
                            <Clock className="w-3 h-3" />
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
            <section className="py-20 bg-white dark:bg-slate-950">
                <div className="container mx-auto px-4">
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        className="text-center max-w-2xl mx-auto"
                    >
                        <Sparkles className="w-12 h-12 text-purple-500 mx-auto mb-6" />
                        <h2 className="text-3xl md:text-4xl font-bold text-slate-900 dark:text-white mb-4">
                            Get Personalized Recommendations
                        </h2>
                        <p className="text-slate-600 dark:text-slate-400 mb-8">
                            Sign in to see auctions tailored to your interests, track your watchlist,
                            and never miss a deal on items you love.
                        </p>
                        <Button
                            size="lg"
                            className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700 text-white"
                            asChild
                        >
                            <Link href="/auth/signin">
                                Sign In for Personalized Experience
                                <ArrowRight className="ml-2 w-5 h-5" />
                            </Link>
                        </Button>
                    </motion.div>
                </div>
            </section>
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
                                <Sparkles className="w-6 h-6 text-purple-500" />
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    Recommended for You
                                </h3>
                            </div>
                            <Button variant="ghost" size="sm" asChild>
                                <Link href="/auctions?recommended=true">
                                    View All
                                    <ArrowRight className="ml-2 w-4 h-4" />
                                </Link>
                            </Button>
                        </div>
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {recommendedAuctions.map((item) => (
                                <SmallAuctionCard key={item.id} item={item} showBadge="For You" />
                            ))}
                            <Card className="flex flex-col items-center justify-center p-6 border-dashed border-2 hover:border-purple-500 transition-colors cursor-pointer">
                                <Sparkles className="w-8 h-8 text-slate-400 mb-2" />
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
                                <Eye className="w-6 h-6 text-blue-500" />
                                <h3 className="text-xl font-bold text-slate-900 dark:text-white">
                                    Recently Viewed
                                </h3>
                            </div>
                            <Button variant="ghost" size="sm" asChild>
                                <Link href="/auctions?viewed=true">
                                    View History
                                    <ArrowRight className="ml-2 w-4 h-4" />
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
                                <Heart className="w-6 h-6 text-red-500" />
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
                                    <ArrowRight className="ml-2 w-4 h-4" />
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
                                <Heart className="w-12 h-12 text-slate-300 dark:text-slate-600 mx-auto mb-4" />
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
