"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCirclePause, faSpinner } from "@fortawesome/free-solid-svg-icons";
import { auctionService } from "@/services/auction.service";
import { toast } from "sonner";

interface DeactivateAuctionDialogProps {
  auctionId: string;
  auctionTitle: string;
  onSuccess?: () => void;
  trigger?: React.ReactNode;
}

export function DeactivateAuctionDialog({
  auctionId,
  auctionTitle,
  onSuccess,
  trigger
}: DeactivateAuctionDialogProps) {
  const [isLoading, setIsLoading] = useState(false);
  const [open, setOpen] = useState(false);
  const [reason, setReason] = useState("");

  const handleDeactivate = async () => {
    setIsLoading(true);
    try {
      await auctionService.deactivateAuction(auctionId, reason || undefined);
      toast.success(`"${auctionTitle}" has been deactivated.`);
      setOpen(false);
      setReason("");
      onSuccess?.();
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Failed to deactivate auction";
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setOpen(false);
    setReason("");
  };

  return (
    <Dialog open={open} onOpenChange={(isOpen) => (isOpen ? setOpen(true) : handleClose())}>
      <DialogTrigger asChild>
        {trigger || (
          <Button variant="outline" size="sm" className="text-orange-600 hover:text-orange-700">
            <FontAwesomeIcon icon={faCirclePause} className="mr-2 h-4 w-4" />
            Deactivate
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-orange-600">
            <FontAwesomeIcon icon={faCirclePause} className="h-5 w-5" />
            Deactivate Auction
          </DialogTitle>
          <DialogDescription>
            Deactivating &quot;{auctionTitle}&quot; will hide it from public view and pause all bidding.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="reason">Reason (Optional)</Label>
            <Textarea
              id="reason"
              placeholder="Provide a reason for deactivation..."
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              rows={3}
            />
            <p className="text-xs text-muted-foreground">
              This reason will be recorded in the audit log.
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={handleClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={handleDeactivate}
            disabled={isLoading}
            className="bg-orange-600 hover:bg-orange-700"
          >
            {isLoading ? (
              <>
                <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                Deactivating...
              </>
            ) : (
              <>
                <FontAwesomeIcon icon={faCirclePause} className="mr-2 h-4 w-4" />
                Deactivate
              </>
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
