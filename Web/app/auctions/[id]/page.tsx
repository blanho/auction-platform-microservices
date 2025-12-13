'use client';

import { useState, useEffect, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { useSession } from 'next-auth/react';
import Image from 'next/image';
import { format, formatDistanceToNow } from 'date-fns';

import { ROUTES, MESSAGES, TIME, AUCTION_BID } from '@/constants';
import {
    formatCurrency,
    formatNumber,
    getTimeRemaining,
    getStatusColor,
} from '@/utils';
import {
    Calendar,
    Clock,
    Copy,
    DollarSign,
    Edit,
    Facebook,
    Gauge,
    Gavel,
    Heart,
    History,
    Loader2,
    MapPin,
    PlayCircle,
    PauseCircle,
    Share2,
    Star,
    Twitter,
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
import { Input } from '@/components/ui/input';
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
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from '@/components/ui/tooltip';
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { toast } from 'sonner';

import { Auction, AuctionStatus } from '@/types/auction';

import { DeleteAuctionDialog } from '@/features/auction/delete-auction-dialog';
import { ActivateAuctionDialog } from '@/features/auction/activate-auction-dialog';
import { DeactivateAuctionDialog } from '@/features/auction/deactivate-auction-dialog';
import { BuyNowButton } from '@/features/auction/buy-now-button';
import { PlaceBidDialog } from '@/features/bid/place-bid-dialog';
import { auctionService } from '@/services/auction.service';
import { AuditHistory } from '@/components/common/audit-history';
import { BidHistory } from '@/components/common/bid-history';

interface TimeLeft {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

function CountdownTimer({ endDate }: { endDate: string }) {
    const [timeLeft, setTimeLeft] = useState<TimeLeft | null>(() => getTimeRemaining(endDate));

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(getTimeRemaining(endDate));
        }, TIME.COUNTDOWN_INTERVAL);
        return () => clearInterval(timer);
    }, [endDate]);

    if (!timeLeft) {
        return <span className="text-red-500 font-semibold">{MESSAGES.LABELS.AUCTION_ENDED}</span>;
    }

    const showDays = timeLeft.days > 0;
    const formatUnit = (value: number) => String(value).padStart(2, '0');

    return (
        <div className="flex gap-1 text-lg font-mono font-bold">
            {showDays && (
                <>
                    <span className="bg-zinc-100 dark:bg-zinc-800 px-2 py-1 rounded">{timeLeft.days}d</span>
                    <span className="text-zinc-400">:</span>
                </>
            )}
            <span className="bg-zinc-100 dark:bg-zinc-800 px-2 py-1 rounded">
                {formatUnit(timeLeft.hours)}
            </span>
            <span className="text-zinc-400">:</span>
            <span className="bg-zinc-100 dark:bg-zinc-800 px-2 py-1 rounded">
                {formatUnit(timeLeft.minutes)}
            </span>
            <span className="text-zinc-400">:</span>
            <span className="bg-amber-500 text-white px-2 py-1 rounded">
                {formatUnit(timeLeft.seconds)}
            </span>
        </div>
    );
}

