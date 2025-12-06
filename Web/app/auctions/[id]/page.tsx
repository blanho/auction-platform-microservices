'use client';

import { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { useSession } from 'next-auth/react';
import Image from 'next/image';
import { format } from 'date-fns';
import {
    Calendar,
    DollarSign,
    Edit,
    Gauge,
    History,
    Loader2,
    MapPin,
    PlayCircle,
    PauseCircle,
    User
} from 'lucide-react';

import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
    Card,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { MainLayout } from '@/components/layout/main-layout';
import {
    Breadcrumb,
    BreadcrumbItem,
    BreadcrumbLink,
    BreadcrumbList,
    BreadcrumbPage,
    BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';

import { Auction, AuctionStatus } from '@/types/auction';

import { DeleteAuctionDialog } from '@/features/auction/delete-auction-dialog';
import { ActivateAuctionDialog } from '@/features/auction/activate-auction-dialog';
import { DeactivateAuctionDialog } from '@/features/auction/deactivate-auction-dialog';
import { auctionService } from '@/services/auction.service';
import { AuditHistory } from '@/components/common/audit-history';

export default function AuctionDetailPage() {
    const params = useParams();
    const router = useRouter();
    const { data: session } = useSession();
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
                const result = await auctionService.getAuctionById(auctionId);
                if (isMounted) {
                    setAuction(result);
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
        const errorResponse = error && typeof error === 'object' && 'response' in error
            ? (error as { response?: { status?: number } })
            : null;
        const isNotFound = errorResponse?.response?.status === 404;

        return (
            <MainLayout>
                <div className="container py-8 max-w-6xl mx-auto">
                    <Alert variant="destructive">
                        <AlertDescription>
                            {isNotFound
                                ? 'Auction not found. It may have been deleted or does not exist.'
                                : 'Failed to load auction. Please try again.'}
                        </AlertDescription>
                    </Alert>
                    <div className="mt-4">
                        <Button asChild>
                            <Link href="/auctions">Back to Auctions</Link>
                        </Button>
                    </div>
                </div>
            </MainLayout>
        );
    }

    const isOwner = session?.user?.name === auction.seller;

    const getStatusColor = (status: AuctionStatus) => {
        switch (status) {
            case AuctionStatus.Live:
                return 'bg-green-500';
            case AuctionStatus.Finished:
                return 'bg-gray-500';
            case AuctionStatus.ReserveNotMet:
                return 'bg-yellow-500';
            case AuctionStatus.Cancelled:
                return 'bg-red-500';
            default:
                return 'bg-gray-500';
        }
    };

    return (
        <MainLayout>
            <div className="container py-8 max-w-6xl mx-auto">
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
                            <BreadcrumbPage>{auction.make} {auction.model}</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>
                <div className="mb-6 flex items-center justify-between">
                    <Button variant="outline" onClick={() => router.back()}>
                        ← Back
                    </Button>
                    {isOwner && (
                        <div className="flex gap-2">
                            {auction.status === AuctionStatus.Inactive && (
                                <ActivateAuctionDialog
                                    auctionId={auction.id}
                                    auctionTitle={auction.title}
                                    onSuccess={() => window.location.reload()}
                                    trigger={
                                        <Button variant="outline" size="sm" className="text-green-600">
                                            <PlayCircle className="mr-2 h-4 w-4" />
                                            Activate
                                        </Button>
                                    }
                                />
                            )}
                            {auction.status === AuctionStatus.Live && (
                                <DeactivateAuctionDialog
                                    auctionId={auction.id}
                                    auctionTitle={auction.title}
                                    onSuccess={() => window.location.reload()}
                                    trigger={
                                        <Button variant="outline" size="sm" className="text-orange-600">
                                            <PauseCircle className="mr-2 h-4 w-4" />
                                            Deactivate
                                        </Button>
                                    }
                                />
                            )}
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => router.push(`/auctions/${auction.id}/edit`)}
                            >
                                <Edit className="mr-2 h-4 w-4" />
                                Edit
                            </Button>
                            <DeleteAuctionDialog
                                auctionId={auction.id}
                                auctionTitle={auction.title}
                                redirectAfterDelete="/auctions"
                            />
                        </div>
                    )}
                </div>

                <div className="grid gap-6 lg:grid-cols-2">
                    {/* Image Section */}
                    <Card>
                        <CardContent className="p-0">
                            <div className="relative aspect-video w-full overflow-hidden rounded-lg">
                                {auction.imageUrl ? (
                                    <Image
                                        src={auction.imageUrl}
                                        alt={auction.title}
                                        fill
                                        className="object-cover"
                                        priority
                                    />
                                ) : (
                                    <div className="flex h-full items-center justify-center bg-muted text-muted-foreground">
                                        No image available
                                    </div>
                                )}
                            </div>
                        </CardContent>
                    </Card>

                    {/* Details Section */}
                    <div className="space-y-6">
                        <Card>
                            <CardHeader>
                                <div className="flex items-start justify-between">
                                    <div className="space-y-1">
                                        <CardTitle className="text-2xl">{auction.title}</CardTitle>
                                        <CardDescription>
                                            {auction.year} {auction.make} {auction.model}
                                        </CardDescription>
                                    </div>
                                    <Badge className={getStatusColor(auction.status)}>
                                        {auction.status}
                                    </Badge>
                                </div>
                            </CardHeader>
                            <CardContent className="space-y-4">
                                <div className="grid grid-cols-2 gap-4">
                                    <div className="flex items-center gap-2">
                                        <DollarSign className="h-4 w-4 text-muted-foreground" />
                                        <div>
                                            <p className="text-sm text-muted-foreground">Current Bid</p>
                                            <p className="font-semibold">
                                                ${auction.currentHighBid?.toLocaleString() || '0'}
                                            </p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <DollarSign className="h-4 w-4 text-muted-foreground" />
                                        <div>
                                            <p className="text-sm text-muted-foreground">Reserve Price</p>
                                            <p className="font-semibold">
                                                ${auction.reservePrice.toLocaleString()}
                                            </p>
                                        </div>
                                    </div>
                                </div>

                                <Separator />

                                <div className="space-y-2">
                                    <div className="flex items-center gap-2">
                                        <Gauge className="h-4 w-4 text-muted-foreground" />
                                        <span className="text-sm">
                                            Mileage: {auction.mileage.toLocaleString()} miles
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <MapPin className="h-4 w-4 text-muted-foreground" />
                                        <span className="text-sm">Color: {auction.color}</span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Calendar className="h-4 w-4 text-muted-foreground" />
                                        <span className="text-sm">
                                            Ends: {format(new Date(auction.auctionEnd), 'PPP p')}
                                        </span>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <User className="h-4 w-4 text-muted-foreground" />
                                        <span className="text-sm">Seller: {auction.seller}</span>
                                    </div>
                                </div>

                                {auction.winner && (
                                    <>
                                        <Separator />
                                        <div className="rounded-lg bg-muted p-3">
                                            <p className="text-sm font-semibold">Winner: {auction.winner}</p>
                                            {auction.soldAmount && (
                                                <p className="text-sm text-muted-foreground">
                                                    Sold for: ${auction.soldAmount.toLocaleString()}
                                                </p>
                                            )}
                                        </div>
                                    </>
                                )}
                            </CardContent>
                        </Card>

                        {auction.status === AuctionStatus.Live && !isOwner && (
                            <Button className="w-full" size="lg">
                                Place Bid
                            </Button>
                        )}
                    </div>
                </div>

                {/* Description */}
                <Card className="mt-6">
                    <CardHeader>
                        <CardTitle>Description</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm leading-relaxed whitespace-pre-wrap">
                            {auction.description}
                        </p>
                    </CardContent>
                </Card>

                {/* Audit History */}
                {isOwner && (
                    <Card className="mt-6">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <History className="h-5 w-5" />
                                Activity History
                            </CardTitle>
                            <CardDescription>
                                Track all changes made to this auction
                            </CardDescription>
                        </CardHeader>
                        <CardContent>
                            <AuditHistory
                                entityType="Auction"
                                entityId={auction.id}
                                maxHeight="400px"
                                showDetails={true}
                            />
                        </CardContent>
                    </Card>
                )}
            </div>
        </MainLayout>
    );
}
