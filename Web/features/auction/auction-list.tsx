'use client';

import { useAuctions } from '@/hooks/use-auctions';
import { AuctionCard } from './auction-card';
import { LoadingSpinner } from '@/components/common/loading-spinner';
import { EmptyState } from '@/components/common/empty-state';
import { Button } from '@/components/ui/button';
import Link from 'next/link';

export function AuctionList() {
    const { data, isLoading, error } = useAuctions({ pageNumber: 1, pageSize: 12 });

    if (isLoading) {
        return (
            <div className="flex items-center justify-center py-12">
                <LoadingSpinner size="lg" />
            </div>
        );
    }

    if (error) {
        return (
            <EmptyState
                title="Error loading auctions"
                description="Something went wrong. Please try again later."
                action={
                    <Button onClick={() => window.location.reload()}>Retry</Button>
                }
            />
        );
    }

    if (!data?.items || data.items.length === 0) {
        return (
            <EmptyState
                title="No auctions found"
                description="Be the first to create an auction!"
                action={
                    <Button asChild>
                        <Link href="/auctions/create">Create Auction</Link>
                    </Button>
                }
            />
        );
    }

    return (
        <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {data.items.map((auction) => (
                <AuctionCard key={auction.id} auction={auction} />
            ))}
        </div>
    );
}
