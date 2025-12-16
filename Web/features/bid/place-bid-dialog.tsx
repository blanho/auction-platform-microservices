"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSpinner, faDollarSign, faGavel } from "@fortawesome/free-solid-svg-icons";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { bidService } from "@/services/bid.service";
import { BidStatus } from "@/types/bid";
import { AuthDialog } from "@/features/auth";
import { useAuthSession } from "@/hooks/use-auth-session";

interface PlaceBidDialogProps {
  auctionId: string;
  currentHighBid: number;
  reservePrice: number;
  onBidPlaced?: () => void;
  trigger?: React.ReactNode;
}

const MIN_BID_INCREMENT = 100;

function createBidSchema(minBid: number) {
  return z.object({
    amount: z.number().min(minBid, `Minimum bid is $${minBid.toLocaleString()}`),
  });
}

type BidFormValues = z.infer<ReturnType<typeof createBidSchema>>;

export function PlaceBidDialog({
  auctionId,
  currentHighBid,
  reservePrice,
  onBidPlaced,
  trigger,
}: PlaceBidDialogProps) {
  const { isAuthenticated, isLoading: sessionLoading } = useAuthSession();
  const [isOpen, setIsOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const minBid = currentHighBid > 0 
    ? currentHighBid + MIN_BID_INCREMENT 
    : reservePrice > 0 
      ? reservePrice 
      : MIN_BID_INCREMENT;

  const bidSchema = createBidSchema(minBid);

  const form = useForm<BidFormValues>({
    resolver: zodResolver(bidSchema),
    defaultValues: {
      amount: minBid,
    },
  });

  const handleOpenChange = (open: boolean) => {
    if (open) {
      form.reset({ amount: minBid });
    }
    setIsOpen(open);
  };

  const onSubmit = async (values: BidFormValues) => {
    setIsSubmitting(true);
    try {
      const result = await bidService.placeBid({
        auctionId,
        amount: values.amount,
      });

      switch (result.status) {
        case BidStatus.Accepted:
          toast.success("Bid placed successfully!", {
            description: `Your bid of $${values.amount.toLocaleString()} is now the highest bid.`,
          });
          break;
        case BidStatus.AcceptedBelowReserve:
          toast.warning("Bid accepted below reserve", {
            description: `Your bid of $${values.amount.toLocaleString()} is below the reserve price.`,
          });
          break;
        case BidStatus.TooLow:
          toast.error("Bid too low", {
            description: "Your bid must be higher than the current highest bid.",
          });
          break;
        default:
          toast.info("Bid submitted", {
            description: `Your bid of $${values.amount.toLocaleString()} has been submitted.`,
          });
      }

      setIsOpen(false);
      onBidPlaced?.();
    } catch (error) {
      const message = error instanceof Error ? error.message : "Failed to place bid";
      toast.error("Failed to place bid", { description: message });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (sessionLoading) {
    return (
      <Button disabled size="lg" className="h-12 flex-shrink-0">
        <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
        Loading...
      </Button>
    );
  }

  if (!isAuthenticated) {
    return (
      <AuthDialog
        trigger={
          <Button size="lg" className="h-12 flex-shrink-0 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
            Sign in to place a bid
          </Button>
        }
      />
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        {trigger || (
          <Button size="lg" className="h-12 flex-shrink-0 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700">
            <FontAwesomeIcon icon={faGavel} className="mr-2 h-4 w-4" />
            Place Bid
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FontAwesomeIcon icon={faGavel} className="h-5 w-5" />
            Place Your Bid
          </DialogTitle>
          <DialogDescription>
            Enter your bid amount. Minimum bid is ${minBid.toLocaleString()}.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div className="rounded-lg bg-muted p-3">
                  <p className="text-muted-foreground">Current High Bid</p>
                  <p className="text-lg font-semibold">
                    ${currentHighBid.toLocaleString()}
                  </p>
                </div>
                <div className="rounded-lg bg-muted p-3">
                  <p className="text-muted-foreground">Reserve Price</p>
                  <p className="text-lg font-semibold">
                    ${reservePrice.toLocaleString()}
                  </p>
                </div>
              </div>

              <FormField
                control={form.control}
                name="amount"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Your Bid</FormLabel>
                    <FormControl>
                      <div className="relative">
                        <FontAwesomeIcon icon={faDollarSign} className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                        <Input
                          type="number"
                          placeholder={minBid.toString()}
                          className="pl-9"
                          {...field}
                          onChange={(e) => field.onChange(Number(e.target.value))}
                        />
                      </div>
                    </FormControl>
                    <FormDescription>
                      Minimum increment: ${MIN_BID_INCREMENT.toLocaleString()}
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => setIsOpen(false)}
                disabled={isSubmitting}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                    Placing Bid...
                  </>
                ) : (
                  <>
                    <FontAwesomeIcon icon={faGavel} className="mr-2 h-4 w-4" />
                    Place Bid
                  </>
                )}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
