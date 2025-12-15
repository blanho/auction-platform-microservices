'use client';

import Link from 'next/link';
import Image from 'next/image';
import { motion } from 'framer-motion';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faHeart,
    faClock,
    faGavel,
    faFire,
    faTag,
    faBolt,
    faEye,
    faCircleCheck,
    faTruck,
} from '@fortawesome/free-solid-svg-icons';
import { Auction, AuctionStatus, ITEM_CONDITION_LABELS, ItemCondition, ShippingType } from '@/types/auction';
import { SearchItem } from '@/types/search';
import { formatDistanceToNow, differenceInHours, differenceInMinutes } from 'date-fns';
import { cn } from '@/lib/utils';
import { useState, useCallback } from 'react';
import { URGENCY } from '@/constants/config';

interface AuctionCardProps {
    auction: Auction | SearchItem;
    variant?: 'default' | 'compact' | 'featured';
}

export function AuctionCard({ auction, variant = 'default' }: AuctionCardProps) {
    const [isWishlisted, setIsWishlisted] = useState(false);

    const getStatusConfig = (status: string) => {
        const statusUpper = status.toUpperCase();
        switch (statusUpper) {
            case 'LIVE':
            case AuctionStatus.Live.toUpperCase():
                return {
                    bg: 'bg-gradient-to-r from-green-500 to-emerald-500',
                    text: 'Live',
                    icon: faBolt,
                    pulse: true,
                };
            case 'FINISHED':
            case AuctionStatus.Finished.toUpperCase():
                return {
                    bg: 'bg-slate-500',
                    text: 'Ended',
                    icon: faCircleCheck,
                    pulse: false,
                };
            case 'RESERVENOTMET':
            case 'RESERVE_NOT_MET':
            case AuctionStatus.ReserveNotMet.toUpperCase():
                return {
                    bg: 'bg-gradient-to-r from-amber-500 to-yellow-500',
                    text: 'Reserve Not Met',
                    icon: faTag,
                    pulse: false,
                };
            case 'CANCELLED':
            case AuctionStatus.Cancelled.toUpperCase():
                return {
                    bg: 'bg-red-500',
                    text: 'Cancelled',
                    icon: null,
                    pulse: false,
                };
            default:
                return {
                    bg: 'bg-slate-500',
                    text: status,
                    icon: null,
                    pulse: false,
                };
        }
    };

    const getTimeInfo = () => {
        if (!('auctionEnd' in auction) || !auction.auctionEnd) return null;
        const endDate = new Date(auction.auctionEnd);
        if (isNaN(endDate.getTime())) return null;

        const now = new Date();
        const hoursRemaining = differenceInHours(endDate, now);
        const minutesRemaining = differenceInMinutes(endDate, now);

        if (minutesRemaining <= 0) {
            return { text: 'Ended', urgent: false };
        }

        if (minutesRemaining < 60) {
            return { text: `${minutesRemaining}m left`, urgent: true };
        }

        if (hoursRemaining < 24) {
            return { text: `${hoursRemaining}h left`, urgent: hoursRemaining < URGENCY.WARNING_HOURS / 4 };
        }

        return {
            text: formatDistanceToNow(endDate, { addSuffix: false }) + ' left',
            urgent: false,
        };
    };

    const getImageUrl = () => {
        if ('imageUrl' in auction && auction.imageUrl) {
            return auction.imageUrl;
        }
        if ('files' in auction && auction.files) {
            const primaryFile = auction.files.find(f => f.isPrimary);
            return primaryFile?.url || auction.files[0]?.url;
        }
        return undefined;
    };

    const imageUrl = getImageUrl();
    const timeInfo = getTimeInfo();
    const statusConfig = getStatusConfig(auction.status);
    const currentBid = 'currentHighBid' in auction ? auction.currentHighBid : ('price' in auction ? auction.price : 0);
    const hasReserve = 'reservePrice' in auction && auction.reservePrice && auction.reservePrice > 0;
    const hasBuyNow = 'buyNowPrice' in auction && auction.buyNowPrice && auction.buyNowPrice > 0;
    const condition = 'condition' in auction && auction.condition ? auction.condition : null;
    const shippingType = 'shippingType' in auction ? auction.shippingType : null;

    const handleWishlistToggle = useCallback((e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setIsWishlisted(prev => !prev);
    }, []);

    if (variant === 'compact') {
        return (
            <Link href={`/auctions/${auction.id}`}>
                <motion.div
                    whileHover={{ scale: 1.02 }}
                    className="flex gap-3 p-3 bg-white dark:bg-slate-900 rounded-xl border border-slate-200/80 dark:border-slate-800/80 hover:shadow-md transition-all"
                >
                    <div className="relative w-20 h-20 rounded-lg overflow-hidden flex-shrink-0">
                        {imageUrl ? (
                            <Image src={imageUrl} alt={auction.title || 'Auction'} fill className="object-cover" />
                        ) : (
                            <div className="w-full h-full bg-slate-100 dark:bg-slate-800 flex items-center justify-center">
                                <FontAwesomeIcon icon={faGavel} className="w-6 h-6 text-slate-400" />
                            </div>
                        )}
                    </div>
                    <div className="flex-1 min-w-0">
                        <h3 className="font-medium text-sm line-clamp-1 text-slate-900 dark:text-white">
                            {auction.title || 'Untitled'}
                        </h3>
                        <p className="text-lg font-bold text-purple-600 dark:text-purple-400">
                            ${(currentBid || 0).toLocaleString()}
                        </p>
                        {timeInfo && (
                            <p className={cn(
                                "text-xs",
                                timeInfo.urgent ? "text-red-500 font-medium" : "text-slate-500"
                            )}>
                                {timeInfo.text}
                            </p>
                        )}
                    </div>
                </motion.div>
            </Link>
        );
    }

    return (
        <Link href={`/auctions/${auction.id}`} className="block">
            <motion.div
                whileHover={{ y: -4 }}
                transition={{ duration: 0.2 }}
                className="group relative bg-white dark:bg-slate-900 rounded-2xl border border-slate-200/80 dark:border-slate-800/80 overflow-hidden shadow-sm hover:shadow-xl transition-all duration-300"
            >
                <div className="relative aspect-[4/3] overflow-hidden bg-slate-100 dark:bg-slate-800">
                    {imageUrl ? (
                        <Image
                            src={imageUrl}
                            alt={auction.title || 'Auction item'}
                            fill
                            className="object-cover transition-transform duration-500 group-hover:scale-110"
                        />
                    ) : (
                        <div className="flex h-full items-center justify-center">
                            <FontAwesomeIcon icon={faGavel} className="w-12 h-12 text-slate-300 dark:text-slate-600" />
                        </div>
                    )}

                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300" />

                    <div className="absolute top-3 left-3 flex flex-wrap gap-2">
                        <span className={cn(
                            "inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-bold text-white shadow-lg",
                            statusConfig.bg,
                            statusConfig.pulse && "animate-pulse"
                        )}>
                            {statusConfig.icon && (
                                <FontAwesomeIcon icon={statusConfig.icon} className="w-3 h-3" />
                            )}
                            {statusConfig.text}
                        </span>

                        {('isFeatured' in auction && auction.isFeatured) && (
                            <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-bold bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-lg">
                                <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                                Hot
                            </span>
                        )}
                    </div>

                    <motion.button
                        whileHover={{ scale: 1.1 }}
                        whileTap={{ scale: 0.9 }}
                        onClick={handleWishlistToggle}
                        className={cn(
                            "absolute top-3 right-3 w-9 h-9 rounded-full flex items-center justify-center transition-all shadow-lg",
                            isWishlisted
                                ? "bg-red-500 text-white"
                                : "bg-white/90 dark:bg-slate-800/90 text-slate-400 dark:text-slate-500 hover:text-red-500 hover:bg-white dark:hover:bg-slate-800"
                        )}
                    >
                        <FontAwesomeIcon
                            icon={faHeart}
                            className={cn("w-4 h-4", isWishlisted && "animate-pulse")}
                        />
                    </motion.button>

                    {timeInfo && (
                        <div className={cn(
                            "absolute bottom-3 left-3 inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-semibold backdrop-blur-md shadow-lg",
                            timeInfo.urgent
                                ? "bg-red-500/90 text-white"
                                : "bg-white/90 dark:bg-slate-800/90 text-slate-700 dark:text-slate-200"
                        )}>
                            <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                            {timeInfo.text}
                        </div>
                    )}

                    <div className="absolute bottom-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg text-xs font-medium bg-white/90 dark:bg-slate-800/90 text-slate-700 dark:text-slate-200 backdrop-blur-md shadow-lg">
                            <FontAwesomeIcon icon={faEye} className="w-3 h-3" />
                            View
                        </span>
                    </div>
                </div>

                <div className="p-4 space-y-3">
                    <div>
                        <h3 className="font-semibold text-slate-900 dark:text-white line-clamp-1 group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                            {auction.title || 'Untitled'}
                        </h3>
                        {'make' in auction && auction.make && (
                            <p className="text-sm text-slate-500 dark:text-slate-400 line-clamp-1">
                                {auction.year} {auction.make} {auction.model}
                            </p>
                        )}
                    </div>

                    <div className="flex items-end justify-between gap-2">
                        <div>
                            <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">
                                {statusConfig.text === 'Ended' ? 'Final Price' : 'Current Bid'}
                            </p>
                            <p className="text-xl font-bold text-slate-900 dark:text-white">
                                ${(currentBid || 0).toLocaleString()}
                            </p>
                        </div>

                        {hasBuyNow && statusConfig.text !== 'Ended' && (
                            <div className="text-right">
                                <p className="text-xs text-slate-500 dark:text-slate-400 mb-0.5">Buy Now</p>
                                <p className="text-sm font-semibold text-green-600 dark:text-green-400">
                                    ${'buyNowPrice' in auction ? (auction.buyNowPrice || 0).toLocaleString() : '0'}
                                </p>
                            </div>
                        )}
                    </div>

                    <div className="flex items-center gap-2 flex-wrap">
                        {condition && (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400">
                                {ITEM_CONDITION_LABELS[condition as ItemCondition] || condition}
                            </span>
                        )}
                        {shippingType === ShippingType.Free && (
                            <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-xs font-medium bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400">
                                <FontAwesomeIcon icon={faTruck} className="w-3 h-3" />
                                Free Shipping
                            </span>
                        )}
                        {hasReserve && (
                            <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400">
                                Reserve
                            </span>
                        )}
                    </div>
                </div>

                <div className="absolute inset-x-0 bottom-0 h-1 bg-gradient-to-r from-purple-500 to-blue-500 transform scale-x-0 group-hover:scale-x-100 transition-transform duration-300 origin-left" />
            </motion.div>
        </Link>
    );
}
