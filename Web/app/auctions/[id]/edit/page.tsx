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
import { AuctionForm, AuctionFormValues } from '@/features/auction/auction-form';
import { auctionService } from '@/services/auction.service';
import { Auction, Category, UpdateAuctionDto, ShippingType } from '@/types/auction';
import { UploadedImage } from '@/components/ui/image-upload';
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

    const handleSubmit = async (values: AuctionFormValues, images: UploadedImage[]) => {
        const successfulImages = images.filter(img => img.status === 'success');
        const files = successfulImages.map((img, index) => ({
            url: img.url,
            fileId: img.fileId,
            fileName: img.name,
            contentType: 'image/jpeg',
            size: 0,
            displayOrder: index,
            isPrimary: img.isPrimary || (index === 0 && !successfulImages.some(i => i.isPrimary)),
        }));

        const data: UpdateAuctionDto = {
            title: values.title,
            description: values.description,
            condition: values.condition || undefined,
            yearManufactured: values.yearManufactured || undefined,
            reservePrice: values.reservePrice,
            buyNowPrice: values.buyNowPrice || undefined,
            auctionEnd: values.auctionEnd,
            categoryId: values.categoryId || undefined,
            isFeatured: values.isFeatured,
            shippingType: values.shippingType,
            shippingCost: values.shippingType === ShippingType.Flat ? values.shippingCost : undefined,
            handlingTime: values.handlingTime,
            shipsFrom: values.shipsFrom || undefined,
            localPickupAvailable: values.localPickupAvailable,
            localPickupAddress: values.localPickupAvailable ? values.localPickupAddress : undefined,
            files: files.length > 0 ? files : undefined,
        };

        await auctionService.updateAuction(auctionId, data);
    };

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
                    <AuctionForm
                        mode="edit"
                        initialData={auction}
                        categories={categories}
                        onSubmit={handleSubmit}
                    />
                </div>
            </MainLayout>
        </RequireAuth>
    );
}
