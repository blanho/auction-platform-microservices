import { useState } from 'react'
import {
  Box,
  Typography,
  Stack,
  Avatar,
  Rating,
  Button,
  Skeleton,
  Divider,
  LinearProgress,
} from '@mui/material'
import { Star, Reply, ExpandMore, ExpandLess } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import type { Review, UserRatingSummary } from '@/modules/users/api/reviews.api'
import { formatTimeAgo } from '../utils'

interface ReviewsListProps {
  reviews: Review[]
  ratingSummary?: UserRatingSummary
  isLoading?: boolean
  showSummary?: boolean
}

function RatingSummaryCard({ summary }: { summary: UserRatingSummary }) {
  const distribution = summary.ratingDistribution || {}
  const maxCount = Math.max(...Object.values(distribution), 1)

  return (
    <Box
      sx={{
        p: 3,
        bgcolor: palette.neutral[50],
        borderRadius: 2,
        border: `1px solid ${palette.neutral[100]}`,
        mb: 3,
      }}
    >
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={4} alignItems={{ sm: 'center' }}>
        <Box sx={{ textAlign: 'center', minWidth: 120 }}>
          <Typography
            sx={{
              fontSize: '3rem',
              fontWeight: 700,
              color: palette.neutral[900],
              lineHeight: 1,
            }}
          >
            {summary.averageRating.toFixed(1)}
          </Typography>
          <Rating
            value={summary.averageRating}
            precision={0.1}
            size="small"
            readOnly
            sx={{ color: palette.brand.primary, mt: 1 }}
          />
          <Typography sx={{ color: palette.neutral[500], fontSize: '0.875rem', mt: 0.5 }}>
            {summary.totalReviews} reviews
          </Typography>
        </Box>

        <Box sx={{ flex: 1 }}>
          {[5, 4, 3, 2, 1].map((rating) => (
            <Stack key={rating} direction="row" spacing={1} alignItems="center" sx={{ mb: 0.5 }}>
              <Typography sx={{ width: 20, fontSize: '0.875rem', color: palette.neutral[500] }}>
                {rating}
              </Typography>
              <Star sx={{ fontSize: 16, color: palette.brand.primary }} />
              <LinearProgress
                variant="determinate"
                value={((distribution[rating] || 0) / maxCount) * 100}
                sx={{
                  flex: 1,
                  height: 8,
                  borderRadius: 4,
                  bgcolor: palette.neutral[100],
                  '& .MuiLinearProgress-bar': {
                    bgcolor: palette.brand.primary,
                    borderRadius: 4,
                  },
                }}
              />
              <Typography
                sx={{
                  width: 30,
                  fontSize: '0.875rem',
                  color: palette.neutral[500],
                  textAlign: 'right',
                }}
              >
                {distribution[rating] || 0}
              </Typography>
            </Stack>
          ))}
        </Box>
      </Stack>
    </Box>
  )
}

function ReviewItem({ review }: { review: Review }) {
  const [expanded, setExpanded] = useState(false)

  const isLongComment = review.comment.length > 300

  return (
    <Box sx={{ py: 3 }}>
      <Stack direction="row" spacing={2}>
        <Avatar
          sx={{
            width: 44,
            height: 44,
            bgcolor: palette.brand.primary,
            fontSize: '1rem',
          }}
        >
          {review.reviewerUsername.charAt(0).toUpperCase()}
        </Avatar>

        <Box sx={{ flex: 1 }}>
          <Stack direction="row" alignItems="center" spacing={1} flexWrap="wrap">
            <Typography sx={{ fontWeight: 600, color: palette.neutral[900] }}>
              {review.reviewerUsername}
            </Typography>
            <Rating
              value={review.rating}
              size="small"
              readOnly
              sx={{ color: palette.brand.primary }}
            />
            <Typography sx={{ color: palette.neutral[500], fontSize: '0.8125rem' }}>
              {formatTimeAgo(review.createdAt)}
            </Typography>
          </Stack>

          {review.title && (
            <Typography
              sx={{
                fontWeight: 600,
                color: palette.neutral[900],
                mt: 1,
                fontSize: '0.9375rem',
              }}
            >
              {review.title}
            </Typography>
          )}

          <Typography
            sx={{
              color: palette.neutral[700],
              mt: 1,
              lineHeight: 1.6,
              ...(isLongComment &&
                !expanded && {
                  display: '-webkit-box',
                  WebkitLineClamp: 3,
                  WebkitBoxOrient: 'vertical',
                  overflow: 'hidden',
                }),
            }}
          >
            {review.comment}
          </Typography>

          {isLongComment && (
            <Button
              size="small"
              onClick={() => setExpanded(!expanded)}
              endIcon={expanded ? <ExpandLess /> : <ExpandMore />}
              sx={{
                color: palette.brand.primary,
                textTransform: 'none',
                mt: 0.5,
                p: 0,
                '&:hover': { bgcolor: 'transparent' },
              }}
            >
              {expanded ? 'Show less' : 'Read more'}
            </Button>
          )}

          {review.sellerResponse && (
            <Box
              sx={{
                mt: 2,
                p: 2,
                bgcolor: palette.neutral[100],
                borderRadius: 1,
                borderLeft: `3px solid ${palette.brand.primary}`,
              }}
            >
              <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 1 }}>
                <Reply
                  sx={{ fontSize: 18, color: palette.neutral[500], transform: 'scaleX(-1)' }}
                />
                <Typography
                  sx={{ fontWeight: 600, color: palette.neutral[900], fontSize: '0.875rem' }}
                >
                  Seller Response
                </Typography>
              </Stack>
              <Typography
                sx={{ color: palette.neutral[700], fontSize: '0.875rem', lineHeight: 1.6 }}
              >
                {review.sellerResponse}
              </Typography>
            </Box>
          )}
        </Box>
      </Stack>
    </Box>
  )
}

