"use client";

import { useState, useEffect, useCallback } from "react";
import { useSession } from "next-auth/react";
import { format } from "date-fns";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faStar,
  faStarHalfAlt,
  faUser,
  faReply,
  faSpinner,
} from "@fortawesome/free-solid-svg-icons";
import { motion } from "framer-motion";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

import {
  reviewService,
  Review,
  CreateReviewDto,
  UserRatingSummary,
} from "@/services/review.service";

interface ReviewsSectionProps {
  auctionId: string;
  sellerId: string;
  sellerUsername: string;
  auctionStatus: string;
  winnerId?: string;
}

function StarRating({
  rating,
  onRatingChange,
  readonly = false,
  size = "md",
}: {
  rating: number;
  onRatingChange?: (rating: number) => void;
  readonly?: boolean;
  size?: "sm" | "md" | "lg";
}) {
  const [hoverRating, setHoverRating] = useState(0);
  const displayRating = hoverRating || rating;

  const sizeClasses = {
    sm: "w-3 h-3",
    md: "w-5 h-5",
    lg: "w-6 h-6",
  };

  return (
    <div className="flex items-center gap-0.5">
      {[1, 2, 3, 4, 5].map((star) => {
        const isFilled = displayRating >= star;
        const isHalf = !isFilled && displayRating >= star - 0.5;
        return (
          <button
            key={star}
            type="button"
            disabled={readonly}
            onClick={() => onRatingChange?.(star)}
            onMouseEnter={() => !readonly && setHoverRating(star)}
            onMouseLeave={() => !readonly && setHoverRating(0)}
            className={readonly ? "cursor-default" : "cursor-pointer hover:scale-110 transition-transform"}
          >
            <FontAwesomeIcon
              icon={isHalf ? faStarHalfAlt : faStar}
              className={`${sizeClasses[size]} ${
                isFilled || isHalf ? "text-yellow-400" : "text-slate-300 dark:text-slate-600"
              }`}
            />
          </button>
        );
      })}
    </div>
  );
}

function ReviewCard({ review, isOwner, onResponseAdded }: { review: Review; isOwner: boolean; onResponseAdded: () => void }) {
  const [showResponseForm, setShowResponseForm] = useState(false);
  const [response, setResponse] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmitResponse = async () => {
    if (!response.trim()) return;
    setIsSubmitting(true);
    try {
      await reviewService.addSellerResponse(review.id, response);
      toast.success("Response added successfully");
      setShowResponseForm(false);
      setResponse("");
      onResponseAdded();
    } catch {
      toast.error("Failed to add response");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      className="border-b border-slate-200 dark:border-slate-800 pb-4 last:border-0"
    >
      <div className="flex items-start gap-3">
        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-purple-500 to-blue-500 flex items-center justify-center shrink-0">
          <FontAwesomeIcon icon={faUser} className="w-4 h-4 text-white" />
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className="font-semibold text-slate-900 dark:text-white">
              {review.reviewerUsername}
            </span>
            <StarRating rating={review.rating} readonly size="sm" />
            <span className="text-xs text-slate-500">
              {format(new Date(review.createdAt), "MMM d, yyyy")}
            </span>
          </div>
          {review.title && (
            <p className="font-medium text-slate-800 dark:text-slate-200 mt-1">
              {review.title}
            </p>
          )}
          {review.comment && (
            <p className="text-sm text-slate-600 dark:text-slate-400 mt-1">
              {review.comment}
            </p>
          )}

          {review.sellerResponse && (
            <div className="mt-3 pl-4 border-l-2 border-purple-500 bg-purple-50 dark:bg-purple-900/20 p-3 rounded-r-lg">
              <div className="flex items-center gap-2 mb-1">
                <Badge variant="secondary" className="text-xs">Seller Response</Badge>
                {review.sellerResponseAt && (
                  <span className="text-xs text-slate-500">
                    {format(new Date(review.sellerResponseAt), "MMM d, yyyy")}
                  </span>
                )}
              </div>
              <p className="text-sm text-slate-600 dark:text-slate-400">
                {review.sellerResponse}
              </p>
            </div>
          )}

          {isOwner && !review.sellerResponse && (
            <>
              {showResponseForm ? (
                <div className="mt-3 space-y-2">
                  <Textarea
                    placeholder="Write your response..."
                    value={response}
                    onChange={(e) => setResponse(e.target.value)}
                    className="min-h-[80px]"
                  />
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      onClick={handleSubmitResponse}
                      disabled={isSubmitting || !response.trim()}
                    >
                      {isSubmitting && <FontAwesomeIcon icon={faSpinner} className="w-3 h-3 mr-2 animate-spin" />}
                      Submit Response
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setShowResponseForm(false)}
                    >
                      Cancel
                    </Button>
                  </div>
                </div>
              ) : (
                <Button
                  size="sm"
                  variant="ghost"
                  className="mt-2 text-purple-600 hover:text-purple-700"
                  onClick={() => setShowResponseForm(true)}
                >
                  <FontAwesomeIcon icon={faReply} className="w-3 h-3 mr-2" />
                  Respond
                </Button>
              )}
            </>
          )}
        </div>
      </div>
    </motion.div>
  );
}

