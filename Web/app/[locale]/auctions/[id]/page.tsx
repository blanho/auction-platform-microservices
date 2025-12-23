'use client';

import { useState, useEffect, useMemo, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { format, formatDistanceToNow } from 'date-fns';
import { motion } from 'framer-motion';
import Image from 'next/image';

import { ROUTES, MESSAGES, AUCTION_BID } from '@/constants';
import {
    formatCurrency,
    getTimeRemaining,
    getAuctionTitle,
    getAuctionDescription,
    getAuctionAttributes,
    getAuctionYearManufactured,
    getAuctionSellerUsername,
    getAuctionWinnerUsername,
} from '@/utils';
import { useAuthSession } from '@/hooks/use-auth-session';

import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {
    faGavel,
    faHeart,
    faClock,
    faCalendarAlt,
    faTachometerAlt,
    faPalette,
    faTag,
    faStar,
    faShare,
    faCopy,
    faPlay,
    faPause,
    faEdit,
    faDollarSign,
    faTrophy,
    faCheckCircle,
    faChevronLeft,
    faUser,
    faBolt,
    faShieldAlt,
    faFire,
    faHistory,
} from '@fortawesome/free-solid-svg-icons';
import { faFacebook, faXTwitter } from '@fortawesome/free-brands-svg-icons';

import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
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
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { toast } from 'sonner';

import { Auction, AuctionStatus } from '@/types/auction';
import { getAuctionStatusStyle } from '@/constants/status';

import { DeleteAuctionDialog } from '@/features/auction/delete-auction-dialog';
import { ActivateAuctionDialog } from '@/features/auction/activate-auction-dialog';
import { DeactivateAuctionDialog } from '@/features/auction/deactivate-auction-dialog';
import { BuyNowButton } from '@/features/auction/buy-now-button';
import { CountdownTimer, EnhancedGallery, EnhancedBidHistory, AuctionDetailTabs, ShippingInfo } from '@/features/auction/auction-detail';
import { PlaceBidDialog, AutoBidDialog } from '@/features/bid';
import { ReviewsSection } from '@/features/review';
import { useAuctionQuery, useRelatedAuctionsQuery } from '@/hooks/queries';
import { bookmarkService } from '@/services/bookmark.service';
import { AuditHistory } from '@/components/common/audit-history';
import { StickyBidBar } from '@/components/common/mobile-enhancements';
import { bidService } from '@/services/bid.service';

export default function AuctionDetailPage() {
    const params = useParams();
    const router = useRouter();
    const { user } = useAuthSession();
    const auctionId = params?.id as string;
    const { data: auction, isLoading, error, refetch: refetchAuction } = useAuctionQuery(auctionId);
    const { data: relatedAuctions = [] } = useRelatedAuctionsQuery(auction?.categoryId, auctionId, !!auction);
    const [bidRefreshTrigger, setBidRefreshTrigger] = useState(0);
    const [isInWatchlist, setIsInWatchlist] = useState(false);
    const [bidAmount, setBidAmount] = useState('');

    const handleBidPlaced = useCallback(() => {
        setBidRefreshTrigger(prev => prev + 1);
        refetchAuction();
    }, [refetchAuction]);

    const handlePlaceBidFromMobile = useCallback(async (amount: number) => {
        await bidService.placeBid({
            auctionId,
            amount,
        });
        handleBidPlaced();
    }, [auctionId, handleBidPlaced]);

    const auctionFiles = auction?.files;
    const images = useMemo(() => {
        if (!auctionFiles?.length) return [];
        return auctionFiles
            .filter(f => f.fileType === 'Image' && f.url)
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map(f => f.url!);
    }, [auctionFiles]);

    const attributes = useMemo(() => 
        auction ? getAuctionAttributes(auction) : {}, 
        [auction]
    );

    const toggleWatchlist = useCallback(async () => {
        if (!auction) return;
        if (!user) {
            toast.error('Please sign in to add to watchlist');
            return;
        }
        try {
            const result = await bookmarkService.toggleBookmark(auction.id, 0);
            setIsInWatchlist(result.isBookmarked);
            toast.success(result.message);
        } catch {
            toast.error('Failed to update watchlist');
        }
    }, [auction, user]);

    const copyLink = useCallback(() => {
        navigator.clipboard.writeText(window.location.href);
        toast.success('Link copied to clipboard');
    }, []);

    const shareToTwitter = useCallback(() => {
        const text = `Check out this auction: ${auction?.title}`;
        window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(window.location.href)}`, '_blank');
    }, [auction?.title]);

    const shareToFacebook = useCallback(() => {
        window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(window.location.href)}`, '_blank');
    }, []);

    useEffect(() => {
        if (!auction?.id || !user) {
            return;
        }

        let isMounted = true;
        bookmarkService.isInWatchlist(auction.id)
            .then(inWatchlist => {
                if (isMounted) setIsInWatchlist(inWatchlist);
            })
            .catch(() => {
                if (isMounted) setIsInWatchlist(false);
            });

        return () => { isMounted = false; };
    }, [auction?.id, user]);

    if (isLoading) {
        return (
            <MainLayout>
                <div className="container py-16 flex flex-col items-center justify-center min-h-[60vh]">
                    <motion.div
                        animate={{ rotate: 360 }}
                        transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                        className="w-16 h-16 rounded-full bg-linear-to-r from-purple-600 to-blue-600 flex items-center justify-center shadow-lg shadow-purple-500/25"
                    >
                        <FontAwesomeIcon icon={faGavel} className="w-8 h-8 text-white" />
                    </motion.div>
                    <p className="mt-4 text-slate-500 animate-pulse">Loading auction details...</p>
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
                    <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }}>
                        <Alert variant="destructive" className="border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/50">
                            <AlertDescription className="flex items-center gap-2">
                                <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                {isNotFound
                                    ? 'Auction not found. It may have been deleted or does not exist.'
                                    : 'Failed to load auction. Please try again.'}
                            </AlertDescription>
                        </Alert>
                        <div className="mt-4">
                            <Button asChild className="bg-linear-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
                                <Link href={ROUTES.AUCTIONS.LIST}>
                                    <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 mr-2" />
                                    Back to Auctions
                                </Link>
                            </Button>
                        </div>
                    </motion.div>
                </div>
            </MainLayout>
        );
    }

    const sellerUsername = getAuctionSellerUsername(auction);
    const isOwner = user?.name === sellerUsername;
    const statusConfig = getAuctionStatusStyle(auction.status);
    const timeRemaining = getTimeRemaining(auction.auctionEnd);
    const isUrgent = timeRemaining && timeRemaining.days === 0 && timeRemaining.hours < 1;
    const auctionTitle = getAuctionTitle(auction);
    const yearManufactured = getAuctionYearManufactured(auction);
    const isLive = auction.status === AuctionStatus.Live;

    return (
        <MainLayout>
            <div className="container py-8 max-w-7xl mx-auto pb-24 md:pb-8">
                <motion.div initial={{ opacity: 0, y: -10 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.3 }}>
                    <Breadcrumb className="mb-6">
                        <BreadcrumbList>
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href={ROUTES.HOME} className="text-slate-500 hover:text-purple-600 transition-colors">Home</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href={ROUTES.AUCTIONS.LIST} className="text-slate-500 hover:text-purple-600 transition-colors">Auctions</Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            {auction.categoryName && (
                                <>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink asChild>
                                            <Link href={`${ROUTES.AUCTIONS.LIST}?category=${auction.categorySlug}`} className="text-slate-500 hover:text-purple-600 transition-colors">
                                                {auction.categoryName}
                                            </Link>
                                        </BreadcrumbLink>
                                    </BreadcrumbItem>
                                </>
                            )}
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage className="font-medium text-slate-900 dark:text-white">{auctionTitle}</BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>
                </motion.div>

                {isOwner && (
                    <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="mb-6 flex flex-wrap items-center justify-between gap-4 p-4 rounded-xl bg-linear-to-r from-purple-50 to-blue-50 dark:from-purple-950/30 dark:to-blue-950/30 border border-purple-200/50 dark:border-purple-800/50"
                    >
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-linear-to-r from-purple-600 to-blue-600 flex items-center justify-center">
                                <FontAwesomeIcon icon={faUser} className="w-5 h-5 text-white" />
                            </div>
                            <span className="text-sm font-medium text-purple-700 dark:text-purple-300">You are the owner of this auction</span>
                        </div>
                        <div className="flex gap-2 flex-wrap">
                            {auction.status === AuctionStatus.Inactive && (
                                <ActivateAuctionDialog
                                    auctionId={auction.id}
                                    auctionTitle={auctionTitle}
                                    onSuccess={() => window.location.reload()}
                                    trigger={
                                        <Button variant="outline" size="sm" className="border-green-300 text-green-600 hover:bg-green-50 dark:border-green-700 dark:hover:bg-green-950">
                                            <FontAwesomeIcon icon={faPlay} className="mr-2 w-3 h-3" />
                                            Activate
                                        </Button>
                                    }
                                />
                            )}
                            {auction.status === AuctionStatus.Live && (
                                <DeactivateAuctionDialog
                                    auctionId={auction.id}
                                    auctionTitle={auctionTitle}
                                    onSuccess={() => window.location.reload()}
                                    trigger={
                                        <Button variant="outline" size="sm" className="border-orange-300 text-orange-600 hover:bg-orange-50 dark:border-orange-700 dark:hover:bg-orange-950">
                                            <FontAwesomeIcon icon={faPause} className="mr-2 w-3 h-3" />
                                            Deactivate
                                        </Button>
                                    }
                                />
                            )}
                            <Button
                                variant="outline"
                                size="sm"
                                onClick={() => router.push(`/auctions/${auction.id}/edit`)}
                                className="border-slate-300 hover:bg-slate-50 dark:border-slate-700 dark:hover:bg-slate-800"
                            >
                                <FontAwesomeIcon icon={faEdit} className="mr-2 w-3 h-3" />
                                Edit
                            </Button>
                            <DeleteAuctionDialog auctionId={auction.id} auctionTitle={auctionTitle} redirectAfterDelete="/auctions" />
                        </div>
                    </motion.div>
                )}

                <div className="grid gap-8 lg:grid-cols-5">
                    <motion.div
                        initial={{ opacity: 0, x: -20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: 0.4 }}
                        className="lg:col-span-3 space-y-4"
                    >
                        <div className="relative">
                            <div className={`absolute top-4 left-4 z-10 px-4 py-2 rounded-full bg-linear-to-r ${statusConfig.gradient} text-white text-sm font-semibold shadow-lg ${statusConfig.bgGlow} flex items-center gap-2`}>
                                {statusConfig.pulse && <span className="w-2 h-2 rounded-full bg-white animate-pulse" />}
                                {auction.status}
                            </div>
                            {isLive && isUrgent && (
                                <div className="absolute top-4 right-4 z-10 px-3 py-1.5 rounded-full bg-linear-to-r from-red-500 to-orange-500 text-white text-xs font-semibold shadow-lg animate-pulse flex items-center gap-1.5">
                                    <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                                    Ending Soon
                                </div>
                            )}
                            <EnhancedGallery images={images} title={auctionTitle} />
                        </div>

                        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardHeader className="pb-2">
                                <div className="flex items-start justify-between">
                                    <div>
                                        <CardTitle className="text-2xl font-bold bg-linear-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
                                            {auctionTitle}
                                        </CardTitle>
                                        <CardDescription className="text-base mt-1">
                                            {yearManufactured && `${yearManufactured} `}
                                            {Object.values(attributes).join(' ')}
                                        </CardDescription>
                                    </div>
                                    {auction.isFeatured && (
                                        <Badge className="bg-linear-to-r from-amber-500 to-orange-500 text-white border-0">
                                            <FontAwesomeIcon icon={faFire} className="w-3 h-3 mr-1" />
                                            Featured
                                        </Badge>
                                    )}
                                </div>
                            </CardHeader>
                            <CardContent>
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                    {attributes.mileage && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-blue-500 to-cyan-500 flex items-center justify-center">
                                                <FontAwesomeIcon icon={faTachometerAlt} className="w-4 h-4 text-white" />
                                            </div>
                                            <div>
                                                <p className="text-xs text-slate-500">Mileage</p>
                                                <p className="font-semibold text-slate-900 dark:text-white">{attributes.mileage}</p>
                                            </div>
                                        </div>
                                    )}
                                    {attributes.color && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-purple-500 to-pink-500 flex items-center justify-center">
                                                <FontAwesomeIcon icon={faPalette} className="w-4 h-4 text-white" />
                                            </div>
                                            <div>
                                                <p className="text-xs text-slate-500">Color</p>
                                                <p className="font-semibold text-slate-900 dark:text-white">{attributes.color}</p>
                                            </div>
                                        </div>
                                    )}
                                    <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <div className="w-10 h-10 rounded-full bg-linear-to-br from-emerald-500 to-green-500 flex items-center justify-center">
                                            <FontAwesomeIcon icon={faCalendarAlt} className="w-4 h-4 text-white" />
                                        </div>
                                        <div>
                                            <p className="text-xs text-slate-500">Year</p>
                                            <p className="font-semibold text-slate-900 dark:text-white">{yearManufactured || 'N/A'}</p>
                                        </div>
                                    </div>
                                    {auction.categoryName && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-linear-to-br from-amber-500 to-orange-500 flex items-center justify-center">
                                                <FontAwesomeIcon icon={faTag} className="w-4 h-4 text-white" />
                                            </div>
                                            <div>
                                                <p className="text-xs text-slate-500">Category</p>
                                                <p className="font-semibold text-slate-900 dark:text-white">{auction.categoryName}</p>
                                            </div>
                                        </div>
                                    )}
                                </div>
                            </CardContent>
                        </Card>
                    </motion.div>

                    <motion.div
                        initial={{ opacity: 0, x: 20 }}
                        animate={{ opacity: 1, x: 0 }}
                        transition={{ duration: 0.4, delay: 0.1 }}
                        className="lg:col-span-2 space-y-4"
                    >
                        <Card className="border-0 shadow-xl bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl overflow-hidden">
                            <div className="absolute inset-x-0 top-0 h-1 bg-linear-to-r from-purple-600 to-blue-600" />
                            <CardContent className="pt-8">
                                <div className="space-y-6">
                                    <div>
                                        <p className="text-sm text-slate-500 flex items-center gap-2">
                                            <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                            Current Price
                                        </p>
                                        <p className="text-4xl font-bold bg-linear-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mt-1">
                                            {formatCurrency(auction.currentHighBid || 0)}
                                        </p>
                                    </div>

                                    <div className="p-4 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <div className="flex items-center gap-2 mb-3">
                                            <FontAwesomeIcon icon={faClock} className="w-4 h-4 text-slate-400" />
                                            <span className="text-sm text-slate-500">Time Remaining</span>
                                        </div>
                                        <CountdownTimer endDate={auction.auctionEnd} isUrgent={isUrgent} />
                                    </div>

                                    {isLive && !isOwner && (
                                        <>
                                            <Separator className="bg-slate-200 dark:bg-slate-700" />
                                            <BiddingSection
                                                auction={auction}
                                                auctionTitle={auctionTitle}
                                                bidAmount={bidAmount}
                                                setBidAmount={setBidAmount}
                                                handleBidPlaced={handleBidPlaced}
                                            />
                                        </>
                                    )}

                                    {getAuctionWinnerUsername(auction) && (
                                        <motion.div
                                            initial={{ opacity: 0, y: 10 }}
                                            animate={{ opacity: 1, y: 0 }}
                                            className="rounded-xl bg-linear-to-br from-emerald-50 to-green-50 dark:from-emerald-950/30 dark:to-green-950/30 p-5 border border-emerald-200/50 dark:border-emerald-800/50"
                                        >
                                            <p className="text-sm font-semibold text-emerald-700 dark:text-emerald-400 flex items-center gap-2">
                                                <FontAwesomeIcon icon={faTrophy} className="w-5 h-5 text-amber-500" />
                                                Winner: {getAuctionWinnerUsername(auction)}
                                            </p>
                                            {auction.soldAmount && (
                                                <p className="text-sm text-emerald-600 dark:text-emerald-500 mt-1">
                                                    Sold for: {formatCurrency(auction.soldAmount)}
                                                </p>
                                            )}
                                        </motion.div>
                                    )}

                                    <Separator className="bg-slate-200 dark:bg-slate-700" />

                                    <div className="flex gap-2">
                                        <TooltipProvider>
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <motion.button
                                                        whileHover={{ scale: 1.05 }}
                                                        whileTap={{ scale: 0.95 }}
                                                        onClick={toggleWatchlist}
                                                        className={`w-12 h-12 rounded-xl flex items-center justify-center transition-all ${isInWatchlist
                                                            ? 'bg-red-100 dark:bg-red-950/50 text-red-500'
                                                            : 'bg-slate-100 dark:bg-slate-800 text-slate-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30'
                                                        }`}
                                                    >
                                                        <FontAwesomeIcon icon={faHeart} className={`w-5 h-5 ${isInWatchlist ? 'text-red-500' : ''}`} />
                                                    </motion.button>
                                                </TooltipTrigger>
                                                <TooltipContent>{isInWatchlist ? 'Remove from Watchlist' : 'Add to Watchlist'}</TooltipContent>
                                            </Tooltip>
                                        </TooltipProvider>

                                        <DropdownMenu>
                                            <DropdownMenuTrigger asChild>
                                                <motion.button
                                                    whileHover={{ scale: 1.05 }}
                                                    whileTap={{ scale: 0.95 }}
                                                    className="w-12 h-12 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center text-slate-500 hover:text-purple-500 hover:bg-purple-50 dark:hover:bg-purple-950/30 transition-all"
                                                >
                                                    <FontAwesomeIcon icon={faShare} className="w-5 h-5" />
                                                </motion.button>
                                            </DropdownMenuTrigger>
                                            <DropdownMenuContent className="rounded-xl">
                                                <DropdownMenuItem onClick={shareToFacebook} className="cursor-pointer">
                                                    <FontAwesomeIcon icon={faFacebook} className="w-4 h-4 mr-2 text-blue-600" />
                                                    Facebook
                                                </DropdownMenuItem>
                                                <DropdownMenuItem onClick={shareToTwitter} className="cursor-pointer">
                                                    <FontAwesomeIcon icon={faXTwitter} className="w-4 h-4 mr-2" />
                                                    X (Twitter)
                                                </DropdownMenuItem>
                                                <DropdownMenuItem onClick={copyLink} className="cursor-pointer">
                                                    <FontAwesomeIcon icon={faCopy} className="w-4 h-4 mr-2 text-slate-500" />
                                                    Copy Link
                                                </DropdownMenuItem>
                                            </DropdownMenuContent>
                                        </DropdownMenu>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>

                        <SellerCard sellerUsername={sellerUsername} />

                        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardContent className="pt-6">
                                <div className="space-y-3">
                                    <div className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <span className="text-sm text-slate-500 flex items-center gap-2">
                                            <FontAwesomeIcon icon={faShieldAlt} className="w-4 h-4" />
                                            Reserve Price
                                        </span>
                                        <span className={`font-semibold ${auction.currentHighBid && auction.currentHighBid >= auction.reservePrice
                                            ? 'text-emerald-600 dark:text-emerald-400'
                                            : 'text-slate-900 dark:text-white'
                                        }`}>
                                            {auction.currentHighBid && auction.currentHighBid >= auction.reservePrice
                                                ? MESSAGES.LABELS.RESERVE_MET
                                                : formatCurrency(auction.reservePrice)}
                                        </span>
                                    </div>
                                    <div className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <span className="text-sm text-slate-500 flex items-center gap-2">
                                            <FontAwesomeIcon icon={faCalendarAlt} className="w-4 h-4" />
                                            Auction Ends
                                        </span>
                                        <span className="font-medium text-sm text-slate-900 dark:text-white">
                                            {format(new Date(auction.auctionEnd), 'PPP p')}
                                        </span>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                    </motion.div>
                </div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.4, delay: 0.2 }}
                    className="mt-8"
                >
                    <AuctionDetailTabs
                        descriptionContent={
                            <div className="prose dark:prose-invert max-w-none">
                                <p className="text-sm leading-relaxed whitespace-pre-wrap text-slate-600 dark:text-slate-300">
                                    {getAuctionDescription(auction)}
                                </p>
                            </div>
                        }
                        bidsContent={
                            <EnhancedBidHistory
                                auctionId={auction.id}
                                refreshTrigger={bidRefreshTrigger}
                                isLive={isLive}
                                pollingInterval={isLive ? 5000 : undefined}
                            />
                        }
                        shippingContent={
                            <ShippingInfo
                                shippingOptions={[
                                    { method: 'Standard Shipping', price: 0, estimatedDays: '5-7 business days' },
                                    { method: 'Express Shipping', price: 25, estimatedDays: '2-3 business days', carrier: 'FedEx' },
                                ]}
                                sellerLocation="United States"
                                shipsTo={['United States', 'Canada', 'Europe']}
                                returnPolicy="30-day return policy. Buyer pays return shipping."
                            />
                        }
                        reviewsContent={
                            <ReviewsSection
                                auctionId={auction.id}
                                sellerId={auction.sellerId}
                                sellerUsername={sellerUsername}
                                auctionStatus={auction.status}
                                winnerId={auction.winnerId}
                            />
                        }
                        reviewCount={0}
                    />
                </motion.div>

                {relatedAuctions.length > 0 && (
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4, delay: 0.4 }}
                        className="mt-8"
                    >
                        <div className="flex items-center gap-3 mb-6">
                            <div className="w-10 h-10 rounded-xl bg-linear-to-br from-emerald-500 to-green-600 flex items-center justify-center shadow-lg shadow-emerald-500/25">
                                <FontAwesomeIcon icon={faGavel} className="w-5 h-5 text-white" />
                            </div>
                            <h2 className="text-2xl font-bold bg-linear-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
                                Related Items
                            </h2>
                        </div>
                        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                            {relatedAuctions.map((related, index) => {
                                const relatedImage = related.files?.find(f => f.isPrimary && f.url)?.url || '/placeholder-car.jpg';
                                return (
                                    <motion.div
                                        key={related.id}
                                        initial={{ opacity: 0, y: 20 }}
                                        animate={{ opacity: 1, y: 0 }}
                                        transition={{ duration: 0.3, delay: 0.1 * index }}
                                    >
                                        <Link href={`/auctions/${related.id}`}>
                                            <Card className="overflow-hidden border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl group cursor-pointer hover:shadow-xl transition-all duration-300 hover:-translate-y-1">
                                                <div className="relative aspect-4/3">
                                                    <Image
                                                        src={relatedImage}
                                                        alt={related.title}
                                                        fill
                                                        className="object-cover transition-transform duration-500 group-hover:scale-105"
                                                    />
                                                    <div className="absolute inset-0 bg-linear-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                                                </div>
                                                <CardContent className="p-4">
                                                    <h3 className="font-semibold truncate text-slate-900 dark:text-white group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                                        {related.title}
                                                    </h3>
                                                    <p className="text-sm text-slate-500 mt-0.5">{related.categoryName || 'Uncategorized'}</p>
                                                    <div className="flex items-center justify-between mt-3">
                                                        <span className="font-bold bg-linear-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
                                                            {formatCurrency(related.currentHighBid || related.reservePrice)}
                                                        </span>
                                                        <span className="text-xs text-slate-400 flex items-center gap-1">
                                                            <FontAwesomeIcon icon={faClock} className="w-3 h-3" />
                                                            {formatDistanceToNow(new Date(related.auctionEnd), { addSuffix: true })}
                                                        </span>
                                                    </div>
                                                </CardContent>
                                            </Card>
                                        </Link>
                                    </motion.div>
                                );
                            })}
                        </div>
                    </motion.div>
                )}

                {isOwner && (
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4, delay: 0.5 }}
                    >
                        <Card className="mt-6 border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardHeader>
                                <CardTitle className="flex items-center gap-2 text-xl">
                                    <div className="w-8 h-8 rounded-lg bg-linear-to-br from-slate-600 to-slate-800 flex items-center justify-center">
                                        <FontAwesomeIcon icon={faHistory} className="w-4 h-4 text-white" />
                                    </div>
                                    Activity History
                                </CardTitle>
                                <CardDescription>Track all changes made to this auction</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <AuditHistory entityType="Auction" entityId={auction.id} maxHeight="400px" showDetails={true} />
                            </CardContent>
                        </Card>
                    </motion.div>
                )}
            </div>

            {isLive && !isOwner && (
                <StickyBidBar
                    currentBid={auction.currentHighBid || auction.reservePrice}
                    minBidIncrement={AUCTION_BID.MIN_INCREMENT}
                    isLive={isLive}
                    isAuthenticated={!!user}
                    onPlaceBid={handlePlaceBidFromMobile}
                    onBuyNow={auction.isBuyNowAvailable && auction.buyNowPrice ? () => {} : undefined}
                    buyNowPrice={auction.buyNowPrice}
                />
            )}
        </MainLayout>
    );
}

