"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { Button, Input, Label, Spinner } from "@repo/ui";
import { placeBidSchema, type PlaceBidInput } from "@repo/validators";
import { usePlaceBid, useAuth } from "@repo/hooks";
import { formatCurrency, getMinBid } from "@repo/utils";

interface BidFormProps {
  auctionId: string;
  currentBid: number | null | undefined;
  reservePrice: number;
  bidIncrement: number;
}

export function BidForm({ auctionId, currentBid, reservePrice, bidIncrement }: BidFormProps) {
  const router = useRouter();
  const { user } = useAuth();
  const { mutate: placeBid, isPending, error } = usePlaceBid();
  const [bidError, setBidError] = useState<string | null>(null);

  const minBid = getMinBid(currentBid || null, reservePrice);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<PlaceBidInput>({
    resolver: zodResolver(placeBidSchema),
    defaultValues: {
      auctionId,
      amount: minBid,
    },
  });

  function onSubmit(data: PlaceBidInput) {
    if (!user) {
      router.push("/login");
      return;
    }

    setBidError(null);
    placeBid(data, {
      onSuccess: (result) => {
        if (result.success) {
          reset({ auctionId, amount: (result.newHighBid || data.amount) + bidIncrement });
        } else {
          setBidError(result.error || "Failed to place bid");
        }
      },
      onError: (err) => {
        setBidError(err.message || "Failed to place bid");
      },
    });
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {(bidError || error) && (
        <div className="rounded-md bg-destructive/10 p-3 text-sm text-destructive">
          {bidError || error?.message}
        </div>
      )}

      <div className="space-y-2">
        <Label htmlFor="amount">Your Bid</Label>
        <div className="relative">
          <span className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
            $
          </span>
          <Input
            id="amount"
            type="number"
            step="0.01"
            min={minBid}
            className="pl-7"
            {...register("amount", { valueAsNumber: true })}
            aria-invalid={!!errors.amount}
          />
        </div>
        {errors.amount && (
          <p className="text-sm text-destructive">{errors.amount.message}</p>
        )}
        <p className="text-xs text-muted-foreground">
          Minimum bid: {formatCurrency(minBid)}
        </p>
      </div>

      <Button type="submit" className="w-full" disabled={isPending}>
        {isPending ? <Spinner size="sm" /> : "Place Bid"}
      </Button>

      {!user && (
        <p className="text-center text-sm text-muted-foreground">
          You must be signed in to place a bid
        </p>
      )}
    </form>
  );
}
