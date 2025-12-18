"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSpinner, faDollarSign, faGavel, faShieldHalved, faArrowLeft, faCheckCircle } from "@fortawesome/free-solid-svg-icons";
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
import { Separator } from "@/components/ui/separator";
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
  auctionTitle?: string;
}

const MIN_BID_INCREMENT = 100;
const PLATFORM_FEE_PERCENT = 2.5;
const PENDING_BID_KEY = "pending_bid_intent";

type DialogStep = "input" | "confirm";

interface PendingBidIntent {
  auctionId: string;
  amount: number;
  timestamp: number;
}

function savePendingBidIntent(auctionId: string, amount: number): void {
  try {
    const intent: PendingBidIntent = {
      auctionId,
      amount,
      timestamp: Date.now(),
    };
    sessionStorage.setItem(PENDING_BID_KEY, JSON.stringify(intent));
  } catch {}
}

function getPendingBidIntent(auctionId: string): number | null {
  try {
    const stored = sessionStorage.getItem(PENDING_BID_KEY);
    if (!stored) return null;
    
    const intent: PendingBidIntent = JSON.parse(stored);
    const fiveMinutesAgo = Date.now() - 5 * 60 * 1000;
    
    if (intent.auctionId === auctionId && intent.timestamp > fiveMinutesAgo) {
      return intent.amount;
    }
    
    sessionStorage.removeItem(PENDING_BID_KEY);
    return null;
  } catch {
    return null;
  }
}

function clearPendingBidIntent(): void {
  try {
    sessionStorage.removeItem(PENDING_BID_KEY);
  } catch {}
}

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
  auctionTitle,
}: PlaceBidDialogProps) {
  const { isAuthenticated, isLoading: sessionLoading } = useAuthSession();
  const [isOpen, setIsOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [step, setStep] = useState<DialogStep>("input");
  const [pendingBidChecked, setPendingBidChecked] = useState(false);

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

  useEffect(() => {
    if (isAuthenticated && !pendingBidChecked) {
      setPendingBidChecked(true);
      const pendingAmount = getPendingBidIntent(auctionId);
      if (pendingAmount && pendingAmount >= minBid) {
        form.setValue("amount", pendingAmount);
        setIsOpen(true);
        clearPendingBidIntent();
        toast.info("Your bid amount has been restored", {
          description: "Continue where you left off",
        });
      }
    }
  }, [isAuthenticated, pendingBidChecked, auctionId, minBid, form]);

  const watchedAmount = form.watch("amount");
  const platformFee = (watchedAmount * PLATFORM_FEE_PERCENT) / 100;
  const meetsReserve = watchedAmount >= reservePrice;

  const handleOpenChange = (open: boolean) => {
    if (open) {
      form.reset({ amount: minBid });
      setStep("input");
    }
    setIsOpen(open);
  };

  const handleContinueToConfirm = async () => {
    const isValid = await form.trigger();
    if (isValid) {
      setStep("confirm");
    }
  };

  const handleBackToInput = () => {
    setStep("input");
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
    const handleAuthTriggerClick = () => {
      savePendingBidIntent(auctionId, minBid);
    };

    return (
      <AuthDialog
        trigger={
          <Button 
            size="lg" 
            className="h-12 flex-shrink-0 bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
            onClick={handleAuthTriggerClick}
          >
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
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)}>
            {step === "input" ? (
              <>
                <DialogHeader>
                  <DialogTitle className="flex items-center gap-2">
                    <FontAwesomeIcon icon={faGavel} className="h-5 w-5" />
                    Place Your Bid
                  </DialogTitle>
                  <DialogDescription>
                    Enter your bid amount. Minimum bid is ${minBid.toLocaleString()}.
                  </DialogDescription>
                </DialogHeader>

                <div className="space-y-4 py-4">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div className="rounded-lg bg-muted p-3">
                      <p className="text-muted-foreground text-xs">Current High Bid</p>
                      <p className="text-lg font-semibold">
                        ${currentHighBid.toLocaleString()}
                      </p>
                    </div>
                    <div className="rounded-lg bg-muted p-3">
                      <p className="text-muted-foreground text-xs">Reserve Price</p>
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
                              className="pl-9 text-lg h-12"
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
                  >
                    Cancel
                  </Button>
                  <Button type="button" onClick={handleContinueToConfirm}>
                    Continue
                  </Button>
                </DialogFooter>
              </>
            ) : (
              <>
                <DialogHeader>
                  <DialogTitle className="flex items-center gap-2">
                    <FontAwesomeIcon icon={faCheckCircle} className="h-5 w-5 text-green-500" />
                    Confirm Your Bid
                  </DialogTitle>
                  <DialogDescription>
                    Review the details before placing your bid.
                  </DialogDescription>
                </DialogHeader>

                <div className="space-y-4 py-4">
                  {auctionTitle && (
                    <div className="text-sm text-muted-foreground">
                      Bidding on: <span className="font-medium text-foreground">{auctionTitle}</span>
                    </div>
                  )}

                  <div className="rounded-xl border bg-gradient-to-br from-purple-50 to-blue-50 dark:from-purple-950/30 dark:to-blue-950/30 p-4 space-y-3">
                    <div className="flex justify-between items-center">
                      <span className="text-muted-foreground">Your Bid</span>
                      <span className="text-2xl font-bold">${watchedAmount.toLocaleString()}</span>
                    </div>
                    <Separator />
                    <div className="flex justify-between items-center text-sm">
                      <span className="text-muted-foreground">Platform Fee ({PLATFORM_FEE_PERCENT}%)</span>
                      <span className="font-medium">${platformFee.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between items-center text-sm">
                      <span className="text-muted-foreground">If you win, you pay</span>
                      <span className="font-semibold text-lg">${(watchedAmount + platformFee).toLocaleString()}</span>
                    </div>
                  </div>

                  {!meetsReserve && (
                    <div className="rounded-lg bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800 p-3 text-sm">
                      <p className="text-amber-700 dark:text-amber-400 flex items-center gap-2">
                        <FontAwesomeIcon icon={faShieldHalved} className="h-4 w-4" />
                        Below reserve price — seller may not accept
                      </p>
                    </div>
                  )}

                  {meetsReserve && (
                    <div className="rounded-lg bg-green-50 dark:bg-green-950/30 border border-green-200 dark:border-green-800 p-3 text-sm">
                      <p className="text-green-700 dark:text-green-400 flex items-center gap-2">
                        <FontAwesomeIcon icon={faCheckCircle} className="h-4 w-4" />
                        Meets reserve price — eligible to win
                      </p>
                    </div>
                  )}

                  <p className="text-xs text-muted-foreground text-center">
                    By placing this bid, you agree to pay if you win.
                  </p>
                </div>

                <DialogFooter className="flex-col sm:flex-row gap-2">
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={handleBackToInput}
                    disabled={isSubmitting}
                    className="w-full sm:w-auto"
                  >
                    <FontAwesomeIcon icon={faArrowLeft} className="mr-2 h-4 w-4" />
                    Back
                  </Button>
                  <Button 
                    type="submit" 
                    disabled={isSubmitting}
                    className="w-full sm:w-auto bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
                  >
                    {isSubmitting ? (
                      <>
                        <FontAwesomeIcon icon={faSpinner} className="mr-2 h-4 w-4 animate-spin" />
                        Placing Bid...
                      </>
                    ) : (
                      <>
                        <FontAwesomeIcon icon={faGavel} className="mr-2 h-4 w-4" />
                        Confirm & Place Bid
                      </>
                    )}
                  </Button>
                </DialogFooter>
              </>
            )}
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