function BiddingSection({
    auction,
    auctionTitle,
    bidAmount,
    setBidAmount,
    handleBidPlaced,
}: {
    auction: Auction;
    auctionTitle: string;
    bidAmount: string;
    setBidAmount: React.Dispatch<React.SetStateAction<string>>;
    handleBidPlaced: () => void;
}) {
    if (auction.isBuyNowAvailable && auction.buyNowPrice) {
        return (
            <Tabs defaultValue="bid" className="w-full">
                <TabsList className="grid w-full grid-cols-2 mb-4">
                    <TabsTrigger value="bid" className="gap-2">
                        <FontAwesomeIcon icon={faGavel} className="w-3.5 h-3.5" />
                        Place Bid
                    </TabsTrigger>
                    <TabsTrigger value="buynow" className="gap-2">
                        <FontAwesomeIcon icon={faBolt} className="w-3.5 h-3.5" />
                        Buy Now
                    </TabsTrigger>
                </TabsList>
                
                <TabsContent value="bid" className="space-y-4 mt-0">
                    <BidInputSection
                        auction={auction}
                        bidAmount={bidAmount}
                        setBidAmount={setBidAmount}
                        handleBidPlaced={handleBidPlaced}
                    />
                </TabsContent>
                
                <TabsContent value="buynow" className="mt-0">
                    <div className="rounded-xl bg-linear-to-br from-amber-50 to-orange-50 dark:from-amber-950/30 dark:to-orange-950/30 p-5 border border-amber-200/50 dark:border-amber-800/50">
                        <div className="flex items-center gap-2 mb-2">
                            <FontAwesomeIcon icon={faBolt} className="w-4 h-4 text-amber-500" />
                            <p className="text-sm font-semibold text-amber-700 dark:text-amber-400">Skip the bidding!</p>
                        </div>
                        <p className="text-3xl font-bold text-amber-600 dark:text-amber-500 mb-4">
                            {formatCurrency(auction.buyNowPrice)}
                        </p>
                        <BuyNowButton
                            auctionId={auction.id}
                            buyNowPrice={auction.buyNowPrice}
                            auctionTitle={auctionTitle}
                            onSuccess={() => window.location.reload()}
                        />
                        <p className="text-xs text-amber-600/70 dark:text-amber-400/70 mt-3">
                            Buy now and get this item immediately
                        </p>
                    </div>
                </TabsContent>
            </Tabs>
        );
    }

    return (
        <BidInputSection
            auction={auction}
            bidAmount={bidAmount}
            setBidAmount={setBidAmount}
            handleBidPlaced={handleBidPlaced}
        />
    );
}

