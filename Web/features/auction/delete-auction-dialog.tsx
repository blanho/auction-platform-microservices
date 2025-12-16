'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faSpinner, faTrash } from '@fortawesome/free-solid-svg-icons';

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
import { Button } from '@/components/ui/button';

import { auctionService } from '@/services/auction.service';

interface DeleteAuctionDialogProps {
    auctionId: string;
    auctionTitle: string;
    redirectAfterDelete?: string;
    onSuccess?: () => void;
    trigger?: React.ReactNode;
    isAdmin?: boolean;
}

export function DeleteAuctionDialog({
    auctionId,
    auctionTitle,
    redirectAfterDelete,
    onSuccess,
    trigger,
    isAdmin = false,
}: DeleteAuctionDialogProps) {
    const router = useRouter();
    const [open, setOpen] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    const handleDelete = async () => {
        setIsDeleting(true);
        try {
            if (isAdmin) {
                await auctionService.adminDeleteAuction(auctionId);
            } else {
                await auctionService.deleteAuction(auctionId);
            }
            toast.success('Auction deleted successfully');
            setOpen(false);
            onSuccess?.();
            if (redirectAfterDelete) {
                router.push(redirectAfterDelete);
            }
        } catch (error) {
            const err = error as { response?: { data?: { message?: string } } };
            toast.error(err?.response?.data?.message || 'Failed to delete auction');
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <AlertDialog open={open} onOpenChange={setOpen}>
            <AlertDialogTrigger asChild>
                {trigger || (
                    <Button variant="destructive" size="sm">
                        <FontAwesomeIcon icon={faTrash} className="mr-2 h-4 w-4" />
                        Delete
                    </Button>
                )}
            </AlertDialogTrigger>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>Are you absolutely sure?</AlertDialogTitle>
                    <AlertDialogDescription>
                        This will permanently delete the auction{' '}
                        <span className="font-semibold">&quot;{auctionTitle}&quot;</span>. This
                        action cannot be undone.
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                    <AlertDialogCancel disabled={isDeleting}>
                        Cancel
                    </AlertDialogCancel>
                    <AlertDialogAction
                        onClick={(e) => {
                            e.preventDefault();
                            handleDelete();
                        }}
                        disabled={isDeleting}
                        className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                    >
                        {isDeleting && (
                            <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                        )}
                        Delete Auction
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}
