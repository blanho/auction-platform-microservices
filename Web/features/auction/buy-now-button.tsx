'use client';

import { useState } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSpinner, faBagShopping } from '@fortawesome/free-solid-svg-icons';
import { useSession } from 'next-auth/react';
import { useRouter } from 'next/navigation';

import { Button } from '@/components/ui/button';
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
    AlertDialogTrigger,
} from '@/components/ui/alert-dialog';
import { toast } from 'sonner';

import { auctionService } from '@/services/auction.service';
import { formatCurrency } from '@/utils';
import { ROUTES } from '@/constants';

interface BuyNowButtonProps {
    auctionId: string;
    buyNowPrice: number;
    auctionTitle: string;
    onSuccess?: () => void;
}

export function BuyNowButton({
    auctionId,
    buyNowPrice,
    auctionTitle,
    onSuccess,
}: BuyNowButtonProps) {
    const { data: session } = useSession();
    const router = useRouter();
    const [isLoading, setIsLoading] = useState(false);
    const [isOpen, setIsOpen] = useState(false);

    const handleBuyNow = async () => {
        if (!session) {
            toast.error('Please sign in to use Buy Now');
            router.push(ROUTES.AUTH.LOGIN);
            return;
        }

        setIsLoading(true);
        try {
            const result = await auctionService.buyNow(auctionId);
            if (result.isSuccess) {
                toast.success('Purchase successful! You now own this item.');
                setIsOpen(false);
                onSuccess?.();
            } else {
                toast.error(result.message || 'Failed to complete purchase');
            }
        } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to complete purchase';
            toast.error(errorMessage);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AlertDialog open={isOpen} onOpenChange={setIsOpen}>
            <AlertDialogTrigger asChild>
                <Button
                    className="w-full bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600 text-white font-semibold"
                    size="lg"
                >
                    <FontAwesomeIcon icon={faBagShopping} className="mr-2 h-5 w-5" />
                    Buy Now
                </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Confirm Buy Now</AlertDialogTitle>
                    <AlertDialogDescription className="space-y-4">
                        <p>
                            You are about to purchase this item instantly for {formatCurrency(buyNowPrice)}.
                        </p>
                        <div className="rounded-lg bg-zinc-100 dark:bg-zinc-800 p-4">
                            <p className="font-medium text-zinc-900 dark:text-zinc-100">{auctionTitle}</p>
                            <p className="text-2xl font-bold text-amber-500 mt-1">{formatCurrency(buyNowPrice)}</p>
                        </div>
                        <p className="text-sm text-zinc-500">
                            This action cannot be undone. The auction will end immediately and you will be the winner.
                        </p>
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel disabled={isLoading}>Cancel</AlertDialogCancel>
                    <AlertDialogAction
                        onClick={handleBuyNow}
                        disabled={isLoading}
                        className="bg-gradient-to-r from-amber-500 to-orange-500 hover:from-amber-600 hover:to-orange-600"
                    >
                        {isLoading ? (
                            <>
                                <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                                Processing...
                            </>
                        ) : (
                            <>
                                <FontAwesomeIcon icon={faBagShopping} className="mr-2 h-4 w-4" />
                                Confirm Purchase
                            </>
                        )}
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}
