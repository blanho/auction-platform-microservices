export interface Review {
  id: string
  rating: number
  title: string
  content: string
  createdAt: string
  reviewer: {
    id: string
    name: string
    avatarUrl?: string
    isVerifiedBuyer: boolean
    totalReviews: number
  }
  helpfulCount: number
  notHelpfulCount: number
  userVote?: 'helpful' | 'not_helpful'
  sellerResponse?: {
    content: string
    createdAt: string
  }
}

export interface ReviewsResponse {
  data: Review[]
  totalCount: number
  averageRating: number
  ratingDistribution: Record<number, number>
}

export type SortOption = 'newest' | 'highest' | 'lowest' | 'helpful'
