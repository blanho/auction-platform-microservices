'use client';

import { useState, useEffect, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import { useSession } from 'next-auth/react';
import Image from 'next/image';
import { format, formatDistanceToNow } from 'date-fns';
import { motion, AnimatePresence } from 'framer-motion';

import { ROUTES, MESSAGES, TIME, AUCTION_BID } from '@/constants';
import {
    formatCurrency,
    formatNumber,
    getTimeRemaining,
    getAuctionTitle,
    getAuctionDescription,
    getAuctionAttributes,
    getAuctionYearManufactured,
    getAuctionSellerUsername,
    getAuctionWinnerUsername,
} from '@/utils';

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
    faHistory,
    faTrophy,
    faCheckCircle,
    faChevronLeft,
    faChevronRight,
    faExpand,
    faUser,
    faBolt,
    faShieldAlt,
    faFire,
} from '@fortawesome/free-solid-svg-icons';
import {
    faFacebook,
    faXTwitter,
} from '@fortawesome/free-brands-svg-icons';

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
import { PlaceBidDialog, AutoBidDialog } from '@/features/bid';
import { auctionService } from '@/services/auction.service';
import { AuditHistory } from '@/components/common/audit-history';
import { BidHistory } from '@/components/common/bid-history';

interface TimeLeft {
    days: number;
    hours: number;
    minutes: number;
    seconds: number;
}

const STATUS_CONFIG = {
    [AuctionStatus.Live]: {
        gradient: 'from-emerald-500 to-green-600',
        bgGlow: 'shadow-emerald-500/25',
        pulse: true,
    },
    [AuctionStatus.Finished]: {
        gradient: 'from-slate-500 to-slate-600',
        bgGlow: '',
        pulse: false,
    },
    [AuctionStatus.ReserveNotMet]: {
        gradient: 'from-amber-500 to-orange-600',
        bgGlow: 'shadow-amber-500/25',
        pulse: false,
    },
    [AuctionStatus.Cancelled]: {
        gradient: 'from-red-500 to-rose-600',
        bgGlow: '',
        pulse: false,
    },
    [AuctionStatus.Inactive]: {
        gradient: 'from-slate-400 to-slate-500',
        bgGlow: '',
        pulse: false,
    },
};

