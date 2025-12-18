"use client";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faStar, faQuoteLeft, faCalendar, faCircleCheck } from "@fortawesome/free-solid-svg-icons";
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
                <FontAwesomeIcon icon={faCalendar} className="h-3 w-3" />
                {format(new Date(review.createdAt), "MMM d, yyyy")}
              </div>
            </div>
          </div>
          <div className="flex items-center gap-1">
            {[1, 2, 3, 4, 5].map((starNum) => (
              <FontAwesomeIcon
                key={starNum}
                icon={faStar}
                className={cn(
                  "h-4 w-4",
                  starNum <= review.rating
                    ? "text-amber-400"
                    : "text-zinc-200 dark:text-zinc-700"
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

        {review.sellerResponse && (
          <>
            <Separator className="my-3" />
            <div className="pl-4 border-l-2 border-amber-400">
              <div className="flex items-center gap-2 mb-2">
                <FontAwesomeIcon icon={faQuoteLeft} className="h-4 w-4 text-amber-500" />
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
              {[1, 2, 3, 4, 5].map((starNum) => (
                <FontAwesomeIcon
                  key={starNum}
                  icon={faStar}
                  className={cn(
                    "h-4 w-4",
                    starNum <= Math.round(summary.averageRating)
                      ? "text-amber-400"
                      : "text-zinc-200"
                  )}
                />
              ))}
            </div>
            <p className="text-sm text-zinc-500 mt-1">
              {getRatingLabel(summary.averageRating)}
            </p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