function ReviewForm({
  auctionId,
  reviewedUserId,
  reviewedUsername,
  onSuccess,
}: {
  auctionId: string;
  reviewedUserId: string;
  reviewedUsername: string;
  onSuccess: () => void;
}) {
  const [rating, setRating] = useState(0);
  const [title, setTitle] = useState("");
  const [comment, setComment] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async () => {
    if (rating === 0) {
      toast.error("Please select a rating");
      return;
    }

    setIsSubmitting(true);
    try {
      const dto: CreateReviewDto = {
        auctionId,
        reviewedUserId,
        reviewedUsername,
        rating,
        title: title.trim() || undefined,
        comment: comment.trim() || undefined,
      };
      await reviewService.createReview(dto);
      toast.success("Review submitted successfully");
      setRating(0);
      setTitle("");
      setComment("");
      onSuccess();
    } catch {
      toast.error("Failed to submit review");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="border-purple-200 dark:border-purple-800 bg-purple-50/50 dark:bg-purple-900/10">
      <CardHeader className="pb-3">
        <CardTitle className="text-lg">Write a Review</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <label className="text-sm font-medium mb-2 block">Your Rating</label>
          <StarRating rating={rating} onRatingChange={setRating} size="lg" />
        </div>
        <div>
          <label className="text-sm font-medium mb-2 block">Title (optional)</label>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Summarize your experience"
            className="w-full px-3 py-2 rounded-lg border border-slate-200 dark:border-slate-700 bg-white dark:bg-slate-800"
          />
        </div>
        <div>
          <label className="text-sm font-medium mb-2 block">Your Review (optional)</label>
          <Textarea
            value={comment}
            onChange={(e) => setComment(e.target.value)}
            placeholder="Share your experience with this seller..."
            className="min-h-[100px]"
          />
        </div>
        <Button
          onClick={handleSubmit}
          disabled={isSubmitting || rating === 0}
          className="bg-gradient-to-r from-purple-600 to-blue-600 hover:from-purple-700 hover:to-blue-700"
        >
          {isSubmitting && <FontAwesomeIcon icon={faSpinner} className="w-4 h-4 mr-2 animate-spin" />}
          Submit Review
        </Button>
      </CardContent>
    </Card>
  );
}

export function ReviewsSection({
  auctionId,
  sellerId,
  sellerUsername,
  auctionStatus,
  winnerId,
}: ReviewsSectionProps) {
  const { data: session } = useSession();
  const [reviews, setReviews] = useState<Review[]>([]);
  const [summary, setSummary] = useState<UserRatingSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const currentUserId = session?.user?.id;
  const isOwner = currentUserId === sellerId;
  const isWinner = currentUserId === winnerId;
  const canReview = isWinner && auctionStatus === "Finished" && !reviews.some(r => r.reviewerUsername === session?.user?.name);

  const fetchData = useCallback(async () => {
    try {
      const [reviewsData, summaryData] = await Promise.all([
        reviewService.getReviewsForAuction(auctionId),
        reviewService.getUserRatingSummary(sellerUsername),
      ]);
      setReviews(reviewsData);
      setSummary(summaryData);
    } catch (error) {
    } finally {
      setIsLoading(false);
    }
  }, [auctionId, sellerUsername]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  if (isLoading) {
    return (
      <Card className="mt-6">
        <CardHeader>
          <Skeleton className="h-6 w-32" />
        </CardHeader>
        <CardContent className="space-y-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex gap-3">
              <Skeleton className="w-10 h-10 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-3 w-full" />
              </div>
            </div>
          ))}
        </CardContent>
      </Card>
    );
  }

  return (
    <Card className="mt-6 border-0 shadow-lg bg-white/80 dark:bg-slate-900/80 backdrop-blur-xl rounded-2xl">
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <div className="flex items-center gap-2 text-xl">
            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-yellow-500 to-orange-500 flex items-center justify-center">
              <FontAwesomeIcon icon={faStar} className="w-4 h-4 text-white" />
            </div>
            Seller Reviews
          </div>
          {summary && (
            <div className="flex items-center gap-2">
              <StarRating rating={summary.averageRating} readonly size="sm" />
              <span className="text-lg font-bold">{summary.averageRating.toFixed(1)}</span>
              <span className="text-sm text-slate-500">({summary.totalReviews} reviews)</span>
            </div>
          )}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {canReview && (
          <ReviewForm
            auctionId={auctionId}
            reviewedUserId={sellerId}
            reviewedUsername={sellerUsername}
            onSuccess={fetchData}
          />
        )}

        {reviews.length === 0 ? (
          <div className="text-center py-8 text-slate-500">
            <FontAwesomeIcon icon={faStar} className="w-12 h-12 mb-3 text-slate-300" />
            <p>No reviews yet</p>
          </div>
        ) : (
          <div className="space-y-4">
            {reviews.map((review) => (
              <ReviewCard
                key={review.id}
                review={review}
                isOwner={isOwner}
                onResponseAdded={fetchData}
              />
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
