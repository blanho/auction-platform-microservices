'use client';

import { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { Loader2 } from 'lucide-react';
import { Alert, AlertDescription } from '@/components/ui/alert';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import { MainLayout } from '@/components/layout/main-layout';
import { EditAuctionForm } from '@/features/auction/edit-auction-form';
import apiClient from '@/lib/api/axios';
import { Auction } from '@/types/auction';
import { ApiResponse } from '@/types';
import { API_ENDPOINTS } from '@/constants/api';

export default function EditAuctionPage() {
    const params = useParams();
    const auctionId = params?.id as string;
    const [auction, setAuction] = useState<Auction | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<unknown>(null);

    useEffect(() => {
        if (!auctionId) {
            setIsLoading(false);
            return;
        }

        let isMounted = true;

        const fetchAuction = async () => {
            try {
                const { data } = await apiClient.get<ApiResponse<Auction>>(
                    API_ENDPOINTS.AUCTION_BY_ID(auctionId)
                );
                if (isMounted) {
                    setAuction(data.data);
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

        fetchAuction();

        return () => {
            isMounted = false;
        };
    }, [auctionId]);

    if (isLoading) {
        return (
            <MainLayout>
                <div className="container py-8 flex justify-center">
                    <Loader2 className="h-8 w-8 animate-spin" />
                </div>
            </MainLayout>
        );
    }

    if (error || !auction) {
        return (
            <MainLayout>
                <div className="container py-8 max-w-4xl mx-auto">
                    <Alert variant="destructive">
                        <AlertDescription>
                            Failed to load auction. Please try again.
                        </AlertDescription>
                    </Alert>
                </div>
            </MainLayout>
        );
    }

    return (
        <MainLayout>
            <div className="container py-8 max-w-4xl mx-auto">
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
                            <BreadcrumbLink asChild>
                                <Link href={`/auctions/${auctionId}`}>{auction.make} {auction.model}</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>Edit</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <EditAuctionForm auction={auction} />
            </div>
        </MainLayout>
    );
}
