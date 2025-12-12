'use client';

import Link from 'next/link';
import Image from 'next/image';
import { Card, CardContent, CardFooter, CardHeader } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Auction, AuctionStatus } from '@/types/auction';
import { SearchItem } from '@/types/search';
import { formatDistance } from 'date-fns';

interface AuctionCardProps {
    auction: Auction | SearchItem;
}

export function AuctionCard({ auction }: AuctionCardProps) {
    const getStatusColor = (status: string) => {
        const statusUpper = status.toUpperCase();
        switch (statusUpper) {
            case 'LIVE':
            case AuctionStatus.Live.toUpperCase():
                return 'bg-green-500';
            case 'FINISHED':
            case AuctionStatus.Finished.toUpperCase():
                return 'bg-gray-500';
            case 'RESERVENOTMET':
            case 'RESERVE_NOT_MET':
            case AuctionStatus.ReserveNotMet.toUpperCase():
                return 'bg-yellow-500';
            case 'CANCELLED':
            case AuctionStatus.Cancelled.toUpperCase():
                return 'bg-red-500';
            default:
                return 'bg-gray-500';
        }
    };

    const getTimeRemaining = () => {
        if (!('auctionEnd' in auction) || !auction.auctionEnd) return 'Date not set';
        const endDate = new Date(auction.auctionEnd);
        if (isNaN(endDate.getTime())) return 'Invalid date';
        return formatDistance(endDate, new Date(), { addSuffix: true });
    };

    const getImageUrl = () => {
        // SearchItem has imageUrl directly
        if ('imageUrl' in auction && auction.imageUrl) {
            return auction.imageUrl;
        }
        // Auction has files array
        if ('files' in auction && auction.files) {
            const primaryFile = auction.files.find(f => f.isPrimary);
            return primaryFile?.url || auction.files[0]?.url;
        }
        return undefined;
    };

    const imageUrl = getImageUrl();
    const timeRemaining = getTimeRemaining();

    return (
        <Card className="overflow-hidden transition-all hover:shadow-lg">
            <CardHeader className="p-0">
                <div className="relative aspect-video w-full overflow-hidden bg-muted">
                    {imageUrl ? (
                        <Image
                            src={imageUrl}
                            alt={auction.title || 'Auction item'}
                            fill
                            className="object-cover"
                        />
                    ) : (
                        <div className="flex h-full items-center justify-center text-muted-foreground">
                            No image
                        </div>
                    )}
                    <Badge className={`absolute right-2 top-2 ${getStatusColor(auction.status)}`}>
                        {auction.status}
                    </Badge>
                </div>
            </CardHeader>
            <CardContent className="p-4">
                <h3 className="mb-2 text-lg font-semibold line-clamp-1">{auction.title || 'Untitled'}</h3>
                <div className="space-y-1 text-sm">
                    {'make' in auction && auction.make && (
                        <p className="text-muted-foreground">
                            {auction.year} {auction.make} {auction.model}
                        </p>
                    )}
                    {'description' in auction && auction.description && (
                        <p className="text-muted-foreground text-xs line-clamp-2">
                            {auction.description}
                        </p>
                    )}
                    <p className="font-semibold">
                        {'currentHighBid' in auction
                            ? `Current Bid: $${auction.currentHighBid?.toLocaleString() || '0'}`
                            : `Price: $${('price' in auction ? auction.price : 0).toLocaleString()}`}
                    </p>
                    {'reservePrice' in auction && auction.reservePrice && (
                        <p className="text-xs text-muted-foreground">
                            Reserve: ${auction.reservePrice.toLocaleString()}
                        </p>
                    )}
                    {'auctionEnd' in auction && auction.auctionEnd && (
                        <p className="text-xs text-muted-foreground">Ends {timeRemaining}</p>
                    )}
                </div>
            </CardContent>
            <CardFooter className="p-4 pt-0">
                <Button asChild className="w-full">
                    <Link href={`/auctions/${auction.id}`}>View Details</Link>
                </Button>
            </CardFooter>
        </Card>
    );
}
