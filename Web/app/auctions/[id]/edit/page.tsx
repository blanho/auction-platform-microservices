'use client';

import { useState, useEffect } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSpinner } from '@fortawesome/free-solid-svg-icons';
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
import { RequireAuth } from '@/features/auth';
import { EditAuctionForm } from '@/features/auction/edit-auction-form';
import { auctionService } from '@/services/auction.service';
import { Auction, Category } from '@/types/auction';
import { getAuctionTitle } from '@/utils/auction';

export default function EditAuctionPage() {
    const params = useParams();
    const auctionId = params?.id as string;
    const [auction, setAuction] = useState<Auction | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<unknown>(null);

    useEffect(() => {
        if (!auctionId) {
            setIsLoading(false);
            return;
        }

        let isMounted = true;

        const fetchData = async () => {
            try {
                const [auctionResult, categoriesResult] = await Promise.all([
                    auctionService.getAuctionById(auctionId),
                    auctionService.getCategories(),
                ]);
                if (isMounted) {
                    setAuction(auctionResult);
                    setCategories(categoriesResult);
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

        fetchData();

        return () => {
            isMounted = false;
        };
    }, [auctionId]);

    if (isLoading) {
        return (
            <RequireAuth>
                <MainLayout>
                    <div className="container py-8 flex justify-center">
                        <FontAwesomeIcon icon={faSpinner} className="h-8 w-8 animate-spin" />
                    </div>
                </MainLayout>
            </RequireAuth>
        );
    }

    if (error || !auction) {
        return (
            <RequireAuth>
                <MainLayout>
                    <div className="container py-8 max-w-4xl mx-auto">
                        <Alert variant="destructive">
                            <AlertDescription>
                                Failed to load auction. Please try again.
                            </AlertDescription>
                        </Alert>
                    </div>
                </MainLayout>
            </RequireAuth>
        );
    }

    return (
        <RequireAuth>
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
                                    <Link href={`/auctions/${auctionId}`}>{getAuctionTitle(auction)}</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage>Edit</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>
                    <EditAuctionForm auction={auction} categories={categories} />
                </div>
            </MainLayout>
        </RequireAuth>
    );
}
