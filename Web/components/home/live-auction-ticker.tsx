"use client";

import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBolt, faUsers, faChartLine, faZap } from "@fortawesome/free-solid-svg-icons";
import { useQuickStats } from "@/hooks/use-analytics";
import { LiveIndicator } from "@/components/ui/animated";

interface LiveBid {
    id: string;
    item: string;
    bidder: string;
    amount: number;
    timeAgo: string;
    isNew?: boolean;
}

const SAMPLE_ITEMS = [
    "Rolex Submariner Date",
    "1967 Ford Mustang GT",
    "Banksy Original Print",
    "Charizard PSA 10",
    "Cartier Love Bracelet",
    "MacBook Pro M4 Max",
    "Gibson Les Paul 1959",
    "Hermès Birkin 25",
    "Ferrari 488 Pista",
    "Patek Philippe Nautilus",
    "Air Jordan 1 Chicago",
    "Leica M6 Classic",
];

const SAMPLE_BIDDERS = ["Alex M.", "Sarah K.", "John D.", "Mike T.", "Emma L.", "Chris P.", "David R.", "Anna S."];

function generateRandomBid(): LiveBid {
    const item = SAMPLE_ITEMS[Math.floor(Math.random() * SAMPLE_ITEMS.length)];
    const bidder = SAMPLE_BIDDERS[Math.floor(Math.random() * SAMPLE_BIDDERS.length)];
    const amount = Math.floor(Math.random() * 50000) + 500;
    
    return {
        id: Date.now().toString(),
        item,
        bidder,
        amount,
        timeAgo: "just now",
        isNew: true,
    };
}

function BidItem({ bid }: { bid: LiveBid }) {
    return (
        <div
            className={`shrink-0 flex items-center gap-3 px-4 py-2.5 rounded-full transition-all duration-300 ${
                bid.isNew
                    ? "bg-gradient-to-r from-green-500/20 to-emerald-500/20 border border-green-500/30"
                    : "bg-white/10 border border-white/10"
            }`}
        >
            {bid.isNew && <FontAwesomeIcon icon={faZap} className="w-4 h-4 text-green-400" />}
            <span className="text-sm font-medium text-white truncate max-w-[160px]">
                {bid.item}
            </span>
            <Badge className={bid.isNew ? "bg-green-500 text-white border-0" : "bg-purple-500/80 text-white border-0"}>
                ${bid.amount.toLocaleString()}
            </Badge>
            <span className="text-xs text-white/60">{bid.bidder}</span>
        </div>
    );
}

export function LiveAuctionTicker() {
    const { data: stats, isLoading } = useQuickStats();
    const [bids, setBids] = useState<LiveBid[]>([]);

    useEffect(() => {
        const initialBids: LiveBid[] = Array.from({ length: 8 }, (_, i) => ({
            ...generateRandomBid(),
            id: `initial-${i}`,
            isNew: false,
            timeAgo: `${(i + 1) * 3}s ago`,
        }));
        setBids(initialBids);
    }, []);

    useEffect(() => {
        const interval = setInterval(() => {
            const newBid = generateRandomBid();
            setBids((prev) => [newBid, ...prev.slice(0, 7)].map((bid, idx) => ({ ...bid, isNew: idx === 0 })));
        }, 4000);

        return () => clearInterval(interval);
    }, []);

    return (
        <section className="relative py-4 bg-slate-900 overflow-hidden border-y border-slate-800">
            <div className="container mx-auto px-4">
                <div className="flex items-center gap-6">
                    <div className="shrink-0 hidden md:flex items-center gap-6">
                        <div className="flex items-center gap-2 px-4 py-2 rounded-full bg-red-500/20 border border-red-500/30">
                            <LiveIndicator />
                            <span className="text-sm font-bold text-white uppercase tracking-wider">Live Feed</span>
                        </div>

                        <div className="flex items-center gap-6 text-white/80">
                            {isLoading ? (
                                <>
                                    <Skeleton className="w-24 h-5 bg-white/10" />
                                    <Skeleton className="w-24 h-5 bg-white/10" />
                                </>
                            ) : (
                                <>
                                    <div className="flex items-center gap-2">
                                        <FontAwesomeIcon icon={faChartLine} className="w-4 h-4 text-green-400" />
                                        <span className="text-sm">
                                            <span className="font-bold text-green-400">{(stats?.liveAuctions || 0).toLocaleString()}</span>
                                            <span className="text-white/60 ml-1">live auctions</span>
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <FontAwesomeIcon icon={faUsers} className="w-4 h-4 text-blue-400" />
                                        <span className="text-sm">
                                            <span className="font-bold text-blue-400">{(stats?.activeUsers || 0).toLocaleString()}</span>
                                            <span className="text-white/60 ml-1">online</span>
                                        </span>
                                    </div>
                                </>
                            )}
                        </div>
                    </div>

                    <div className="flex-1 overflow-hidden">
                        <div className="flex gap-3 animate-marquee">
                            {bids.map((bid) => (
                                <BidItem key={bid.id} bid={bid} />
                            ))}
                            {bids.map((bid) => (
                                <BidItem key={`${bid.id}-dup`} bid={{ ...bid, isNew: false }} />
                            ))}
                        </div>
                    </div>
                </div>
            </div>

            <div className="absolute left-0 top-0 bottom-0 w-24 bg-gradient-to-r from-slate-900 to-transparent z-10 pointer-events-none" />
            <div className="absolute right-0 top-0 bottom-0 w-24 bg-gradient-to-l from-slate-900 to-transparent z-10 pointer-events-none" />
        </section>
    );
}