export default function AuctionDetailPage() {
    const params = useParams();
    const router = useRouter();
    const { data: session } = useSession();
    const auctionId = params?.id as string;
    const [auction, setAuction] = useState<Auction | null>(null);
    const [relatedAuctions, setRelatedAuctions] = useState<Auction[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<unknown>(null);
    const [bidRefreshTrigger, setBidRefreshTrigger] = useState(0);
    const [selectedImageIndex, setSelectedImageIndex] = useState(0);
    const [isInWatchlist, setIsInWatchlist] = useState(false);
    const [bidAmount, setBidAmount] = useState('');

    const handleBidPlaced = () => {
        setBidRefreshTrigger(prev => prev + 1);
        if (auctionId) {
            auctionService.getAuctionById(auctionId).then(setAuction);
        }
    };

    const getAuctionImages = useCallback(() => {
        if (!auction) return [];
        if (auction.files && auction.files.length > 0) {
            return auction.files
                .filter(f => f.fileType === 'Image' && f.url)
                .sort((a, b) => a.displayOrder - b.displayOrder)
                .map(f => f.url!);
        }
        return [];
    }, [auction]);

    const toggleWatchlist = () => {
        if (!auction) return;
        const watchlist = JSON.parse(localStorage.getItem('watchlist') || '[]');
        if (isInWatchlist) {
            const newWatchlist = watchlist.filter((id: string) => id !== auction.id);
            localStorage.setItem('watchlist', JSON.stringify(newWatchlist));
            setIsInWatchlist(false);
            toast.success('Removed from watchlist');
        } else {
            watchlist.push(auction.id);
            localStorage.setItem('watchlist', JSON.stringify(watchlist));
            setIsInWatchlist(true);
            toast.success('Added to watchlist');
        }
    };

    const copyLink = () => {
        navigator.clipboard.writeText(window.location.href);
        toast.success('Link copied to clipboard');
    };

    const shareToTwitter = () => {
        const text = `Check out this auction: ${auction?.title}`;
        window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(window.location.href)}`, '_blank');
    };

    const shareToFacebook = () => {
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(window.location.href)}`, '_blank');
    };

    useEffect(() => {
        if (!auctionId) {
            setIsLoading(false);
            return;
        }

        let isMounted = true;

        const fetchData = async () => {
            try {
                const result = await auctionService.getAuctionById(auctionId);
                if (isMounted) {
                    setAuction(result);
                    setError(null);

                    const minBid = (result.currentHighBid || result.reservePrice) + AUCTION_BID.MIN_INCREMENT;
                    setBidAmount(minBid.toString());

                    const watchlist = JSON.parse(localStorage.getItem('watchlist') || '[]');
                    setIsInWatchlist(watchlist.includes(result.id));

                    if (result.categoryId) {
                        try {
                            const related = await auctionService.getAuctions({
                                category: result.categoryId,
                                pageSize: 4,
                                status: AuctionStatus.Live
                            });
                            if (isMounted) {
                                setRelatedAuctions(related.items.filter(a => a.id !== result.id).slice(0, 4));
                            }
                        } catch {
                        }
                    }
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
                            <Link href={ROUTES.AUCTIONS.LIST}>Back to Auctions</Link>
                        </Button>
                    </div>
                </div>
            </MainLayout>
        );
    }

    const isOwner = session?.user?.name === auction.seller;
    const images = getAuctionImages();
    const currentImage = images[selectedImageIndex] || '/placeholder-car.jpg';

    const statusBgColor = getStatusColor(auction.status);

    return (
        <MainLayout>
            <div className="container py-8 max-w-7xl mx-auto">
                <Breadcrumb className="mb-6">
                    <BreadcrumbList>
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href={ROUTES.HOME}>Home</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbLink asChild>
                                <Link href={ROUTES.AUCTIONS.LIST}>Auctions</Link>
                            </BreadcrumbLink>
                        </BreadcrumbItem>
                        {auction.categoryName && (
                            <>
                                <BreadcrumbSeparator />
                                <BreadcrumbItem>
                                    <BreadcrumbLink asChild>
                                        <Link href={`${ROUTES.AUCTIONS.LIST}?category=${auction.categorySlug}`}>
                                            {auction.categoryName}
                                        </Link>
                                    </BreadcrumbLink>
                                </BreadcrumbItem>
                            </>
                        )}
                        <BreadcrumbSeparator />
                        <BreadcrumbItem>
                            <BreadcrumbPage>{auction.make} {auction.model}</BreadcrumbPage>
                        </BreadcrumbItem>
                    </BreadcrumbList>
                </Breadcrumb>

                {/* Owner Actions Bar */}
                {isOwner && (
                    <div className="mb-6 flex items-center justify-between p-4 bg-zinc-100 dark:bg-zinc-800 rounded-lg">
                        <span className="text-sm text-zinc-600 dark:text-zinc-400">
                            You are the owner of this auction
                        </span>
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
                    </div>
                )}

                {/* Main Content Grid - Image Gallery + Bidding Panel */}
                <div className="grid gap-8 lg:grid-cols-5">
                    {/* Image Gallery - Left Side (3 cols) */}
                    <div className="lg:col-span-3 space-y-4">
                        {/* Main Image */}
                        <Card className="overflow-hidden">
                            <CardContent className="p-0">
                                <div className="relative aspect-[4/3] w-full">
                                    <Image
                                        src={currentImage}
                                        alt={auction.title}
                                        fill
                                        className="object-cover"
                                        priority
                                    />
                                    <Badge className={`absolute top-4 left-4 ${statusBgColor} text-white`}>
                                        {auction.status}
                                    </Badge>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Thumbnails */}
                        {images.length > 1 && (
                            <div className="flex gap-2 overflow-x-auto pb-2">
                                {images.map((img, idx) => (
                                    <button
                                        key={idx}
                                        onClick={() => setSelectedImageIndex(idx)}
                                        className={`relative w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-all ${
                                            selectedImageIndex === idx
                                                ? 'border-amber-500 ring-2 ring-amber-500/50'
                                                : 'border-transparent hover:border-zinc-300'
                                        }`}
                                    >
                                        <Image
                                            src={img}
                                            alt={`${auction.title} - Image ${idx + 1}`}
                                            fill
                                            className="object-cover"
                                        />
                                    </button>
                                ))}
                            </div>
                        )}

                        {/* Item Details */}
                        <Card>
                            <CardHeader>
                                <CardTitle className="text-2xl">{auction.title}</CardTitle>
                                <CardDescription>
                                    {auction.year} {auction.make} {auction.model}
                                </CardDescription>
                            </CardHeader>
                            <CardContent>
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                    <div className="flex items-center gap-2">
                                        <Gauge className="h-4 w-4 text-zinc-400" />
                                        <div>
                                            <p className="text-xs text-zinc-500">Mileage</p>
                                            <p className="font-medium">{formatNumber(auction.mileage)} mi</p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <MapPin className="h-4 w-4 text-zinc-400" />
                                        <div>
                                            <p className="text-xs text-zinc-500">Color</p>
                                            <p className="font-medium">{auction.color}</p>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-2">
                                        <Calendar className="h-4 w-4 text-zinc-400" />
                                        <div>
                                            <p className="text-xs text-zinc-500">Year</p>
                                            <p className="font-medium">{auction.year}</p>
                                        </div>
                                    </div>
                                    {auction.categoryName && (
                                        <div className="flex items-center gap-2">
                                            <Star className="h-4 w-4 text-zinc-400" />
                                            <div>
                                                <p className="text-xs text-zinc-500">Category</p>
                                                <p className="font-medium">{auction.categoryName}</p>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </CardContent>
                        </Card>
                    </div>

                    {/* Bidding Panel - Right Side (2 cols) */}
                    <div className="lg:col-span-2 space-y-4">
                        {/* Current Bid & Timer */}
                        <Card>
                            <CardContent className="pt-6">
                                <div className="space-y-4">
                                    <div>
                                        <p className="text-sm text-zinc-500">Current Price</p>
                                        <p className="text-4xl font-bold text-amber-500">
                                            {formatCurrency(auction.currentHighBid || 0)}
                                        </p>
                                    </div>

                                    <div className="flex items-center gap-2">
                                        <Clock className="h-4 w-4 text-zinc-400" />
                                        <span className="text-sm text-zinc-500">Time Left:</span>
                                    </div>
                                    <CountdownTimer endDate={auction.auctionEnd} />

                                    <div className="flex items-center gap-2 text-sm text-zinc-500">
                                        <Gavel className="h-4 w-4" />
                                        <span>Multiple bids placed</span>
                                    </div>

                                    <Separator />

                                    {/* Bid Input */}
                                    {auction.status === AuctionStatus.Live && !isOwner && (
                                        <div className="space-y-3">
                                            <div className="flex gap-2">
                                                <div className="relative flex-1">
                                                    <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                                                    <Input
                                                        type="number"
                                                        value={bidAmount}
                                                        onChange={(e) => setBidAmount(e.target.value)}
                                                        className="pl-9"
                                                        placeholder="Enter bid amount"
                                                    />
                                                </div>
                                                <PlaceBidDialog
                                                    auctionId={auction.id}
                                                    currentHighBid={auction.currentHighBid || 0}
                                                    reservePrice={auction.reservePrice}
                                                    onBidPlaced={handleBidPlaced}
                                                />
                                            </div>
                                            <p className="text-xs text-zinc-500">
                                                Min bid: {formatCurrency((auction.currentHighBid || auction.reservePrice) + AUCTION_BID.MIN_INCREMENT)}
                                            </p>
                                        </div>
                                    )}

                                    {/* Buy Now Option */}
                                    {auction.isBuyNowAvailable && auction.buyNowPrice && auction.status === AuctionStatus.Live && !isOwner && (
                                        <div className="space-y-3">
                                            <Separator />
                                            <div className="rounded-lg bg-gradient-to-r from-amber-50 to-orange-50 dark:from-amber-900/20 dark:to-orange-900/20 p-4 border border-amber-200 dark:border-amber-800">
                                                <div className="flex items-center justify-between mb-3">
                                                    <div>
                                                        <p className="text-sm font-medium text-amber-700 dark:text-amber-400">Buy Now Available!</p>
                                                        <p className="text-2xl font-bold text-amber-600 dark:text-amber-500">
                                                            {formatCurrency(auction.buyNowPrice)}
                                                        </p>
                                                    </div>
                                                </div>
                                                <BuyNowButton
                                                    auctionId={auction.id}
                                                    buyNowPrice={auction.buyNowPrice}
                                                    auctionTitle={auction.title}
                                                    onSuccess={() => window.location.reload()}
                                                />
                                            </div>
                                        </div>
                                    )}

                                    {/* Winner display */}
                                    {auction.winner && (
                                        <div className="rounded-lg bg-green-50 dark:bg-green-900/20 p-4 border border-green-200 dark:border-green-800">
                                            <p className="text-sm font-semibold text-green-700 dark:text-green-400">
                                                🏆 Winner: {auction.winner}
                                            </p>
                                            {auction.soldAmount && (
                                                <p className="text-sm text-green-600 dark:text-green-500">
                                                    Sold for: {formatCurrency(auction.soldAmount)}
                                                </p>
                                            )}
                                        </div>
                                    )}

                                    <Separator />

                                    {/* Actions */}
                                    <div className="flex gap-2">
                                        <TooltipProvider>
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <Button
                                                        variant="outline"
                                                        size="icon"
                                                        onClick={toggleWatchlist}
                                                        className={isInWatchlist ? 'text-red-500' : ''}
                                                    >
                                                        <Heart className={`h-4 w-4 ${isInWatchlist ? 'fill-current' : ''}`} />
                                                    </Button>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    {isInWatchlist ? 'Remove from Watchlist' : 'Add to Watchlist'}
                                                </TooltipContent>
                                            </Tooltip>
                                        </TooltipProvider>

                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <Button variant="outline" size="icon">
                                                    <Share2 className="h-4 w-4" />
                                                </Button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent>
                                                <DropdownMenuItem onClick={shareToFacebook}>
                                                    <Facebook className="h-4 w-4 mr-2" />
                                                    Facebook
                                                </DropdownMenuItem>
                                                <DropdownMenuItem onClick={shareToTwitter}>
                                                    <Twitter className="h-4 w-4 mr-2" />
                                                    X (Twitter)
                                                </DropdownMenuItem>
                                                <DropdownMenuItem onClick={copyLink}>
                                                    <Copy className="h-4 w-4 mr-2" />
                                                    Copy Link
                                                </DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Seller Profile */}
                        <Card>
                            <CardHeader className="pb-3">
                                <CardTitle className="text-sm font-medium">Seller</CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="flex items-center gap-3">
                                    <div className="w-12 h-12 rounded-full bg-amber-500 flex items-center justify-center text-white font-bold text-lg">
                                        {auction.seller.charAt(0).toUpperCase()}
                                    </div>
                                    <div>
                                        <p className="font-medium">{auction.seller}</p>
                                        <div className="flex items-center gap-1 text-sm text-zinc-500">
                                            <Star className="h-3 w-3 fill-amber-400 text-amber-400" />
                                            <span>4.8</span>
                                            <span className="text-zinc-300">•</span>
                                            <span>Verified Seller</span>
                                        </div>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        {/* Reserve Price Info */}
                        <Card>
                            <CardContent className="pt-6">
                                <div className="flex items-center justify-between">
                                    <span className="text-sm text-zinc-500">Reserve Price</span>
                                    <span className="font-medium">
                                        {auction.currentHighBid && auction.currentHighBid >= auction.reservePrice
                                            ? MESSAGES.LABELS.RESERVE_MET
                                            : formatCurrency(auction.reservePrice)}
                                    </span>
                                </div>
                                <div className="flex items-center justify-between mt-2">
                                    <span className="text-sm text-zinc-500">Auction Ends</span>
                                    <span className="font-medium text-sm">
                                        {format(new Date(auction.auctionEnd), 'PPP p')}
                                    </span>
                                </div>
                            </CardContent>
                        </Card>
                    </div>
                </div>

                {/* Description Section */}
                <Card className="mt-8">
                    <CardHeader>
                        <CardTitle>Description</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <p className="text-sm leading-relaxed whitespace-pre-wrap text-zinc-600 dark:text-zinc-300">
                            {auction.description}
                        </p>
                    </CardContent>
                </Card>

                {/* Bid History */}
                <Card className="mt-6">
                    <CardHeader>
                        <CardTitle className="flex items-center gap-2">
                            <History className="h-5 w-5" />
                            Bid History
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <BidHistory
                            auctionId={auction.id}
                            refreshTrigger={bidRefreshTrigger}
                            maxHeight="300px"
                        />
                    </CardContent>
                </Card>

                {/* Related Items */}
                {relatedAuctions.length > 0 && (
                    <div className="mt-8">
                        <h2 className="text-2xl font-bold mb-4">Related Items</h2>
                        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                            {relatedAuctions.map((related) => {
                                const relatedImage = related.files?.find(f => f.isPrimary && f.url)?.url || '/placeholder-car.jpg';
                                return (
                                    <Link key={related.id} href={`/auctions/${related.id}`}>
                                        <Card className="overflow-hidden hover:shadow-lg transition-shadow cursor-pointer">
                                            <div className="relative aspect-[4/3]">
                                                <Image
                                                    src={relatedImage}
                                                    alt={related.title}
                                                    fill
                                                    className="object-cover"
                                                />
                                            </div>
                                            <CardContent className="p-4">
                                                <h3 className="font-medium truncate">{related.title}</h3>
                                                <p className="text-sm text-zinc-500">
                                                    {related.year} {related.make} {related.model}
                                                </p>
                                                <div className="flex items-center justify-between mt-2">
                                                    <span className="font-bold text-amber-500">
                                                        {formatCurrency(related.currentHighBid || related.reservePrice)}
                                                    </span>
                                                    <span className="text-xs text-zinc-400">
                                                        {formatDistanceToNow(new Date(related.auctionEnd), { addSuffix: true })}
                                                    </span>
                                                </div>
                                            </CardContent>
                                        </Card>
                                    </Link>
                                );
                            })}
                        </div>
                    </div>
                )}

                {/* Audit History (Owner Only) */}
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