export function ReviewsList({
  reviews,
  ratingSummary,
  isLoading,
  showSummary = true,
}: ReviewsListProps) {
  if (isLoading) {
    return (
      <Box>
        {showSummary && (
          <Box sx={{ p: 3, bgcolor: palette.neutral[50], borderRadius: 2, mb: 3 }}>
            <Stack direction="row" spacing={4}>
              <Box sx={{ textAlign: 'center', minWidth: 120 }}>
                <Skeleton variant="text" width={60} height={60} />
                <Skeleton variant="text" width={80} />
              </Box>
              <Box sx={{ flex: 1 }}>
                {[1, 2, 3, 4, 5].map((i) => (
                  <Skeleton
                    key={i}
                    variant="rectangular"
                    height={8}
                    sx={{ mb: 1, borderRadius: 4 }}
                  />
                ))}
              </Box>
            </Stack>
          </Box>
        )}
        {[1, 2, 3].map((i) => (
          <Box key={i} sx={{ py: 3, borderBottom: '1px solid #E5E5E5' }}>
            <Stack direction="row" spacing={2}>
              <Skeleton variant="circular" width={44} height={44} />
              <Box sx={{ flex: 1 }}>
                <Skeleton variant="text" width={200} />
                <Skeleton variant="text" width="100%" />
                <Skeleton variant="text" width="80%" />
              </Box>
            </Stack>
          </Box>
        ))}
      </Box>
    )
  }

  if (reviews.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 6 }}>
        <Star sx={{ fontSize: 48, color: palette.neutral[300], mb: 2 }} />
        <Typography sx={{ color: palette.neutral[500], mb: 1 }}>No reviews yet</Typography>
        <Typography sx={{ color: palette.neutral[400], fontSize: '0.875rem' }}>
          Be the first to leave a review after your purchase
        </Typography>
      </Box>
    )
  }

  return (
    <Box>
      {showSummary && ratingSummary && <RatingSummaryCard summary={ratingSummary} />}

      <Stack divider={<Divider />}>
        {reviews.map((review) => (
          <ReviewItem key={review.id} review={review} />
        ))}
      </Stack>
    </Box>
  )
}

export function ReviewsListSkeleton() {
  return (
    <Box>
      <Box sx={{ p: 3, bgcolor: palette.neutral[50], borderRadius: 2, mb: 3 }}>
        <Stack direction="row" spacing={4}>
          <Box sx={{ textAlign: 'center', minWidth: 120 }}>
            <Skeleton variant="text" width={60} height={60} />
            <Skeleton variant="text" width={80} />
          </Box>
          <Box sx={{ flex: 1 }}>
            {[1, 2, 3, 4, 5].map((i) => (
              <Skeleton key={i} variant="rectangular" height={8} sx={{ mb: 1, borderRadius: 4 }} />
            ))}
          </Box>
        </Stack>
      </Box>
      {[1, 2, 3].map((i) => (
        <Box key={i} sx={{ py: 3, borderBottom: '1px solid #E5E5E5' }}>
          <Stack direction="row" spacing={2}>
            <Skeleton variant="circular" width={44} height={44} />
            <Box sx={{ flex: 1 }}>
              <Skeleton variant="text" width={200} />
              <Skeleton variant="text" width="100%" />
              <Skeleton variant="text" width="80%" />
            </Box>
          </Stack>
        </Box>
      ))}
    </Box>
  )
}