function CountdownTimer({ endDate, isUrgent }: { endDate: string; isUrgent?: boolean }) {
    const [timeLeft, setTimeLeft] = useState<TimeLeft | null>(() => getTimeRemaining(endDate));

    useEffect(() => {
        const timer = setInterval(() => {
            setTimeLeft(getTimeRemaining(endDate));
        }, TIME.COUNTDOWN_INTERVAL);
        return () => clearInterval(timer);
    }, [endDate]);

    if (!timeLeft) {
        return (
            <div className="flex items-center gap-2 text-red-500 font-semibold">
                <FontAwesomeIcon icon={faGavel} className="w-5 h-5" />
                <span>{MESSAGES.LABELS.AUCTION_ENDED}</span>
            </div>
        );
    }

    const showDays = timeLeft.days > 0;
    const formatUnit = (value: number) => String(value).padStart(2, '0');
    const urgentClass = isUrgent ? 'animate-pulse' : '';

    return (
        <div className="flex items-center gap-3">
            <div className={`flex gap-2 font-mono ${urgentClass}`}>
                {showDays && (
                    <div className="flex flex-col items-center">
                        <span className="text-3xl font-bold bg-gradient-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                            {timeLeft.days}
                        </span>
                        <span className="text-[10px] uppercase tracking-wider text-slate-500">Days</span>
                    </div>
                )}
                {showDays && <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>}
                <div className="flex flex-col items-center">
                    <span className="text-3xl font-bold bg-gradient-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                        {formatUnit(timeLeft.hours)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Hours</span>
                </div>
                <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>
                <div className="flex flex-col items-center">
                    <span className="text-3xl font-bold bg-gradient-to-br from-slate-800 to-slate-600 dark:from-slate-200 dark:to-slate-400 bg-clip-text text-transparent">
                        {formatUnit(timeLeft.minutes)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Mins</span>
                </div>
                <span className="text-2xl text-slate-300 dark:text-slate-600 self-start mt-1">:</span>
                <div className="flex flex-col items-center">
                    <span className={`text-3xl font-bold ${isUrgent ? 'text-red-500' : 'bg-gradient-to-br from-purple-600 to-blue-600 bg-clip-text text-transparent'}`}>
                        {formatUnit(timeLeft.seconds)}
                    </span>
                    <span className="text-[10px] uppercase tracking-wider text-slate-500">Secs</span>
                </div>
            </div>
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
    const [isImageExpanded, setIsImageExpanded] = useState(false);

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
                <div className="container py-16 flex flex-col items-center justify-center min-h-[60vh]">
                    <motion.div
                        animate={{ rotate: 360 }}
                        transition={{ duration: 2, repeat: Infinity, ease: "linear" }}
                        className="w-16 h-16 rounded-full bg-gradient-to-r from-purple-600 to-blue-600 flex items-center justify-center shadow-lg shadow-purple-500/25"
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
                    <motion.div
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                    >
                        <Alert variant="destructive" className="border-red-200 dark:border-red-800 bg-red-50 dark:bg-red-950/50">
                            <AlertDescription className="flex items-center gap-2">
                                <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                {isNotFound
                                    ? 'Auction not found. It may have been deleted or does not exist.'
                                    : 'Failed to load auction. Please try again.'}
                            </AlertDescription>
                        </Alert>
                        <div className="mt-4">
                            <Button asChild className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
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

    const isOwner = session?.user?.name === getAuctionSellerUsername(auction);
    const images = getAuctionImages();
    const currentImage = images[selectedImageIndex] || '/placeholder-car.jpg';
    const statusConfig = STATUS_CONFIG[auction.status] || STATUS_CONFIG[AuctionStatus.Inactive];
    const timeRemaining = getTimeRemaining(auction.auctionEnd);
    const isUrgent = timeRemaining && timeRemaining.days === 0 && timeRemaining.hours < 1;

    const navigateImage = (direction: 'prev' | 'next') => {
        if (direction === 'prev') {
            setSelectedImageIndex(prev => (prev === 0 ? images.length - 1 : prev - 1));
        } else {
            setSelectedImageIndex(prev => (prev === images.length - 1 ? 0 : prev + 1));
        }
    };

    return (
        <MainLayout>
            <div className="container py-8 max-w-7xl mx-auto">
                <motion.div
                    initial={{ opacity: 0, y: -10 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.3 }}
                >
                    <Breadcrumb className="mb-6">
                        <BreadcrumbList>
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href={ROUTES.HOME} className="text-slate-500 hover:text-purple-600 transition-colors">
                                        Home
                                    </Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbLink asChild>
                                    <Link href={ROUTES.AUCTIONS.LIST} className="text-slate-500 hover:text-purple-600 transition-colors">
                                        Auctions
                                    </Link>
                                </BreadcrumbLink>
                            </BreadcrumbItem>
                            {auction.categoryName && (
                                <>
                                    <BreadcrumbSeparator />
                                    <BreadcrumbItem>
                                        <BreadcrumbLink asChild>
                                            <Link 
                                                href={`${ROUTES.AUCTIONS.LIST}?category=${auction.categorySlug}`}
                                                className="text-slate-500 hover:text-purple-600 transition-colors"
                                            >
                                                {auction.categoryName}
                                            </Link>
                                        </BreadcrumbLink>
                                    </BreadcrumbItem>
                                </>
                            )}
                            <BreadcrumbSeparator />
                            <BreadcrumbItem>
                                <BreadcrumbPage className="font-medium text-slate-900 dark:text-white">
                                    {getAuctionTitle(auction)}
                                </BreadcrumbPage>
                            </BreadcrumbItem>
                        </BreadcrumbList>
                    </Breadcrumb>
                </motion.div>

                {isOwner && (
                    <motion.div
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="mb-6 flex items-center justify-between p-4 rounded-xl bg-gradient-to-r from-purple-50 to-blue-50 dark:from-purple-950/30 dark:to-blue-950/30 border border-purple-200/50 dark:border-purple-800/50"
                    >
                        <div className="flex items-center gap-3">
                            <div className="w-10 h-10 rounded-full bg-gradient-to-r from-purple-600 to-blue-600 flex items-center justify-center">
                                <FontAwesomeIcon icon={faUser} className="w-5 h-5 text-white" />
                            </div>
                            <span className="text-sm font-medium text-purple-700 dark:text-purple-300">
                                You are the owner of this auction
                            </span>
                        </div>
                        <div className="flex gap-2">
                            {auction.status === AuctionStatus.Inactive && (
                                <ActivateAuctionDialog
                                    auctionId={auction.id}
                                    auctionTitle={getAuctionTitle(auction)}
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
                                    auctionTitle={getAuctionTitle(auction)}
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
                            <DeleteAuctionDialog
                                auctionId={auction.id}
                                auctionTitle={getAuctionTitle(auction)}
                                redirectAfterDelete="/auctions"
                            />
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
                        <div className="relative rounded-2xl overflow-hidden bg-slate-100 dark:bg-slate-900 shadow-xl">
                            <div className="relative aspect-[4/3] w-full group">
                                <Image
                                    src={currentImage}
                                    alt={getAuctionTitle(auction)}
                                    fill
                                    className="object-cover transition-transform duration-500 group-hover:scale-105"
                                    priority
                                />
                                
                                <div className={`absolute top-4 left-4 px-4 py-2 rounded-full bg-gradient-to-r ${statusConfig.gradient} text-white text-sm font-semibold shadow-lg ${statusConfig.bgGlow} flex items-center gap-2`}>
                                    {statusConfig.pulse && (
                                        <span className="w-2 h-2 rounded-full bg-white animate-pulse" />
                                    )}
                                    {auction.status}
                                </div>

                                {auction.status === AuctionStatus.Live && isUrgent && (
                                    <div className="absolute top-4 right-4 px-3 py-1.5 rounded-full bg-gradient-to-r from-red-500 to-orange-500 text-white text-xs font-semibold shadow-lg animate-pulse flex items-center gap-1.5">
                                        <FontAwesomeIcon icon={faFire} className="w-3 h-3" />
                                        Ending Soon
                                    </div>
                                )}

                                {images.length > 1 && (
                                    <>
                                        <button
                                            onClick={() => navigateImage('prev')}
                                            className="absolute left-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all hover:scale-110"
                                        >
                                            <FontAwesomeIcon icon={faChevronLeft} className="w-4 h-4 text-slate-700 dark:text-white" />
                                        </button>
                                        <button
                                            onClick={() => navigateImage('next')}
                                            className="absolute right-4 top-1/2 -translate-y-1/2 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all hover:scale-110"
                                        >
                                            <FontAwesomeIcon icon={faChevronRight} className="w-4 h-4 text-slate-700 dark:text-white" />
                                        </button>
                                    </>
                                )}

                                <button
                                    onClick={() => setIsImageExpanded(!isImageExpanded)}
                                    className="absolute bottom-4 right-4 w-10 h-10 rounded-full bg-white/90 dark:bg-slate-900/90 backdrop-blur-sm shadow-lg flex items-center justify-center opacity-0 group-hover:opacity-100 transition-all hover:scale-110"
                                >
                                    <FontAwesomeIcon icon={faExpand} className="w-4 h-4 text-slate-700 dark:text-white" />
                                </button>

                                <div className="absolute bottom-4 left-4 px-3 py-1.5 rounded-full bg-black/50 backdrop-blur-sm text-white text-xs font-medium">
                                    {selectedImageIndex + 1} / {images.length || 1}
                                </div>
                            </div>
                        </div>

                        {images.length > 1 && (
                            <div className="flex gap-2 overflow-x-auto pb-2 scrollbar-thin">
                                {images.map((img, idx) => (
                                    <motion.button
                                        key={idx}
                                        whileHover={{ scale: 1.05 }}
                                        whileTap={{ scale: 0.95 }}
                                        onClick={() => setSelectedImageIndex(idx)}
                                        className={`relative w-20 h-20 flex-shrink-0 rounded-xl overflow-hidden transition-all ${
                                            selectedImageIndex === idx
                                                ? 'ring-2 ring-purple-500 ring-offset-2 ring-offset-white dark:ring-offset-slate-900'
                                                : 'opacity-60 hover:opacity-100'
                                        }`}
                                    >
                                        <Image
                                            src={img}
                                            alt={`${getAuctionTitle(auction)} - Image ${idx + 1}`}
                                            fill
                                            className="object-cover"
                                        />
                                    </motion.button>
                                ))}
                            </div>
                        )}
                        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardHeader className="pb-2">
                                <div className="flex items-start justify-between">
                                    <div>
                                        <CardTitle className="text-2xl font-bold bg-gradient-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
                                            {getAuctionTitle(auction)}
                                        </CardTitle>
                                        <CardDescription className="text-base mt-1">
                                            {getAuctionYearManufactured(auction) && `${getAuctionYearManufactured(auction)} `}
                                            {Object.values(getAuctionAttributes(auction)).join(' ')}
                                        </CardDescription>
                                    </div>
                                    {auction.isFeatured && (
                                        <Badge className="bg-gradient-to-r from-amber-500 to-orange-500 text-white border-0">
                                            <FontAwesomeIcon icon={faFire} className="w-3 h-3 mr-1" />
                                            Featured
                                        </Badge>
                                    )}
                                </div>
                            </CardHeader>
                            <CardContent>
                                <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                    {getAuctionAttributes(auction).mileage && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-500 to-cyan-500 flex items-center justify-center">
                                                <FontAwesomeIcon icon={faTachometerAlt} className="w-4 h-4 text-white" />
                                            </div>
                                            <div>
                                                <p className="text-xs text-slate-500">Mileage</p>
                                                <p className="font-semibold text-slate-900 dark:text-white">{getAuctionAttributes(auction).mileage}</p>
                                            </div>
                                        </div>
                                    )}
                                    {getAuctionAttributes(auction).color && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-500 to-pink-500 flex items-center justify-center">
                                                <FontAwesomeIcon icon={faPalette} className="w-4 h-4 text-white" />
                                            </div>
                                            <div>
                                                <p className="text-xs text-slate-500">Color</p>
                                                <p className="font-semibold text-slate-900 dark:text-white">{getAuctionAttributes(auction).color}</p>
                                            </div>
                                        </div>
                                    )}
                                    <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-emerald-500 to-green-500 flex items-center justify-center">
                                            <FontAwesomeIcon icon={faCalendarAlt} className="w-4 h-4 text-white" />
                                        </div>
                                        <div>
                                            <p className="text-xs text-slate-500">Year</p>
                                            <p className="font-semibold text-slate-900 dark:text-white">{getAuctionYearManufactured(auction) || 'N/A'}</p>
                                        </div>
                                    </div>
                                    {auction.categoryName && (
                                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                            <div className="w-10 h-10 rounded-full bg-gradient-to-br from-amber-500 to-orange-500 flex items-center justify-center">
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
                            <div className="absolute inset-x-0 top-0 h-1 bg-gradient-to-r from-purple-600 to-blue-600" />
                            <CardContent className="pt-8">
                                <div className="space-y-6">
                                    <div>
                                        <p className="text-sm text-slate-500 flex items-center gap-2">
                                            <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                            Current Price
                                        </p>
                                        <p className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent mt-1">
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

                                    <div className="flex items-center gap-2 text-sm text-slate-500">
                                        <FontAwesomeIcon icon={faGavel} className="w-4 h-4" />
                                        <span>Multiple bids placed</span>
                                    </div>

                                    <Separator className="bg-slate-200 dark:bg-slate-700" />

                                    {auction.status === AuctionStatus.Live && !isOwner && (
                                        <div className="space-y-4">
                                            <div className="flex gap-2">
                                                <div className="relative flex-1">
                                                    <FontAwesomeIcon icon={faDollarSign} className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
                                                    <Input
                                                        type="number"
                                                        value={bidAmount}
                                                        onChange={(e) => setBidAmount(e.target.value)}
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
                                    )}

                                    {auction.isBuyNowAvailable && auction.buyNowPrice && auction.status === AuctionStatus.Live && !isOwner && (
                                        <div className="space-y-3">
                                            <Separator className="bg-slate-200 dark:bg-slate-700" />
                                            <motion.div 
                                                initial={{ opacity: 0, scale: 0.95 }}
                                                animate={{ opacity: 1, scale: 1 }}
                                                className="rounded-xl bg-gradient-to-br from-amber-50 to-orange-50 dark:from-amber-950/30 dark:to-orange-950/30 p-5 border border-amber-200/50 dark:border-amber-800/50"
                                            >
                                                <div className="flex items-center gap-2 mb-2">
                                                    <FontAwesomeIcon icon={faBolt} className="w-4 h-4 text-amber-500" />
                                                    <p className="text-sm font-semibold text-amber-700 dark:text-amber-400">Buy Now Available!</p>
                                                </div>
                                                <p className="text-3xl font-bold text-amber-600 dark:text-amber-500 mb-4">
                                                    {formatCurrency(auction.buyNowPrice)}
                                                </p>
                                                <BuyNowButton
                                                    auctionId={auction.id}
                                                    buyNowPrice={auction.buyNowPrice}
                                                    auctionTitle={getAuctionTitle(auction)}
                                                    onSuccess={() => window.location.reload()}
                                                />
                                            </motion.div>
                                        </div>
                                    )}

                                    {getAuctionWinnerUsername(auction) && (
                                        <motion.div 
                                            initial={{ opacity: 0, y: 10 }}
                                            animate={{ opacity: 1, y: 0 }}
                                            className="rounded-xl bg-gradient-to-br from-emerald-50 to-green-50 dark:from-emerald-950/30 dark:to-green-950/30 p-5 border border-emerald-200/50 dark:border-emerald-800/50"
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
                                                        className={`w-12 h-12 rounded-xl flex items-center justify-center transition-all ${
                                                            isInWatchlist 
                                                                ? 'bg-red-100 dark:bg-red-950/50 text-red-500' 
                                                                : 'bg-slate-100 dark:bg-slate-800 text-slate-500 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-950/30'
                                                        }`}
                                                    >
                                                        <FontAwesomeIcon icon={faHeart} className={`w-5 h-5 ${isInWatchlist ? 'text-red-500' : ''}`} />
                                                    </motion.button>
                                                </TooltipTrigger>
                                                <TooltipContent>
                                                    {isInWatchlist ? 'Remove from Watchlist' : 'Add to Watchlist'}
                                                </TooltipContent>
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

                        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardHeader className="pb-3">
                                <CardTitle className="text-sm font-semibold flex items-center gap-2">
                                    <FontAwesomeIcon icon={faUser} className="w-4 h-4 text-purple-500" />
                                    Seller
                                </CardTitle>
                            </CardHeader>
                            <CardContent>
                                <div className="flex items-center gap-4">
                                    <div className="w-14 h-14 rounded-full bg-gradient-to-br from-purple-500 to-blue-600 flex items-center justify-center text-white font-bold text-xl shadow-lg shadow-purple-500/25">
                                        {getAuctionSellerUsername(auction).charAt(0).toUpperCase()}
                                    </div>
                                    <div>
                                        <p className="font-semibold text-slate-900 dark:text-white">{getAuctionSellerUsername(auction)}</p>
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

                        <Card className="border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                            <CardContent className="pt-6">
                                <div className="space-y-3">
                                    <div className="flex items-center justify-between p-3 rounded-xl bg-slate-50 dark:bg-slate-800/50">
                                        <span className="text-sm text-slate-500 flex items-center gap-2">
                                            <FontAwesomeIcon icon={faShieldAlt} className="w-4 h-4" />
                                            Reserve Price
                                        </span>
                                        <span className={`font-semibold ${
                                            auction.currentHighBid && auction.currentHighBid >= auction.reservePrice
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
                >
                    <Card className="mt-8 border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-xl">
                                <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-purple-500 to-blue-600 flex items-center justify-center">
                                    <FontAwesomeIcon icon={faTag} className="w-4 h-4 text-white" />
                                </div>
                                Description
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p className="text-sm leading-relaxed whitespace-pre-wrap text-slate-600 dark:text-slate-300">
                                {getAuctionDescription(auction)}
                            </p>
                        </CardContent>
                    </Card>
                </motion.div>

                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.4, delay: 0.3 }}
                >
                    <Card className="mt-6 border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2 text-xl">
                                <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-amber-500 to-orange-600 flex items-center justify-center">
                                    <FontAwesomeIcon icon={faHistory} className="w-4 h-4 text-white" />
                                </div>
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
                </motion.div>

                {relatedAuctions.length > 0 && (
                    <motion.div 
                        initial={{ opacity: 0, y: 20 }}
                        animate={{ opacity: 1, y: 0 }}
                        transition={{ duration: 0.4, delay: 0.4 }}
                        className="mt-8"
                    >
                        <div className="flex items-center gap-3 mb-6">
                            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500 to-green-600 flex items-center justify-center shadow-lg shadow-emerald-500/25">
                                <FontAwesomeIcon icon={faGavel} className="w-5 h-5 text-white" />
                            </div>
                            <h2 className="text-2xl font-bold bg-gradient-to-r from-slate-900 to-slate-600 dark:from-white dark:to-slate-300 bg-clip-text text-transparent">
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
                                                <div className="relative aspect-[4/3]">
                                                    <Image
                                                        src={relatedImage}
                                                        alt={related.title}
                                                        fill
                                                        className="object-cover transition-transform duration-500 group-hover:scale-105"
                                                    />
                                                    <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity" />
                                                </div>
                                                <CardContent className="p-4">
                                                    <h3 className="font-semibold truncate text-slate-900 dark:text-white group-hover:text-purple-600 dark:group-hover:text-purple-400 transition-colors">
                                                        {related.title}
                                                    </h3>
                                                    <p className="text-sm text-slate-500 mt-0.5">
                                                        {related.categoryName || 'Uncategorized'}
                                                    </p>
                                                    <div className="flex items-center justify-between mt-3">
                                                        <span className="font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
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
                                    <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-slate-600 to-slate-800 flex items-center justify-center">
                                        <FontAwesomeIcon icon={faHistory} className="w-4 h-4 text-white" />
                                    </div>
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
                    </motion.div>
                )}
            </div>
        </MainLayout>
    );
}
