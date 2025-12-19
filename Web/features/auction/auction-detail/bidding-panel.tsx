"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faGavel,
    faClock,
    faDollarSign,
    faHeart,
    faShare,
    faBolt,
    faTrophy,
} from "@fortawesome/free-solid-svg-icons";
import { faFacebook, faXTwitter } from "@fortawesome/free-brands-svg-icons";
import { faCopy } from "@fortawesome/free-solid-svg-icons";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from "@/components/ui/tooltip";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { PlaceBidDialog } from "@/features/bid/place-bid-dialog";
import { AutoBidDialog } from "@/features/bid/auto-bid-dialog";
import { BuyNowButton } from "@/features/auction/buy-now-button";
import { CountdownTimer } from "./countdown-timer";
import { formatCurrency, showSuccessToast } from "@/utils";
import { AUCTION_BID } from "@/constants";
import { AuctionStatus } from "@/types";

interface BiddingPanelProps {
    auctionId: string;
    currentHighBid: number;
    reservePrice: number;
    auctionEnd: string;
    status: AuctionStatus;
    isOwner: boolean;
    isUrgent: boolean;
    isBuyNowAvailable?: boolean;
    buyNowPrice?: number | null;
    auctionTitle: string;
    winnerUsername?: string | null;
    soldAmount?: number | null;
    isInWatchlist: boolean;
    onToggleWatchlist: () => void;
    onBidPlaced: () => void;
}

