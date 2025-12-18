"use client";

import { useRef } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faRocket,
    faArrowRight,
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { Button } from "@/components/ui/button";
import { useAuctionsQuery } from "@/hooks/queries";
import { AuctionCard, AuctionCardSkeleton } from "@/features/auction";

export function NewArrivalsSection() {
    const { data, isLoading } = useAuctionsQuery({
        status: "Live",
        orderBy: "createdAt",
        descending: true,
        pageSize: 8,
    });
    const auctions = data?.items ?? [];
    const scrollRef = useRef<HTMLDivElement>(null);

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
                        ? [...Array(4)].map((_, i) => <AuctionCardSkeleton key={i} variant="carousel" />)
                        : auctions.map((auction, index) => (
                              <AuctionCard
                                  key={auction.id}
                                  auction={auction}
                                  variant="carousel"
                                  colorScheme="emerald"
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
