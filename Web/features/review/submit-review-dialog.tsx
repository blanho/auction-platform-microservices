"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faStar, faSpinner, faComment, faThumbsUp } from "@fortawesome/free-solid-svg-icons";
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
import { Textarea } from "@/components/ui/textarea";
import { cn } from "@/lib/utils";
import { reviewService, CreateReviewDto } from "@/services/review.service";

interface SubmitReviewDialogProps {
  orderId: string;
  sellerUsername: string;
  itemTitle: string;
  onReviewSubmitted?: () => void;
  trigger?: React.ReactNode;
}

const reviewSchema = z.object({
  rating: z.number().min(1, "Please select a rating").max(5),
  title: z.string().max(100, "Title must be 100 characters or less").optional(),
  comment: z.string().max(1000, "Comment must be 1000 characters or less").optional(),
});

type ReviewFormValues = z.infer<typeof reviewSchema>;

const RATING_LABELS = [
  { value: 1, label: "Poor", description: "Very dissatisfied" },
  { value: 2, label: "Fair", description: "Somewhat dissatisfied" },
  { value: 3, label: "Good", description: "Neutral" },
  { value: 4, label: "Very Good", description: "Satisfied" },
  { value: 5, label: "Excellent", description: "Very satisfied" },
];

export function SubmitReviewDialog({
  orderId,
  sellerUsername,
  itemTitle,
  onReviewSubmitted,
  trigger,
}: SubmitReviewDialogProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [hoveredRating, setHoveredRating] = useState(0);

  const form = useForm<ReviewFormValues>({
    resolver: zodResolver(reviewSchema),
    defaultValues: {
      rating: 0,
      title: "",
      comment: "",
    },
  });

  const currentRating = form.watch("rating");
  const displayRating = hoveredRating || currentRating;
  const ratingInfo = RATING_LABELS.find((r) => r.value === displayRating);

  const handleOpenChange = (open: boolean) => {
    if (open) {
      form.reset();
      setHoveredRating(0);
    }
    setIsOpen(open);
  };

  const onSubmit = async (values: ReviewFormValues) => {
    setIsSubmitting(true);
    try {
      const dto: CreateReviewDto = {
        orderId,
        rating: values.rating,
        title: values.title || undefined,
        comment: values.comment || undefined,
      };

      await reviewService.createReview(dto);

      toast.success("Review submitted!", {
        description: "Thank you for your feedback.",
      });

      setIsOpen(false);
      onReviewSubmitted?.();
    } catch (error) {
      toast.error("Failed to submit review", {
        description:
          error instanceof Error ? error.message : "Please try again.",
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleOpenChange}>
      <DialogTrigger asChild>
        {trigger || (
          <Button variant="outline" className="gap-2">
            <FontAwesomeIcon icon={faStar} className="h-4 w-4" />
            Leave Review
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <div className="flex items-center gap-3">
            <div className="w-12 h-12 rounded-full bg-amber-100 dark:bg-amber-900/30 flex items-center justify-center">
              <FontAwesomeIcon icon={faComment} className="h-6 w-6 text-amber-600" />
            </div>
            <div>
              <DialogTitle>Leave a Review</DialogTitle>
              <DialogDescription>
                Share your experience with {sellerUsername}
              </DialogDescription>
            </div>
          </div>
        </DialogHeader>

        <div className="py-2">
          <div className="p-3 bg-zinc-50 dark:bg-zinc-900 rounded-lg mb-4">
            <p className="text-sm text-zinc-600 dark:text-zinc-400">Item</p>
            <p className="font-medium text-zinc-900 dark:text-white line-clamp-2">
              {itemTitle}
            </p>
          </div>

          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <FormField
                control={form.control}
                name="rating"
                render={({ field }) => (
                  <FormItem className="text-center">
                    <FormLabel>Your Rating</FormLabel>
                    <FormControl>
                      <div className="space-y-2">
                        <div className="flex justify-center gap-2">
                          {[1, 2, 3, 4, 5].map((value) => (
                            <button
                              key={value}
                              type="button"
                              onClick={() => field.onChange(value)}
                              onMouseEnter={() => setHoveredRating(value)}
                              onMouseLeave={() => setHoveredRating(0)}
                              className="p-1 transition-transform hover:scale-110 focus:outline-none"
                            >
                              <Star
                                className={cn(
                                  "h-10 w-10 transition-colors",
                                  value <= displayRating
                                    ? "fill-amber-400 text-amber-400"
                                    : "fill-zinc-200 text-zinc-200 dark:fill-zinc-700 dark:text-zinc-700"
                                )}
                              />
                            </button>
                          ))}
                        </div>
                        {ratingInfo && (
                          <div className="text-center">
                            <p className="font-medium text-amber-600">
                              {ratingInfo.label}
                            </p>
                            <p className="text-sm text-zinc-500">
                              {ratingInfo.description}
                            </p>
                          </div>
                        )}
                      </div>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Title (Optional)</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Summarize your experience"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="comment"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Your Review (Optional)</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Tell others about your experience with this seller. Was the item as described? How was the shipping?"
                        rows={4}
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      {field.value?.length || 0}/1000 characters
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setIsOpen(false)}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={isSubmitting || currentRating === 0}
                  className="bg-amber-500 hover:bg-amber-600"
                >
                  {isSubmitting ? (
                    <>
                      <FontAwesomeIcon icon={faSpinner} className="h-4 w-4 animate-spin mr-2" />
                      Submitting...
                    </>
                  ) : (
                    <>
                      <FontAwesomeIcon icon={faThumbsUp} className="h-4 w-4 mr-2" />
                      Submit Review
                    </>
                  )}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </div>
      </DialogContent>
    </Dialog>
  );
}
