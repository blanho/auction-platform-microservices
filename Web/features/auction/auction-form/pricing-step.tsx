"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faDollarSign, faClock } from "@fortawesome/free-solid-svg-icons";

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";

import { StepProps } from "./types";

function RequiredIndicator() {
  return <span className="text-red-500 ml-0.5">*</span>;
}

export function PricingStep({ form }: StepProps) {
  return (
    <div className="space-y-8">
      <div className="space-y-6">
        <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
          <FontAwesomeIcon icon={faDollarSign} className="h-5 w-5 text-green-500" />
          Pricing
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="reservePrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>
                  Starting Price
                  <RequiredIndicator />
                </FormLabel>
                <FormControl>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500">
                      $
                    </span>
                    <Input
                      type="number"
                      min={0}
                      step={0.01}
                      className="pl-7"
                      placeholder="0.00"
                      {...field}
                      onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                    />
                  </div>
                </FormControl>
                <FormDescription>
                  Minimum starting bid for your auction
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="buyNowPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Buy Now Price</FormLabel>
                <FormControl>
                  <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500">
                      $
                    </span>
                    <Input
                      type="number"
                      min={0}
                      step={0.01}
                      className="pl-7"
                      placeholder="0.00"
                      {...field}
                      value={field.value || ""}
                      onChange={(e) =>
                        field.onChange(parseFloat(e.target.value) || undefined)
                      }
                    />
                  </div>
                </FormControl>
                <FormDescription>
                  Optional instant purchase price (must be higher than starting price)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
      </div>

      <div className="space-y-6">
        <div className="flex items-center gap-2 text-lg font-semibold text-slate-900 dark:text-white">
          <FontAwesomeIcon icon={faClock} className="h-5 w-5 text-blue-500" />
          Auction Duration
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="auctionEnd"
            render={({ field }) => (
              <FormItem>
                <FormLabel>
                  End Date & Time
                  <RequiredIndicator />
                </FormLabel>
                <FormControl>
                  <Input
                    type="datetime-local"
                    min={new Date().toISOString().slice(0, 16)}
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  When the auction will end (minimum 1 hour from now)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>
      </div>
    </div>
  );
}
