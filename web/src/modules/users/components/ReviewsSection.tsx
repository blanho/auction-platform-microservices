import { useState } from 'react'
import {
  Box,
  Typography,
  Card,
  Avatar,
  Rating,
  Stack,
  Button,
  Divider,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  LinearProgress,
  Pagination,
} from '@mui/material'
import {
  ThumbUp,
  ThumbDown,
  MoreVert,
  Flag,
  VerifiedUser,
  Sort,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { motion, AnimatePresence } from 'framer-motion'
import { http } from '@/services/http'
import { formatRelativeTime } from '@/shared/utils/formatters'
import { useAuth } from '@/app/hooks/useAuth'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'

interface Review {
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

interface ReviewsResponse {
  data: Review[]
  totalCount: number
  averageRating: number
  ratingDistribution: Record<number, number>
}

interface ReviewsSectionProps {
  sellerId: string
  auctionId?: string
}

type SortOption = 'newest' | 'highest' | 'lowest' | 'helpful'

export function ReviewsSection({ sellerId, auctionId }: ReviewsSectionProps) {
  const { user } = useAuth()
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [sortBy, setSortBy] = useState<SortOption>('newest')
  const [sortAnchor, setSortAnchor] = useState<null | HTMLElement>(null)
  const pageSize = 5

  const reviewsQuery = useQuery<ReviewsResponse>({
    queryKey: ['reviews', sellerId, auctionId, page, sortBy],
    queryFn: async (): Promise<ReviewsResponse> => {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        sortBy,
        ...(auctionId && { auctionId }),
      })
      const response = await http.get(`/users/${sellerId}/reviews?${params}`)
      return response.data as ReviewsResponse
    },
  })

  const voteMutation = useMutation({
    mutationFn: async ({ reviewId, vote }: { reviewId: string; vote: 'helpful' | 'not_helpful' }) => {
      const response = await http.post(`/reviews/${reviewId}/vote`, { vote })
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reviews', sellerId] })
    },
  })

  const sortOptions: { value: SortOption; label: string }[] = [
    { value: 'newest', label: 'Newest First' },
    { value: 'highest', label: 'Highest Rating' },
    { value: 'lowest', label: 'Lowest Rating' },
    { value: 'helpful', label: 'Most Helpful' },
  ]

  const totalPages = reviewsQuery.data
    ? Math.ceil(reviewsQuery.data.totalCount / pageSize)
    : 0

  const ratingDistribution = reviewsQuery.data?.ratingDistribution || {}
  const totalReviews = reviewsQuery.data?.totalCount || 0

  return (
    <motion.div variants={staggerContainer} initial="initial" animate="animate">
      <Card sx={{ p: 3 }}>
        <motion.div variants={fadeInUp}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
            <Typography variant="h6" fontWeight={600}>
              Reviews & Ratings
            </Typography>
            <Button
              startIcon={<Sort />}
              onClick={(e) => setSortAnchor(e.currentTarget)}
              variant="outlined"
              size="small"
            >
              {sortOptions.find(o => o.value === sortBy)?.label}
            </Button>
            <Menu
              anchorEl={sortAnchor}
              open={Boolean(sortAnchor)}
              onClose={() => setSortAnchor(null)}
            >
              {sortOptions.map((option) => (
                <MenuItem
                  key={option.value}
                  selected={sortBy === option.value}
                  onClick={() => {
                    setSortBy(option.value)
                    setSortAnchor(null)
                  }}
                >
                  {option.label}
                </MenuItem>
              ))}
            </Menu>
          </Box>
        </motion.div>

        {reviewsQuery.data && (
          <motion.div variants={staggerItem}>
            <Box sx={{ display: 'flex', gap: 4, mb: 4 }}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography
                  variant="h2"
                  fontWeight={700}
                  sx={{ fontFamily: '"Playfair Display", serif' }}
                >
                  {reviewsQuery.data.averageRating.toFixed(1)}
                </Typography>
                <Rating
                  value={reviewsQuery.data.averageRating}
                  precision={0.1}
                  readOnly
                  sx={{ color: '#CA8A04' }}
                />
                <Typography variant="body2" color="text.secondary">
                  {totalReviews} reviews
                </Typography>
              </Box>

              <Box sx={{ flex: 1 }}>
                {[5, 4, 3, 2, 1].map((rating) => {
                  const count = ratingDistribution[rating] || 0
                  const percentage = totalReviews > 0 ? (count / totalReviews) * 100 : 0
                  return (
                    <Box key={rating} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                      <Typography variant="body2" sx={{ width: 20 }}>
                        {rating}
                      </Typography>
                      <LinearProgress
                        variant="determinate"
                        value={percentage}
                        sx={{
                          flex: 1,
                          height: 8,
                          borderRadius: 1,
                          bgcolor: 'grey.200',
                          '& .MuiLinearProgress-bar': {
                            bgcolor: '#CA8A04',
                          },
                        }}
                      />
                      <Typography variant="body2" color="text.secondary" sx={{ width: 30 }}>
                        {count}
                      </Typography>
                    </Box>
                  )
                })}
              </Box>
            </Box>
          </motion.div>
        )}

        <Divider sx={{ mb: 3 }} />

        {reviewsQuery.isLoading ? (
          <Box sx={{ py: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">Loading reviews...</Typography>
          </Box>
        ) : reviewsQuery.data?.data.length === 0 ? (
          <Box sx={{ py: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">No reviews yet</Typography>
          </Box>
        ) : (
          <AnimatePresence mode="wait">
            <Stack spacing={3}>
              {reviewsQuery.data?.data.map((review) => (
                <ReviewCard
                  key={review.id}
                  review={review}
                  onVote={(vote) => voteMutation.mutate({ reviewId: review.id, vote })}
                  isVoting={voteMutation.isPending}
                  currentUserId={user?.id}
                />
              ))}
            </Stack>
          </AnimatePresence>
        )}

        {totalPages > 1 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Pagination
              count={totalPages}
              page={page}
              onChange={(_, value) => setPage(value)}
              color="primary"
            />
          </Box>
        )}
      </Card>
    </motion.div>
  )
}

interface ReviewCardProps {
  review: Review
  onVote: (vote: 'helpful' | 'not_helpful') => void
  isVoting: boolean
  currentUserId?: string
}

function ReviewCard({ review, onVote, isVoting, currentUserId }: ReviewCardProps) {
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null)

  return (
    <motion.div variants={staggerItem}>
      <Box>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Avatar
            src={review.reviewer.avatarUrl}
            sx={{ width: 48, height: 48, bgcolor: 'primary.main' }}
          >
            {review.reviewer.name[0]}
          </Avatar>

          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
              <Typography variant="subtitle2" fontWeight={600}>
                {review.reviewer.name}
              </Typography>
              {review.reviewer.isVerifiedBuyer && (
                <Chip
                  icon={<VerifiedUser sx={{ fontSize: 14 }} />}
                  label="Verified Buyer"
                  size="small"
                  color="success"
                  sx={{ height: 22 }}
                />
              )}
              <Box sx={{ flex: 1 }} />
              <IconButton
                size="small"
                onClick={(e) => setMenuAnchor(e.currentTarget)}
              >
                <MoreVert fontSize="small" />
              </IconButton>
              <Menu
                anchorEl={menuAnchor}
                open={Boolean(menuAnchor)}
                onClose={() => setMenuAnchor(null)}
              >
                <MenuItem onClick={() => setMenuAnchor(null)}>
                  <Flag sx={{ mr: 1, fontSize: 18 }} />
                  Report Review
                </MenuItem>
              </Menu>
            </Box>

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <Rating
                value={review.rating}
                readOnly
                size="small"
                sx={{ color: '#CA8A04' }}
              />
              <Typography variant="caption" color="text.secondary">
                {formatRelativeTime(review.createdAt)}
              </Typography>
            </Box>

            {review.title && (
              <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                {review.title}
              </Typography>
            )}

            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              {review.content}
            </Typography>

            {review.sellerResponse && (
              <Box
                sx={{
                  bgcolor: 'grey.50',
                  p: 2,
                  borderRadius: 1,
                  ml: 2,
                  borderLeft: 3,
                  borderColor: 'primary.main',
                  mb: 2,
                }}
              >
                <Typography variant="caption" color="primary" fontWeight={600}>
                  Seller Response
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  {review.sellerResponse.content}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {formatRelativeTime(review.sellerResponse.createdAt)}
                </Typography>
              </Box>
            )}

            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="caption" color="text.secondary">
                Was this helpful?
              </Typography>
              <Button
                size="small"
                startIcon={<ThumbUp fontSize="small" />}
                onClick={() => onVote('helpful')}
                disabled={isVoting || review.reviewer.id === currentUserId}
                sx={{
                  minWidth: 0,
                  color: review.userVote === 'helpful' ? 'primary.main' : 'text.secondary',
                }}
              >
                {review.helpfulCount}
              </Button>
              <Button
                size="small"
                startIcon={<ThumbDown fontSize="small" />}
                onClick={() => onVote('not_helpful')}
                disabled={isVoting || review.reviewer.id === currentUserId}
                sx={{
                  minWidth: 0,
                  color: review.userVote === 'not_helpful' ? 'error.main' : 'text.secondary',
                }}
              >
                {review.notHelpfulCount}
              </Button>
            </Stack>
          </Box>
        </Box>
      </Box>
    </motion.div>
  )
}
