"use client";

import { Star, Quote, Calendar, CheckCircle2 } from "lucide-react";
import { format } from "date-fns";
import { cn } from "@/lib/utils";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import {
  Review,
  UserRatingSummary,
  getRatingColor,
  getRatingLabel,
} from "@/services/review.service";

interface ReviewCardProps {
  review: Review;
}

export function ReviewCard({ review }: ReviewCardProps) {
  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <Avatar className="h-10 w-10">
              <AvatarFallback>
                {review.reviewerUsername.charAt(0).toUpperCase()}
              </AvatarFallback>
            </Avatar>
            <div>
              <CardTitle className="text-base">
                {review.reviewerUsername}
              </CardTitle>
              <div className="flex items-center gap-2 text-sm text-zinc-500">
                <Calendar className="h-3 w-3" />
                {format(new Date(review.createdAt), "MMM d, yyyy")}
              </div>
            </div>
          </div>
          <div className="flex items-center gap-1">
            {[1, 2, 3, 4, 5].map((star) => (
              <Star
                key={star}
                className={cn(
                  "h-4 w-4",
                  star <= review.rating
                    ? "fill-amber-400 text-amber-400"
                    : "fill-zinc-200 text-zinc-200 dark:fill-zinc-700 dark:text-zinc-700"
                )}
              />
            ))}
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {review.title && (
          <h4 className="font-semibold text-zinc-900 dark:text-white">
            {review.title}
          </h4>
        )}
        {review.comment && (
          <p className="text-zinc-600 dark:text-zinc-400">{review.comment}</p>
        )}
        {review.isVerifiedPurchase && (
          <Badge
            variant="secondary"
            className="gap-1 text-green-600 bg-green-50 dark:bg-green-950/30"
          >
            <CheckCircle2 className="h-3 w-3" />
            Verified Purchase
          </Badge>
        )}

        {review.sellerResponse && (
          <>
            <Separator className="my-3" />
            <div className="pl-4 border-l-2 border-amber-400">
              <div className="flex items-center gap-2 mb-2">
                <Quote className="h-4 w-4 text-amber-500" />
                <span className="text-sm font-medium text-amber-600">
                  Seller Response
                </span>
                {review.sellerResponseAt && (
                  <span className="text-xs text-zinc-500">
                    {format(new Date(review.sellerResponseAt), "MMM d, yyyy")}
                  </span>
                )}
              </div>
              <p className="text-sm text-zinc-600 dark:text-zinc-400">
                {review.sellerResponse}
              </p>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

interface RatingSummaryCardProps {
  summary: UserRatingSummary;
}

export function RatingSummaryCard({ summary }: RatingSummaryCardProps) {
  const ratingBars = [
    { stars: 5, count: summary.fiveStarCount },
    { stars: 4, count: summary.fourStarCount },
    { stars: 3, count: summary.threeStarCount },
    { stars: 2, count: summary.twoStarCount },
    { stars: 1, count: summary.oneStarCount },
  ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">Seller Rating</CardTitle>
        <CardDescription>
          Based on {summary.totalReviews} reviews
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex items-center gap-4">
          <div className="text-center">
            <div className={cn("text-4xl font-bold", getRatingColor(summary.averageRating))}>
              {summary.averageRating.toFixed(1)}
            </div>
            <div className="flex items-center justify-center gap-0.5 mt-1">
              {[1, 2, 3, 4, 5].map((star) => (
                <Star
                  key={star}
                  className={cn(
                    "h-4 w-4",
                    star <= Math.round(summary.averageRating)
                      ? "fill-amber-400 text-amber-400"
                      : "fill-zinc-200 text-zinc-200"
                  )}
                />
              ))}
            </div>
            <p className="text-sm text-zinc-500 mt-1">
              {getRatingLabel(summary.averageRating)}
            </p>
          </div>

          <div className="flex-1 space-y-2">
            {ratingBars.map((bar) => {
              const percentage =
                summary.totalReviews > 0
                  ? (bar.count / summary.totalReviews) * 100
                  : 0;
              return (
                <div key={bar.stars} className="flex items-center gap-2">
                  <span className="text-sm text-zinc-500 w-3">{bar.stars}</span>
                  <Star className="h-3 w-3 fill-amber-400 text-amber-400" />
                  <div className="flex-1 h-2 bg-zinc-100 dark:bg-zinc-800 rounded-full overflow-hidden">
                    <div
                      className="h-full bg-amber-400 rounded-full transition-all"
                      style={{ width: `${percentage}%` }}
                    />
                  </div>
                  <span className="text-sm text-zinc-500 w-8 text-right">
                    {bar.count}
                  </span>
                </div>
              );
            })}
          </div>
        </div>

        <div className="flex items-center justify-center p-3 bg-green-50 dark:bg-green-950/30 rounded-lg">
          <CheckCircle2 className="h-5 w-5 text-green-500 mr-2" />
          <span className="text-green-700 dark:text-green-300 font-medium">
            {summary.positivePercentage.toFixed(0)}% Positive Reviews
          </span>
        </div>
      </CardContent>
    </Card>
  );
}
