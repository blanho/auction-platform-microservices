'use client';

import { useState, useEffect } from 'react';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';
import { Plus, Loader2 } from 'lucide-react';
import Link from 'next/link';

import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { MainLayout } from '@/components/layout/main-layout';
import { AuctionCard } from '@/features/auction/auction-card';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';

import apiClient from '@/lib/api/axios';
import { Auction } from '@/types/auction';
import { ApiResponse } from '@/types';
import { API_ENDPOINTS } from '@/constants/api';

export default function MyAuctionsPage() {
    const { data: session, status } = useSession();
    const router = useRouter();
    const [auctions, setAuctions] = useState<Auction[] | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<unknown>(null);

    useEffect(() => {
        if (status !== 'authenticated' || !session?.user?.name) {
            return;
        }

        let isMounted = true;

        const fetchAuctions = async () => {
            try {
                const { data } = await apiClient.get<ApiResponse<Auction[]>>(
                    `${API_ENDPOINTS.AUCTIONS}?seller=${session?.user?.name}`
                );
                if (isMounted) {
                    setAuctions(data.data);
                    setError(null);
                }
            } catch (err) {
                if (isMounted) {
                    setError(err);
                }
            } finally {
                if (isMounted) {
                    setIsLoading(false);
                }
            }
        };

        setIsLoading(true);
        fetchAuctions();

        return () => {
            isMounted = false;
        };
    }, [status, session?.user?.name]);

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
                    <Button asChild>
                        <Link href="/auctions/create">
                            <Plus className="mr-2 h-4 w-4" />
                            Create Auction
                        </Link>
                    </Button>
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
                        ) : (
                            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                                {auctions.map((auction) => (
                                    <AuctionCard key={auction.id} auction={auction} />
                                ))}
                            </div>
                        )}
                    </>
                )}
            </div>
        </MainLayout>
    );
}
