"use client";

import { useState, useEffect } from "react";
import { useSession } from "next-auth/react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import {
  Loader2,
  DollarSign,
  Bot,
  CheckCircle2,
  Info,
  Trash2,
} from "lucide-react";
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
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import {
  autoBidService,
  AutoBid,
  CreateAutoBidDto,
} from "@/services/autobid.service";
import { AuthDialog } from "@/features/auth";

interface AutoBidDialogProps {
  auctionId: string;
  currentHighBid: number;
  reservePrice: number;
  trigger?: React.ReactNode;
}

const MIN_BID_INCREMENT = 100;

function createAutoBidSchema(minAmount: number) {
  return z.object({
    maxAmount: z
      .number()
      .min(minAmount, `Minimum auto-bid amount is $${minAmount.toLocaleString()}`),
  });
}

type AutoBidFormValues = z.infer<ReturnType<typeof createAutoBidSchema>>;

export function AutoBidDialog({
  auctionId,
  currentHighBid,
  reservePrice,
  trigger,
}: AutoBidDialogProps) {
  const { data: session, status: sessionStatus } = useSession();
  const [isOpen, setIsOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [existingAutoBid, setExistingAutoBid] = useState<AutoBid | null>(null);
  const [isCancelling, setIsCancelling] = useState(false);

  const minAmount =
    currentHighBid > 0
      ? currentHighBid + MIN_BID_INCREMENT * 2
      : reservePrice > 0
        ? reservePrice + MIN_BID_INCREMENT
        : MIN_BID_INCREMENT * 2;

  const autoBidSchema = createAutoBidSchema(minAmount);

  const form = useForm<AutoBidFormValues>({
    resolver: zodResolver(autoBidSchema),
    defaultValues: {
      maxAmount: minAmount,
    },
  });

  useEffect(() => {
    if (isOpen && session?.user) {
      setIsLoading(true);
      autoBidService
        .getMyAutoBidForAuction(auctionId)
        .then((autoBid) => {
          setExistingAutoBid(autoBid);
          if (autoBid) {
            form.setValue("maxAmount", autoBid.maxAmount);
          }
        })
        .catch(() => {
          setExistingAutoBid(null);
        })
        .finally(() => {
          setIsLoading(false);
        });
    }
  }, [isOpen, auctionId, session?.user, form]);

  const handleOpenChange = (open: boolean) => {
    if (open) {
      form.reset({ maxAmount: existingAutoBid?.maxAmount || minAmount });
    }
    setIsOpen(open);
  };

  const onSubmit = async (values: AutoBidFormValues) => {
    setIsSubmitting(true);
    try {
      if (existingAutoBid) {
        await autoBidService.updateAutoBid(existingAutoBid.id, {
          maxAmount: values.maxAmount,
          isActive: true,
        });
        toast.success("Auto-bid updated!", {
          description: `Your maximum bid has been updated to $${values.maxAmount.toLocaleString()}.`,
        });
      } else {
        const dto: CreateAutoBidDto = {
          auctionId,
          maxAmount: values.maxAmount,
        };
        await autoBidService.createAutoBid(dto);
        toast.success("Auto-bid activated!", {
          description: `Auto-bidding will automatically bid up to $${values.maxAmount.toLocaleString()}.`,
        });
      }

      setIsOpen(false);
      const updatedAutoBid = await autoBidService.getMyAutoBidForAuction(
        auctionId
      );
      setExistingAutoBid(updatedAutoBid);
    } catch (error) {
      toast.error("Failed to set auto-bid", {
        description:
          error instanceof Error
            ? error.message
            : "Please try again.",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelAutoBid = async () => {
    if (!existingAutoBid) return;

    setIsCancelling(true);
    try {
      await autoBidService.cancelAutoBid(existingAutoBid.id);
      toast.success("Auto-bid cancelled", {
        description: "Your auto-bid has been deactivated.",
      });
      setExistingAutoBid(null);
      setIsOpen(false);
    } catch (error) {
      toast.error("Failed to cancel auto-bid", {
        description:
          error instanceof Error
            ? error.message
            : "Please try again.",
      });
    } finally {
      setIsCancelling(false);
    }
  };

  const isAuthenticated = sessionStatus === "authenticated";

  if (!isAuthenticated) {
    return (
      <AuthDialog
        trigger={
          trigger || (
            <Button variant="outline" className="gap-2">
              <Bot className="h-4 w-4" />
              Set Auto-Bid
            </Button>
          )
        }
      />
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        {trigger || (
          <Button
            variant="outline"
            className="gap-2"
          >
            <Bot className="h-4 w-4" />
            {existingAutoBid?.isActive ? "Update Auto-Bid" : "Set Auto-Bid"}
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <div className="w-10 h-10 rounded-full bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
              <Bot className="h-5 w-5 text-amber-600" />
            </div>
            <div>
              <DialogTitle>
                {existingAutoBid ? "Manage Auto-Bid" : "Set Up Auto-Bid"}
              </DialogTitle>
              <DialogDescription>
                Let our system bid automatically for you
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
          </div>
        ) : (
          <>
            <Alert className="border-amber-200 bg-amber-50 dark:bg-amber-950/30">
              <Info className="h-4 w-4 text-amber-600" />
              <AlertTitle className="text-amber-800 dark:text-amber-200">
                How Auto-Bid Works
              </AlertTitle>
              <AlertDescription className="text-amber-700 dark:text-amber-300 text-sm">
                Set your maximum amount and our system will automatically place
                bids on your behalf, using the minimum increment needed to
                stay ahead, up to your maximum.
              </AlertDescription>
            </Alert>

            {existingAutoBid && (
              <div className="p-4 bg-zinc-50 dark:bg-zinc-900 rounded-lg space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-zinc-600 dark:text-zinc-400">
                    Current Auto-Bid
                  </span>
                  <Badge
                    variant={existingAutoBid.isActive ? "default" : "secondary"}
                    className={
                      existingAutoBid.isActive
                        ? "bg-green-500"
                        : "bg-zinc-500"
                    }
                  >
                    {existingAutoBid.isActive ? "Active" : "Inactive"}
                  </Badge>
                </div>
                <div className="flex items-baseline justify-between">
                  <span className="text-sm text-zinc-600 dark:text-zinc-400">
                    Maximum Amount
                  </span>
                  <span className="text-lg font-bold text-zinc-900 dark:text-white">
                    ${existingAutoBid.maxAmount.toLocaleString()}
                  </span>
                </div>
                <div className="flex items-baseline justify-between">
                  <span className="text-sm text-zinc-600 dark:text-zinc-400">
                    Current Bid Placed
                  </span>
                  <span className="text-sm font-medium text-zinc-700 dark:text-zinc-300">
                    ${existingAutoBid.currentBidAmount.toLocaleString()}
                  </span>
                </div>
              </div>
            )}

            <Separator />

            <Form {...form}>
              <form
                onSubmit={form.handleSubmit(onSubmit)}
                className="space-y-4"
              >
                <div className="grid grid-cols-2 gap-4 text-sm">
                  <div className="p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg">
                    <p className="text-zinc-500 dark:text-zinc-400">
                      Current High Bid
                    </p>
                    <p className="font-semibold text-zinc-900 dark:text-white">
                      ${currentHighBid.toLocaleString()}
                    </p>
                  </div>
                  <div className="p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg">
                    <p className="text-zinc-500 dark:text-zinc-400">
                      Minimum Auto-Bid
                    </p>
                    <p className="font-semibold text-zinc-900 dark:text-white">
                      ${minAmount.toLocaleString()}
                    </p>
                  </div>
                </div>

                <FormField
                  control={form.control}
                  name="maxAmount"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="flex items-center gap-2">
                        Your Maximum Amount
                        <TooltipProvider>
                          <Tooltip>
                            <TooltipTrigger type="button">
                              <Info className="h-4 w-4 text-zinc-400" />
                            </TooltipTrigger>
                            <TooltipContent className="max-w-[200px]">
                              <p>
                                This is the highest amount you are willing to
                                pay. Other bidders won&apos;t see this amount.
                              </p>
                            </TooltipContent>
                          </Tooltip>
                        </TooltipProvider>
                      </FormLabel>
                      <FormControl>
                        <div className="relative">
                          <DollarSign className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-zinc-400" />
                          <Input
                            type="number"
                            placeholder={minAmount.toString()}
                            className="pl-9"
                            {...field}
                            onChange={(e) =>
                              field.onChange(
                                e.target.value
                                  ? parseFloat(e.target.value)
                                  : ""
                              )
                            }
                          />
                        </div>
                      </FormControl>
                      <FormDescription>
                        Set the maximum you&apos;re willing to pay for this item.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <DialogFooter className="flex-col sm:flex-row gap-2">
                  {existingAutoBid && (
                    <Button
                      type="button"
                      variant="destructive"
                      onClick={handleCancelAutoBid}
                      disabled={isCancelling || isSubmitting}
                      className="w-full sm:w-auto"
                    >
                      {isCancelling ? (
                        <Loader2 className="h-4 w-4 animate-spin mr-2" />
                      ) : (
                        <Trash2 className="h-4 w-4 mr-2" />
                      )}
                      Cancel Auto-Bid
                    </Button>
                  )}
                  <Button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full sm:w-auto bg-amber-500 hover:bg-amber-600"
                  >
                    {isSubmitting ? (
                      <Loader2 className="h-4 w-4 animate-spin mr-2" />
                    ) : (
                      <CheckCircle2 className="h-4 w-4 mr-2" />
                    )}
                    {existingAutoBid ? "Update Auto-Bid" : "Activate Auto-Bid"}
                  </Button>
                </DialogFooter>
              </form>
            </Form>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
}
