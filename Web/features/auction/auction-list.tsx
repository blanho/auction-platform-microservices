'use client';

import { useAuctions } from '@/hooks/use-auctions';
import { AuctionCard } from './auction-card';
import { AuctionFilters } from './auction-filters';
import { AuctionSort } from './auction-sort';
import { AuctionPagination } from './auction-pagination';
import { LoadingSpinner } from '@/components/common/loading-spinner';
import { EmptyState } from '@/components/common/empty-state';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { motion, AnimatePresence } from 'framer-motion';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faGavel, faPlus, faListUl, faGrip, faRefresh } from '@fortawesome/free-solid-svg-icons';
import { useState } from 'react';
import { cn } from '@/lib/utils';

export function AuctionList() {
    const { data, isLoading, error } = useAuctions();
    const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

    const containerVariants = {
        hidden: { opacity: 0 },
        visible: {
            opacity: 1,
            transition: {
                staggerChildren: 0.05,
            },
        },
    };

    const itemVariants = {
        hidden: { opacity: 0, y: 20 },
        visible: { opacity: 1, y: 0 },
    };

    return (
        <div className="space-y-6">
            <div className="space-y-4">
                <AuctionFilters />

                <div className="flex items-center justify-between gap-4 flex-wrap">
                    <div className="flex items-center gap-3">
                        <AuctionSort />

                        <div className="hidden sm:flex items-center bg-white dark:bg-slate-900 rounded-xl border border-slate-200 dark:border-slate-700 p-1 h-11">
                            <button
                                onClick={() => setViewMode('grid')}
                                className={cn(
                                    "p-2 rounded-lg transition-all",
                                    viewMode === 'grid'
                                        ? "bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400"
                                        : "text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                                )}
                            >
                                <FontAwesomeIcon icon={faGrip} className="w-4 h-4" />
                            </button>
                            <button
                                onClick={() => setViewMode('list')}
                                className={cn(
                                    "p-2 rounded-lg transition-all",
                                    viewMode === 'list'
                                        ? "bg-purple-100 dark:bg-purple-900/30 text-purple-600 dark:text-purple-400"
                                        : "text-slate-400 hover:text-slate-600 dark:hover:text-slate-300"
                                )}
                            >
                                <FontAwesomeIcon icon={faListUl} className="w-4 h-4" />
                            </button>
                        </div>
                    </div>

                    {data && (
                        <span className="text-sm text-slate-500 dark:text-slate-400">
                            <span className="font-semibold text-purple-600 dark:text-purple-400">{data.totalCount}</span>
                            {' '}auction{data.totalCount !== 1 ? 's' : ''} found
                        </span>
                    )}
                </div>
            </div>

            <AnimatePresence mode="wait">
                {isLoading && (
                    <motion.div
                        key="loading"
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="flex flex-col items-center justify-center py-16 gap-4"
                    >
                        <div className="relative">
                            <div className="w-16 h-16 rounded-full bg-gradient-to-r from-purple-500 to-blue-500 animate-spin" style={{ animationDuration: '1.5s' }}>
                                <div className="absolute inset-1 bg-white dark:bg-slate-950 rounded-full" />
                            </div>
                            <div className="absolute inset-0 flex items-center justify-center">
                                <FontAwesomeIcon icon={faGavel} className="w-6 h-6 text-purple-600" />
                            </div>
                        </div>
                        <p className="text-slate-500 dark:text-slate-400 text-sm">Loading auctions...</p>
                    </motion.div>
                )}

                {error && (
                    <motion.div
                        key="error"
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }}
                    >
                        <div className="flex flex-col items-center justify-center py-16 gap-4">
                            <div className="w-20 h-20 rounded-full bg-red-100 dark:bg-red-900/30 flex items-center justify-center">
                                <FontAwesomeIcon icon={faGavel} className="w-8 h-8 text-red-500" />
                            </div>
                            <div className="text-center">
                                <h3 className="text-lg font-semibold text-slate-900 dark:text-white mb-1">
                                    Error loading auctions
                                </h3>
                                <p className="text-slate-500 dark:text-slate-400 text-sm mb-4">
                                    Something went wrong. Please try again later.
                                </p>
                                <Button onClick={() => window.location.reload()} className="gap-2">
                                    <FontAwesomeIcon icon={faRefresh} className="w-4 h-4" />
                                    Retry
                                </Button>
                            </div>
                        </div>
                    </motion.div>
                )}

                {!isLoading && !error && (!data?.items || data.items.length === 0) && (
                    <motion.div
                        key="empty"
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0 }}
                    >
                        <div className="flex flex-col items-center justify-center py-16 gap-4">
                            <div className="w-24 h-24 rounded-full bg-gradient-to-br from-purple-100 to-blue-100 dark:from-purple-900/30 dark:to-blue-900/30 flex items-center justify-center">
                                <FontAwesomeIcon icon={faGavel} className="w-10 h-10 text-purple-500" />
                            </div>
                            <div className="text-center max-w-sm">
                                <h3 className="text-xl font-semibold text-slate-900 dark:text-white mb-2">
                                    No auctions found
                                </h3>
                                <p className="text-slate-500 dark:text-slate-400 text-sm mb-6">
                                    Try adjusting your filters or be the first to create an auction!
                                </p>
                                <Button asChild className="gap-2 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
                                    <Link href="/auctions/create">
                                        <FontAwesomeIcon icon={faPlus} className="w-4 h-4" />
                                        Create Auction
                                    </Link>
                                </Button>
                            </div>
                        </div>
                    </motion.div>
                )}

                {!isLoading && !error && data?.items && data.items.length > 0 && (
                    <motion.div
                        key="content"
                        variants={containerVariants}
                        initial="hidden"
                        animate="visible"
                    >
                        <div className={cn(
                            viewMode === 'grid'
                                ? "grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
                                : "flex flex-col gap-4"
                        )}>
                            {data.items.map((auction) => (
                                <motion.div key={auction.id} variants={itemVariants}>
                                    <AuctionCard
                                        auction={auction}
                                        variant={viewMode === 'list' ? 'compact' : 'default'}
                                    />
                                </motion.div>
                            ))}
                        </div>

                        <div className="mt-8">
                            <AuctionPagination />
                        </div>
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
}