export function BiddingPanel({
    auctionId,
    currentHighBid,
    reservePrice,
    auctionEnd,
    status,
    isOwner,
    isUrgent,
    isBuyNowAvailable,
    buyNowPrice,
    auctionTitle,
    winnerUsername,
    soldAmount,
    isInWatchlist,
    onToggleWatchlist,
    onBidPlaced,
}: BiddingPanelProps) {
    const [bidAmount, setBidAmount] = useState("");

    const shareToFacebook = () => {
        window.open(
            `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(window.location.href)}`,
            "_blank"
        );
    };

    const shareToTwitter = () => {
        window.open(
            `https://twitter.com/intent/tweet?url=${encodeURIComponent(window.location.href)}&text=${encodeURIComponent(`Check out this auction: ${auctionTitle}`)}`,
            "_blank"
        );
    };

    const copyLink = () => {
        navigator.clipboard.writeText(window.location.href);
        showSuccessToast("Link copied to clipboard!");
    };

    return (
        <Card className="border-0 shadow-xl bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl overflow-hidden">
            <div className="absolute inset-x-0 top-0 h-1 bg-linear-to-r from-purple-600 to-blue-600" />
            <CardContent className="pt-8">
                <div className="space-y-6">
                    <div>
                        <p className="text-sm text-slate-500 flex items-center gap-2">
                            <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                            Current Price
                        </p>
                        <p className="text-4xl font-bold bg-linear-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mt-1">
                            {formatCurrency(currentHighBid || 0)}
                        </p>
                    </div>

                    <div className="p-4 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                        <div className="flex items-center gap-2 mb-3">
                            <FontAwesomeIcon icon={faClock} className="w-4 h-4 text-slate-400" />
                            <span className="text-sm text-slate-500">Time Remaining</span>
                        </div>
                        <CountdownTimer endDate={auctionEnd} isUrgent={isUrgent} />
                    </div>

                    <div className="flex items-center gap-2 text-sm text-slate-500">
                        <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                        <span>Multiple bids placed</span>
                    </div>

                    <Separator className="bg-slate-200 dark:bg-slate-700" />

                    {status === AuctionStatus.Live && !isOwner && (
                        <div className="space-y-4">
                            <div className="rounded-xl bg-linear-to-br from-purple-50/80 to-blue-50/80 dark:from-purple-950/20 dark:to-blue-950/20 p-4 border border-purple-200/50 dark:border-purple-800/30">
                                <p className="text-xs font-medium text-purple-700 dark:text-purple-400 uppercase tracking-wide mb-3">
                                    Place Your Bid
                                </p>
                                <div className="flex gap-2">
                                    <div className="relative flex-1">
                                        <FontAwesomeIcon
                                            icon={faDollarSign}
                                            className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400"
                                        />
                                        <Input
                                            type="number"
                                            value={bidAmount}
                                            onChange={(e) => setBidAmount(e.target.value)}
                                            className="pl-9 h-12 rounded-xl border-purple-200 dark:border-purple-800/50 focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white dark:bg-slate-900/50"
                                            placeholder="Enter bid amount"
                                        />
                                    </div>
                                    <PlaceBidDialog
                                        auctionId={auctionId}
                                        currentHighBid={currentHighBid || 0}
                                        reservePrice={reservePrice}
                                        auctionTitle={auctionTitle}
                                        onBidPlaced={onBidPlaced}
                                    />
                                </div>
                                <div className="flex items-center justify-between mt-3">
                                    <p className="text-xs text-slate-500">
                                        Min bid: {formatCurrency((currentHighBid || reservePrice) + AUCTION_BID.MIN_INCREMENT)}
                                    </p>
                                    <AutoBidDialog
                                        auctionId={auctionId}
                                        currentHighBid={currentHighBid || 0}
                                        reservePrice={reservePrice}
                                    />
                                </div>
                            </div>
                        </div>
                    )}

                    {isBuyNowAvailable && buyNowPrice && status === AuctionStatus.Live && !isOwner && (
                        <div className="space-y-2">
                            <div className="flex items-center gap-2">
                                <div className="flex-1 h-px bg-slate-200 dark:bg-slate-700" />
                                <span className="text-xs font-medium text-slate-400 uppercase">or</span>
                                <div className="flex-1 h-px bg-slate-200 dark:bg-slate-700" />
                            </div>
                            <div className="rounded-xl bg-slate-50 dark:bg-slate-800/50 p-4 border border-slate-200/50 dark:border-slate-700/50">
                                <div className="flex items-center justify-between">
                                    <div>
                                        <div className="flex items-center gap-2">
                                            <FontAwesomeIcon icon={faBolt} className="w-3.5 h-3.5 text-amber-500" />
                                            <p className="text-xs font-medium text-slate-500 dark:text-slate-400">
                                                Skip bidding
                                            </p>
                                        </div>
                                        <p className="text-xl font-bold text-slate-700 dark:text-slate-200 mt-0.5">
                                            {formatCurrency(buyNowPrice)}
                                        </p>
                                    </div>
                                    <BuyNowButton
                                        auctionId={auctionId}
                                        buyNowPrice={buyNowPrice}
                                        auctionTitle={auctionTitle}
                                        variant="secondary"
                                        onSuccess={() => window.location.reload()}
                                    />
                                </div>
                            </div>
                        </div>
                    )}

                    {winnerUsername && (
                        <motion.div
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            className="rounded-xl bg-linear-to-br from-emerald-50 to-green-50 dark:from-emerald-950/30 dark:to-green-950/30 p-5 border border-emerald-200/50 dark:border-emerald-800/50"
                        >
                            <p className="text-sm font-semibold text-emerald-700 dark:text-emerald-400 flex items-center gap-2">
                                <FontAwesomeIcon icon={faTrophy} className="w-5 h-5 text-amber-500" />
                                Winner: {winnerUsername}
                            </p>
                            {soldAmount && (
                                <p className="text-sm text-emerald-600 dark:text-emerald-500 mt-1">
                                    Sold for: {formatCurrency(soldAmount)}
                                </p>
                            )}
                        </motion.div>
                    )}

                    <Separator className="bg-slate-200 dark:bg-slate-700" />

                    <div className="flex gap-2">
                        <TooltipProvider>
                            <Tooltip>
                                <TooltipTrigger asChild>
                                    <motion.button
                                        whileHover={{ scale: 1.05 }}
                                        whileTap={{ scale: 0.95 }}
                                        onClick={onToggleWatchlist}
                                        className={`w-12 h-12 rounded-xl flex items-center justify-center transition-all ${
                                            isInWatchlist
                                                ? "bg-red-100 dark:bg-red-950/50 text-red-500"
                                                : "bg-slate-100 dark:bg-slate-800 text-slate-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30"
                                        }`}
                                    >
                                        <FontAwesomeIcon
                                            icon={faHeart}
                                            className={`w-5 h-5 ${isInWatchlist ? "text-red-500" : ""}`}
                                        />
                                    </motion.button>
                                </TooltipTrigger>
                                <TooltipContent>
                                    {isInWatchlist ? "Remove from Watchlist" : "Add to Watchlist"}
                                </TooltipContent>
                            </Tooltip>
                        </TooltipProvider>

                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <motion.button
                                    whileHover={{ scale: 1.05 }}
                                    whileTap={{ scale: 0.95 }}
                                    className="w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 transition-all"
                                >
                                    <FontAwesomeIcon icon={faShare} className="w-5 h-5" />
                                </motion.button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent className="rounded-xl">
                                <DropdownMenuItem onClick={shareToFacebook} className="cursor-pointer">
                                    <FontAwesomeIcon icon={faFacebook} className="w-4 h-4 mr-2 text-blue-600" />
                                    Facebook
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={shareToTwitter} className="cursor-pointer">
                                    <FontAwesomeIcon icon={faXTwitter} className="w-4 h-4 mr-2" />
                                    X (Twitter)
                                </DropdownMenuItem>
                                <DropdownMenuItem onClick={copyLink} className="cursor-pointer">
                                    <FontAwesomeIcon icon={faCopy} className="w-4 h-4 mr-2 text-slate-500" />
                                    Copy Link
                                </DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
