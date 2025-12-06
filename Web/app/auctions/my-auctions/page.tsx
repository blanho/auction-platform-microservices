'use client';

import { useState, useEffect, useCallback } from 'react';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { Plus, Loader2, LayoutGrid, TableIcon } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { MainLayout } from '@/components/layout/main-layout';
import { AuctionCardWithActions } from '@/features/auction/auction-card-with-actions';
import { AuctionDataTable } from '@/features/auction/auction-data-table';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import {
    ToggleGroup,
    ToggleGroupItem,
} from '@/components/ui/toggle-group';

import { auctionService } from '@/services/auction.service';
import { Auction } from '@/types/auction';

export default function MyAuctionsPage() {
    const { status } = useSession();
    const router = useRouter();
    const [auctions, setAuctions] = useState<Auction[] | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<unknown>(null);
    const [viewMode, setViewMode] = useState<'table' | 'grid'>('table');

    const fetchAuctions = useCallback(async () => {
        if (status !== 'authenticated') return;
        
        setIsLoading(true);
        try {
            // Use the dedicated my-auctions endpoint (authenticated by token)
            const response = await auctionService.getMyAuctions();
            setAuctions(response.items);
            setError(null);
        } catch (err) {
            setError(err);
        } finally {
            setIsLoading(false);
        }
    }, [status]);

    useEffect(() => {
        if (status !== 'authenticated') {
            return;
        }
        fetchAuctions();
    }, [status, fetchAuctions]);

    if (status === 'loading') {
        return (
            <MainLayout>
                <div className="container py-8 flex justify-center">
                    <Loader2 className="h-8 w-8 animate-spin" />
                </div>
            </MainLayout>
        );
    }

    if (status === 'unauthenticated') {
        router.push('/auth/signin?callbackUrl=/auctions/my-auctions');
        return null;
    }

    return (
        <MainLayout>
            <div className="container py-8">
                <Breadcrumb className="mb-6">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/">Home</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href="/auctions">Auctions</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>My Auctions</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <div className="mb-8 flex items-center justify-between">
                    <div>
                        <h1 className="text-3xl font-bold">My Auctions</h1>
                        <p className="text-muted-foreground">
                            Manage your auction listings
                        </p>
                    </div>
                    <div className="flex items-center gap-2">
                        <ToggleGroup 
                            type="single" 
                            value={viewMode} 
                            onValueChange={(value) => value && setViewMode(value as 'table' | 'grid')}
                        >
                            <ToggleGroupItem value="table" aria-label="Table view">
                                <TableIcon className="h-4 w-4" />
                            </ToggleGroupItem>
                            <ToggleGroupItem value="grid" aria-label="Grid view">
                                <LayoutGrid className="h-4 w-4" />
                            </ToggleGroupItem>
                        </ToggleGroup>
                        <Button asChild>
                            <Link href="/auctions/create">
                                <Plus className="mr-2 h-4 w-4" />
                                Create Auction
                            </Link>
                        </Button>
                    </div>
                </div>

                {isLoading && (
                    <div className="flex justify-center py-12">
                        <Loader2 className="h-8 w-8 animate-spin" />
                    </div>
                )}

                {!!error && (
                    <Alert variant="destructive">
                        <AlertDescription>
                            Failed to load your auctions. Please try again.
                        </AlertDescription>
                    </Alert>
                )}

                {!isLoading && !error && auctions && (
                    <>
                        {auctions.length === 0 ? (
                            <Card>
                                <CardHeader>
                                    <CardTitle>No auctions yet</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <p className="text-muted-foreground mb-4">
                                        You haven&apos;t created any auctions yet. Create your first auction to get started!
                                    </p>
                                    <Button asChild>
                                        <Link href="/auctions/create">
                                            <Plus className="mr-2 h-4 w-4" />
                                            Create Your First Auction
                                        </Link>
                                    </Button>
                                </CardContent>
                            </Card>
                        ) : viewMode === 'table' ? (
                            <AuctionDataTable 
                                data={auctions} 
                                onActionComplete={fetchAuctions}
                            />
                        ) : (
                            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                                {auctions.map((auction) => (
                                    <AuctionCardWithActions 
                                        key={auction.id} 
                                        auction={auction}
                                        onActionComplete={fetchAuctions}
                                    />
                                ))}
                            </div>
                        )}
                    </>
                )}
            </div>
        </MainLayout>
    );
}
