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

export function AuctionList() {
    const { data, isLoading, error } = useAuctions();

    return (
        <div className="space-y-6">
            <div className="space-y-4">
                <AuctionFilters />
                <div className="flex items-center justify-between">
                    <AuctionSort />
                    {data && (
                        <span className="text-sm text-muted-foreground">
                            {data.totalCount} auction{data.totalCount !== 1 ? 's' : ''} found
                        </span>
                    )}
                </div>
            </div>

            {isLoading && (
                <div className="flex items-center justify-center py-12">
                    <LoadingSpinner size="lg" />
                </div>
            )}

            {error && (
                <EmptyState
                    title="Error loading auctions"
                    description="Something went wrong. Please try again later."
                    action={
                        <Button onClick={() => window.location.reload()}>Retry</Button>
                    }
                />
            )}

            {!isLoading && !error && (!data?.items || data.items.length === 0) && (
                <EmptyState
                    title="No auctions found"
                    description="Try adjusting your filters or be the first to create an auction!"
                    action={
                        <Button asChild>
                            <Link href="/auctions/create">Create Auction</Link>
                        </Button>
                    }
                />
            )}

            {!isLoading && !error && data?.items && data.items.length > 0 && (
                <>
                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                        {data.items.map((auction) => (
                            <AuctionCard key={auction.id} auction={auction} />
                        ))}
                    </div>

                    <AuctionPagination />
                </>
            )}
        </div>
    );
}
