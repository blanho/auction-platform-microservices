"use client";

import { useEffect, useState } from "react";
import { Badge } from "@/components/ui/badge";
import { TrendingUp, Users, Zap } from "lucide-react";
import { INITIAL_BIDS, NEW_BIDS_POOL, LIVE_TICKER_CONTENT } from "@/constants/landing";
import { LiveIndicator } from "@/components/ui/animated";

interface LiveBid {
    id: string;
    item: string;
    bidder: string;
    amount: number;
    timeAgo: string;
    isNew?: boolean;
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
            {bid.isNew && <Zap className="w-4 h-4 text-green-400" />}
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
    const [bids, setBids] = useState<LiveBid[]>(INITIAL_BIDS);
    const [stats, setStats] = useState(LIVE_TICKER_CONTENT.INITIAL_STATS);

    useEffect(() => {
        const interval = setInterval(() => {
            const randomBid = NEW_BIDS_POOL[Math.floor(Math.random() * NEW_BIDS_POOL.length)];
            const newBid: LiveBid = {
                ...randomBid,
                id: Date.now().toString(),
                timeAgo: "just now",
                isNew: true,
                amount: randomBid.amount + Math.floor(Math.random() * 500),
            };

            setBids((prev) => [newBid, ...prev.slice(0, 7)].map((bid, idx) => ({ ...bid, isNew: idx === 0 })));
            setStats((prev) => ({
                totalBids: prev.totalBids + 1,
                activeBidders: prev.activeBidders + (Math.random() > 0.7 ? 1 : 0),
            }));
        }, LIVE_TICKER_CONTENT.UPDATE_INTERVAL);

        return () => clearInterval(interval);
    }, []);

    return (
        <section className="relative py-4 bg-slate-900 overflow-hidden border-y border-slate-800">
            <div className="container mx-auto px-4">
                <div className="flex items-center gap-6">
                    <div className="shrink-0 hidden md:flex items-center gap-6">
                        <div className="flex items-center gap-2 px-4 py-2 rounded-full bg-red-500/20 border border-red-500/30">
                            <LiveIndicator />
                            <span className="text-sm font-bold text-white uppercase tracking-wider">{LIVE_TICKER_CONTENT.BADGE}</span>
                        </div>

                        <div className="flex items-center gap-6 text-white/80">
                            <div className="flex items-center gap-2">
                                <TrendingUp className="w-4 h-4 text-green-400" />
                                <span className="text-sm">
                                    <span className="font-bold text-green-400">{stats.totalBids.toLocaleString()}</span>
                                    <span className="text-white/60 ml-1">bids today</span>
                                </span>
                            </div>
                            <div className="flex items-center gap-2">
                                <Users className="w-4 h-4 text-blue-400" />
                                <span className="text-sm">
                                    <span className="font-bold text-blue-400">{stats.activeBidders.toLocaleString()}</span>
                                    <span className="text-white/60 ml-1">online</span>
                                </span>
                            </div>
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
