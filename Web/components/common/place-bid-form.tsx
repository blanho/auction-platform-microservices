"use client";

import { useState, useCallback } from "react";
import { useForm } from "react-hook-form";
import { useSession } from "next-auth/react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/components/ui/dialog";
import { Gavel, Loader2 } from "lucide-react";
import { bidService } from "@/services/bid.service";
import { PlaceBidDto, BidStatus } from "@/types/bid";
import { toast } from "sonner";

interface PlaceBidFormProps {
    auctionId: string;
    currentHighBid?: number;
    reservePrice?: number;
    onBidPlaced?: () => void;
}

interface FormData {
    amount: number;
}

const BID_INCREMENT = 1;

function calculateMinimumBid(currentHighBid: number, reservePrice: number): number {
    return Math.max(currentHighBid + BID_INCREMENT, reservePrice);
}

function handleBidStatusNotification(status: string, amount: number): void {
    const formattedAmount = `$${amount.toLocaleString()}`;

    switch (status) {
        case BidStatus.Accepted:
            toast.success("Bid placed successfully!", {
                description: `Your bid of ${formattedAmount} has been accepted.`
            });
            break;
        case BidStatus.AcceptedBelowReserve:
            toast.warning("Bid accepted below reserve", {
                description: `Your bid of ${formattedAmount} is below the reserve price.`
            });
            break;
        case BidStatus.TooLow:
            toast.error("Bid too low", {
                description: "Your bid must be higher than the current highest bid."
            });
            break;
        default:
            toast.info(`Bid status: ${status}`);
    }
}


function SignInPromptButton() {
    return (
        <Button disabled className="w-full">
            <Gavel className="mr-2 h-4 w-4" />
            Sign in to bid
        </Button>
    );
}

interface SubmitButtonProps {
    isSubmitting: boolean;
}

function SubmitButton({ isSubmitting }: SubmitButtonProps) {
    return (
        <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
                <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Placing Bid...
                </>
            ) : (
                <>
                    <Gavel className="mr-2 h-4 w-4" />
                    Confirm Bid
                </>
            )}
        </Button>
    );
}

export function PlaceBidForm({
    auctionId,
    currentHighBid = 0,
    reservePrice = 0,
    onBidPlaced
}: PlaceBidFormProps) {
    const [open, setOpen] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const { status } = useSession();

    const minimumBid = calculateMinimumBid(currentHighBid, reservePrice);

    const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
        defaultValues: {
            amount: minimumBid
        }
    });

    const handleClose = useCallback(() => setOpen(false), []);

    const onSubmit = useCallback(async (data: FormData) => {
        if (status !== "authenticated") {
            toast.error("Please sign in to place a bid");
            return;
        }

        setIsSubmitting(true);
        try {
            const dto: PlaceBidDto = {
                auctionId,
                amount: data.amount
            };

            const bid = await bidService.placeBid(dto);
            handleBidStatusNotification(bid.status, data.amount);

            if (bid.status === BidStatus.Accepted || bid.status === BidStatus.AcceptedBelowReserve) {
                setOpen(false);
                reset();
                onBidPlaced?.();
            }
        } catch (error) {
            console.error("Failed to place bid:", error);
            toast.error("Failed to place bid", {
                description: "Please try again later."
            });
        } finally {
            setIsSubmitting(false);
        }
    }, [status, auctionId, reset, onBidPlaced]);

    if (status !== "authenticated") {
        return <SignInPromptButton />;
    }

    const showReservePrice = reservePrice > 0 && currentHighBid < reservePrice;

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
                <Button className="w-full">
                    <Gavel className="mr-2 h-4 w-4" />
                    Place Bid
                </Button>
            </DialogTrigger>
            <DialogContent className="sm:max-w-[425px]">
                <DialogHeader>
                    <DialogTitle>Place a Bid</DialogTitle>
                    <DialogDescription>
                        {currentHighBid > 0
                            ? `Current highest bid: $${currentHighBid.toLocaleString()}`
                            : "Be the first to bid on this auction!"}
                    </DialogDescription>
                </DialogHeader>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <div className="grid gap-4 py-4">
                        <div className="space-y-2">
                            <Label htmlFor="amount">Your Bid Amount ($)</Label>
                            <Input
                                id="amount"
                                type="number"
                                placeholder={`Minimum: $${minimumBid.toLocaleString()}`}
                                {...register("amount", {
                                    required: "Bid amount is required",
                                    valueAsNumber: true,
                                    min: {
                                        value: minimumBid,
                                        message: `Bid must be at least $${minimumBid.toLocaleString()}`
                                    }
                                })}
                            />
                            {errors.amount && (
                                <p className="text-sm text-red-500">{errors.amount.message}</p>
                            )}
                            <p className="text-xs text-gray-500">
                                Minimum bid: ${minimumBid.toLocaleString()}
                                {showReservePrice && (
                                    <span className="block mt-1">
                                        Reserve price: ${reservePrice.toLocaleString()}
                                    </span>
                                )}
                            </p>
                        </div>
                    </div>
                    <DialogFooter>
                        <Button type="button" variant="outline" onClick={handleClose}>
                            Cancel
                        </Button>
                        <SubmitButton isSubmitting={isSubmitting} />
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
