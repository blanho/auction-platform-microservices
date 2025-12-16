"use client";

import { useState } from "react";
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
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCirclePlay, faSpinner } from "@fortawesome/free-solid-svg-icons";
import { auctionService } from "@/services/auction.service";
import { toast } from "sonner";

interface ActivateAuctionDialogProps {
  auctionId: string;
  auctionTitle: string;
  onSuccess?: () => void;
  trigger?: React.ReactNode;
}

export function ActivateAuctionDialog({
  auctionId,
  auctionTitle,
  onSuccess,
  trigger
}: ActivateAuctionDialogProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [open, setOpen] = useState(false);

  const handleActivate = async () => {
    setIsLoading(true);
    try {
      await auctionService.activateAuction(auctionId);
      toast.success(`"${auctionTitle}" has been activated successfully.`);
      setOpen(false);
      onSuccess?.();
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to activate auction";
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <AlertDialogTrigger asChild>
        {trigger || (
          <Button variant="outline" size="sm" className="text-green-600 hover:text-green-700">
            <FontAwesomeIcon icon={faCirclePlay} className="mr-2 h-4 w-4" />
            Activate
          </Button>
        )}
      </AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Activate Auction</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to activate &quot;{auctionTitle}&quot;?
            <br />
            <br />
            This will make the auction visible to all users and allow bidding to begin.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isLoading}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleActivate}
            disabled={isLoading}
            className="bg-green-600 hover:bg-green-700"
          >
            {isLoading ? (
              <>
                <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                Activating...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faCirclePlay} className="mr-2 h-4 w-4" />
                Activate
              </>
            )}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
