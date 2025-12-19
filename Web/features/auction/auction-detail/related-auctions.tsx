"use client";

import Link from "next/link";
import Image from "next/image";
import { motion } from "framer-motion";
import { formatDistanceToNow } from "date-fns";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faGavel, faClock } from "@fortawesome/free-solid-svg-icons";
import { Card, CardContent } from "@/components/ui/card";
import { formatCurrency } from "@/utils";
import { Auction } from "@/types";

interface RelatedAuctionsProps {
    auctions: Auction[];
}

export function RelatedAuctions({ auctions }: RelatedAuctionsProps) {
    if (auctions.length === 0) {
        return null;
    }

    return (
        <div className="mt-8">
            <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 rounded-xl bg-linear-to-br from-emerald-500 to-green-600 flex items-center justify-center shadow-lg shadow-emerald-500/25">
                    <FontAwesomeIcon icon={faGavel} className="w-5 h-5 text-white" />
                </div>
                <h2 className="text-2xl font-bold bg-linear-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
                    Related Items
                </h2>
            </div>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                {auctions.map((auction, index) => {
                    const auctionImage =
                        auction.files?.find((f) => f.isPrimary && f.url)?.url || "/placeholder-car.jpg";
                    return (
                        <motion.div
                            key={auction.id}
                            initial={{ opacity: 0, y: 20 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ duration: 0.3, delay: 0.1 * index }}
                        >
                            <Link href={`/auctions/${auction.id}`}>
                                <Card className="overflow-hidden border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl group cursor-pointer hover:shadow-xl transition-all duration-300 hover:-translate-y-1">
                                    <div className="relative aspect-[4/3]">
                                        <Image
                                            src={auctionImage}
                                            alt={auction.title}
                                            fill
                                            className="object-cover transition-transform duration-500 group-hover:scale-105"
                                        />
                                        <div className="absolute inset-0 bg-linear-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                                    </div>
                                    <CardContent className="p-4">
                                        <h3 className="font-semibold truncate text-slate-900 dark:text-white group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                            {auction.title}
                                        </h3>
                                        <p className="text-sm text-slate-500 mt-0.5">
                                            {auction.categoryName || "Uncategorized"}
                                        </p>
                                        <div className="flex items-center justify-between mt-3">
                                            <span className="font-bold bg-linear-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                                                {formatCurrency(auction.currentHighBid || auction.reservePrice)}
                                            </span>
                                            <span className="text-xs text-slate-400 flex items-center gap-1">
                                                <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                                                {formatDistanceToNow(new Date(auction.auctionEnd), {
                                                    addSuffix: true,
                                                })}
                                            </span>
                                        </div>
                                    </CardContent>
                                </Card>
                            </Link>
                        </motion.div>
                    );
                })}
            </div>
        </div>
    );
}
