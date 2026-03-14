import { http } from '@/services/http'

export interface Review {
  id: string
  auctionId: string
  orderId?: string
  reviewerId: string
  reviewerUsername: string
  reviewedUserId: string
  reviewedUsername: string
  rating: number
  title?: string
  comment: string
  sellerResponse?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateReviewRequest {
  auctionId: string
  orderId?: string
  reviewedUserId: string
  reviewedUsername: string
  rating: number
  title?: string
  comment: string
}

export interface SellerResponseRequest {
  response: string
}

export interface UserRatingSummary {
  username: string
  averageRating: number
  totalReviews: number
  ratingDistribution: Record<number, number>
}

export const reviewsApi = {
  async getReviewById(id: string): Promise<Review> {
    const response = await http.get<Review>(`/reviews/${id}`)
    return response.data
  },

  async getReviewsForAuction(auctionId: string): Promise<Review[]> {
    const response = await http.get<Review[]>(`/reviews/auction/${auctionId}`)
    return response.data
  },

  async getReviewsForUser(username: string): Promise<Review[]> {
    const response = await http.get<Review[]>(`/reviews/user/${username}`)
    return response.data
  },

  async getReviewsByUser(username: string): Promise<Review[]> {
    const response = await http.get<Review[]>(`/reviews/by/${username}`)
    return response.data
  },

  async getUserRatingSummary(username: string): Promise<UserRatingSummary> {
    const response = await http.get<UserRatingSummary>(`/reviews/user/${username}/summary`)
    return response.data
  },

  async createReview(data: CreateReviewRequest): Promise<Review> {
    const response = await http.post<Review>('/reviews', data)
    return response.data
  },

  async addSellerResponse(reviewId: string, data: SellerResponseRequest): Promise<Review> {
    const response = await http.post<Review>(`/reviews/${reviewId}/response`, data)
    return response.data
  },
}
