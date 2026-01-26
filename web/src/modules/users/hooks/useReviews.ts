import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { reviewsApi } from '../api/reviews.api'
import type { CreateReviewRequest, SellerResponseRequest } from '../api/reviews.api'

export const reviewKeys = {
  all: ['reviews'] as const,
  detail: (id: string) => [...reviewKeys.all, 'detail', id] as const,
  auction: (auctionId: string) => [...reviewKeys.all, 'auction', auctionId] as const,
  forUser: (username: string) => [...reviewKeys.all, 'for-user', username] as const,
  byUser: (username: string) => [...reviewKeys.all, 'by-user', username] as const,
  userRating: (username: string) => [...reviewKeys.all, 'user-rating', username] as const,
}

export const useReview = (id: string) => {
  return useQuery({
    queryKey: reviewKeys.detail(id),
    queryFn: () => reviewsApi.getReviewById(id),
    enabled: !!id,
  })
}

export const useAuctionReviews = (auctionId: string) => {
  return useQuery({
    queryKey: reviewKeys.auction(auctionId),
    queryFn: () => reviewsApi.getReviewsForAuction(auctionId),
    enabled: !!auctionId,
  })
}

export const useReviewsForUser = (username: string) => {
  return useQuery({
    queryKey: reviewKeys.forUser(username),
    queryFn: () => reviewsApi.getReviewsForUser(username),
    enabled: !!username,
  })
}

export const useReviewsByUser = (username: string) => {
  return useQuery({
    queryKey: reviewKeys.byUser(username),
    queryFn: () => reviewsApi.getReviewsByUser(username),
    enabled: !!username,
  })
}

export const useUserRatingSummary = (username: string) => {
  return useQuery({
    queryKey: reviewKeys.userRating(username),
    queryFn: () => reviewsApi.getUserRatingSummary(username),
    enabled: !!username,
  })
}

export const useCreateReview = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateReviewRequest) => reviewsApi.createReview(data),
    onSuccess: (review) => {
      queryClient.invalidateQueries({ queryKey: reviewKeys.auction(review.auctionId) })
      queryClient.invalidateQueries({ queryKey: reviewKeys.forUser(review.reviewedUsername) })
      queryClient.invalidateQueries({ queryKey: reviewKeys.userRating(review.reviewedUsername) })
    },
  })
}

export const useAddSellerResponse = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ reviewId, data }: { reviewId: string; data: SellerResponseRequest }) =>
      reviewsApi.addSellerResponse(reviewId, data),
    onSuccess: (review) => {
      queryClient.invalidateQueries({ queryKey: reviewKeys.detail(review.id) })
      queryClient.invalidateQueries({ queryKey: reviewKeys.auction(review.auctionId) })
    },
  })
}