function BidInputSection({
    auction,
    bidAmount,
    setBidAmount,
    handleBidPlaced,
}: {
    auction: Auction;
    bidAmount: string;
    setBidAmount: React.Dispatch<React.SetStateAction<string>>;
    handleBidPlaced: () => void;
}) {
    return (
        <div className="space-y-4">
            <div className="flex gap-2">
                <div className="relative flex-1">
                    <FontAwesomeIcon icon={faDollarSign} className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                    <Input
                        type="number"
                        value={bidAmount}
                        onChange={(e) => setBidAmount(e.target.value)}
                        onKeyDown={(e) => { if (e.key === 'Enter') e.preventDefault(); }}
                        className="pl-9 h-12 rounded-xl border-slate-200 dark:border-slate-700 focus:ring-2 focus:ring-purple-500 focus:border-transparent"
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
            <div className="flex flex-wrap gap-2">
                {[10, 50, 100, 500].map((increment) => (
                    <Button
                        key={increment}
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => setBidAmount((prev) => {
                            const current = parseFloat(prev) || (auction.currentHighBid || auction.reservePrice);
                            return (current + increment).toString();
                        })}
                        className="h-8 px-3 text-xs font-medium rounded-lg border-slate-200 dark:border-slate-700 hover:bg-purple-50 hover:border-purple-300 dark:hover:bg-purple-900/20 dark:hover:border-purple-700 transition-colors"
                    >
                        +${increment}
                    </Button>
                ))}
            </div>
            <div className="flex items-center justify-between">
                <p className="text-xs text-slate-500">
                    Min bid: {formatCurrency((auction.currentHighBid || auction.reservePrice) + AUCTION_BID.MIN_INCREMENT)}
                </p>
                <AutoBidDialog
                    auctionId={auction.id}
                    currentHighBid={auction.currentHighBid || 0}
                    reservePrice={auction.reservePrice}
                />
            </div>
        </div>
    );
}

function SellerCard({ sellerUsername }: { sellerUsername: string }) {
    return (
        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
            <CardHeader className="pb-3">
                <CardTitle className="text-sm font-semibold flex items-center gap-2">
                    <FontAwesomeIcon icon={faUser} className="w-4 h-4 text-purple-500" />
                    Seller
                </CardTitle>
            </CardHeader>
            <CardContent>
                <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-full bg-linear-to-br from-purple-500 to-blue-600 flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-purple-500/25">
                        {sellerUsername.charAt(0).toUpperCase()}
                    </div>
                    <div>
                        <p className="font-semibold text-slate-900 dark:text-white">{sellerUsername}</p>
                        <div className="flex items-center gap-2 text-sm text-slate-500 mt-1">
                            <FontAwesomeIcon icon={faStar} className="w-3 h-3 text-amber-400" />
                            <span className="font-medium">4.8</span>
                            <span className="text-slate-300 dark:text-slate-600">•</span>
                            <span className="flex items-center gap-1 text-emerald-600 dark:text-emerald-400">
                                <FontAwesomeIcon icon={faCheckCircle} className="w-3 h-3" />
                                Verified
                            </span>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}
