"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faStar, faArrowRight } from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { useFeaturedAuctionsQuery } from "@/hooks/queries";
import { AuctionCard, AuctionCardSkeleton } from "@/features/auction";
import { UI, FEATURED } from "@/constants/config";

export function FeaturedAuctionsSection() {
    const { data: featuredAuctions, isLoading, error } = useFeaturedAuctionsQuery(FEATURED.DEFAULT_LIMIT);

    if (isLoading) {
        return (
            <section className="relative py-24 bg-slate-50 dark:bg-slate-950 overflow-hidden">
                <div className="container mx-auto px-4">
                    <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
                        {[...Array(FEATURED.DEFAULT_LIMIT)].map((_, i) => (
                            <AuctionCardSkeleton key={i} variant="featured" />
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
            <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_bottom,var(--tw-gradient-stops))] from-purple-200/30 via-transparent to-transparent dark:from-purple-900/10" />
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
                            <span className="block text-transparent bg-clip-text bg-linear-to-r from-purple-600 via-pink-600 to-purple-600 dark:from-purple-400 dark:via-pink-400 dark:to-purple-400">
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
                            className="h-12 px-6 rounded-full bg-linear-to-r from-purple-600 to-pink-600 hover:from-purple-700 hover:to-pink-700 text-white font-medium shadow-lg shadow-purple-500/25"
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
                        <AuctionCard
                            key={auction.id}
                            auction={auction}
                            variant="featured"
                            index={index}
                        />
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
