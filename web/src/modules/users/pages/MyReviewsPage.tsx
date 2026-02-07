import { useState, useMemo } from 'react'
import { motion } from 'framer-motion'
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  Tabs,
  Tab,
  Stack,
  Rating,
  Avatar,
  Chip,
  Button,
  Skeleton,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from '@mui/material'
import { Star, RateReview, Reply } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { useReviewsForUser, useReviewsByUser, useUserRatingSummary, useAddSellerResponse } from '../hooks/useReviews'
import { useAuth } from '@/app/hooks/useAuth'
import { InlineAlert } from '@/shared/ui'
import { formatRelativeTime } from '@/shared/utils/formatters'
import { Link } from 'react-router-dom'

interface TabPanelProps {
  readonly children?: React.ReactNode
  readonly index: number
  readonly value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  )
}

interface ReviewData {
  id: string
  auctionId: string
  reviewerUsername: string
  reviewedUsername: string
  rating: number
  title?: string
  comment: string
  createdAt: string
  sellerResponse?: string
}

interface ReviewItemProps {
  readonly review: ReviewData
  readonly type: 'received' | 'given'
  readonly onRespond?: (reviewId: string) => void
  readonly canRespond?: boolean
}

function ReviewItem({ review, type, onRespond, canRespond }: ReviewItemProps) {
  const displayUsername = type === 'received' ? review.reviewerUsername : review.reviewedUsername

  return (
    <Card variant="outlined" sx={{ mb: 2 }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
          <Stack direction="row" spacing={2} alignItems="center">
            <Avatar sx={{ width: 44, height: 44, bgcolor: palette.brand.primary }}>
              {displayUsername.charAt(0).toUpperCase()}
            </Avatar>
            <Box>
              <Stack direction="row" spacing={1} alignItems="center">
                <Typography variant="subtitle1" fontWeight={600}>
                  {type === 'received' ? 'From' : 'To'}: {displayUsername}
                </Typography>
                <Chip
                  label={type === 'received' ? 'Received' : 'Given'}
                  size="small"
                  color={type === 'received' ? 'primary' : 'default'}
                  variant="outlined"
                />
              </Stack>
              <Typography variant="caption" color="text.secondary">
                {formatRelativeTime(review.createdAt)}
              </Typography>
            </Box>
          </Stack>
          <Stack direction="row" spacing={1} alignItems="center">
            <Rating value={review.rating} readOnly size="small" />
            {type === 'received' && canRespond && !review.sellerResponse && (
              <Button
                size="small"
                startIcon={<Reply />}
                onClick={() => onRespond?.(review.id)}
              >
                Respond
              </Button>
            )}
          </Stack>
        </Stack>

        <Box sx={{ mt: 2 }}>
          {review.title && (
            <Typography variant="subtitle2" fontWeight={600}>
              {review.title}
            </Typography>
          )}
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            {review.comment}
          </Typography>
        </Box>

        <Button
          component={Link}
          to={`/auctions/${review.auctionId}`}
          size="small"
          sx={{ mt: 2 }}
        >
          View Auction
        </Button>

        {review.sellerResponse && (
          <Box
            sx={{
              mt: 2,
              p: 2,
              bgcolor: 'grey.50',
              borderRadius: 2,
              borderLeft: `3px solid ${palette.brand.primary}`,
            }}
          >
            <Typography variant="caption" fontWeight={600} color="primary">
              Your Response
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {review.sellerResponse}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  )
}

function ReviewsSkeleton() {
  const skeletonKeys = useMemo(
    () => Array.from({ length: 3 }, () => crypto.randomUUID()),
    []
  )

  return (
    <Stack spacing={2}>
      {skeletonKeys.map((key) => (
        <Skeleton key={key} variant="rectangular" height={140} sx={{ borderRadius: 2 }} />
      ))}
    </Stack>
  )
}

function EmptyState({ message, icon }: Readonly<{ message: string; icon: React.ReactNode }>) {
  return (
    <Box sx={{ textAlign: 'center', py: 8 }}>
      <Box sx={{ color: 'text.disabled', mb: 2 }}>{icon}</Box>
      <Typography variant="h6" color="text.secondary">
        {message}
      </Typography>
    </Box>
  )
}

function ReceivedReviewsContent({
  isLoading,
  reviews,
  onRespond,
}: Readonly<{
  isLoading: boolean
  reviews: ReviewData[] | undefined
  onRespond: (reviewId: string) => void
}>) {
  if (isLoading) {
    return <ReviewsSkeleton />
  }

  if (!reviews || reviews.length === 0) {
    return (
      <EmptyState
        message="No reviews received yet"
        icon={<Star sx={{ fontSize: 64 }} />}
      />
    )
  }

  return (
    <Stack spacing={0}>
      {reviews.map((review) => (
        <ReviewItem
          key={review.id}
          review={review}
          type="received"
          onRespond={onRespond}
          canRespond
        />
      ))}
    </Stack>
  )
}

function GivenReviewsContent({
  isLoading,
  reviews,
}: Readonly<{
  isLoading: boolean
  reviews: ReviewData[] | undefined
}>) {
  if (isLoading) {
    return <ReviewsSkeleton />
  }

  if (!reviews || reviews.length === 0) {
    return (
      <EmptyState
        message="No reviews given yet"
        icon={<RateReview sx={{ fontSize: 64 }} />}
      />
    )
  }

  return (
    <Stack spacing={0}>
      {reviews.map((review) => (
        <ReviewItem key={review.id} review={review} type="given" />
      ))}
    </Stack>
  )
}

export function MyReviewsPage() {
  const { user } = useAuth()
  const [activeTab, setActiveTab] = useState(0)
  const [responseDialogOpen, setResponseDialogOpen] = useState(false)
  const [selectedReviewId, setSelectedReviewId] = useState<string | null>(null)
  const [responseText, setResponseText] = useState('')

  const username = user?.username ?? ''

  const { data: reviewsReceived, isLoading: receivedLoading } = useReviewsForUser(username)
  const { data: reviewsGiven, isLoading: givenLoading } = useReviewsByUser(username)
  const { data: ratingSummary, isLoading: summaryLoading } = useUserRatingSummary(username)
  const addResponse = useAddSellerResponse()

  const handleOpenResponse = (reviewId: string) => {
    setSelectedReviewId(reviewId)
    setResponseText('')
    setResponseDialogOpen(true)
  }

  const handleSubmitResponse = async () => {
    if (!selectedReviewId || !responseText.trim()) {
      return
    }

    await addResponse.mutateAsync({
      reviewId: selectedReviewId,
      data: { response: responseText },
    })

    setResponseDialogOpen(false)
    setSelectedReviewId(null)
    setResponseText('')
  }

  if (!user) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <InlineAlert severity="error">Please log in to view your reviews</InlineAlert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Stack direction="row" justifyContent="space-between" alignItems="center" mb={4}>
            <Box>
              <Typography variant="h4" fontWeight={700}>
                My Reviews
              </Typography>
              <Typography variant="body1" color="text.secondary" mt={0.5}>
                Manage reviews you&apos;ve received and given
              </Typography>
            </Box>
          </Stack>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Card sx={{ mb: 4 }}>
            <CardContent>
              <Stack
                direction={{ xs: 'column', md: 'row' }}
                spacing={4}
                alignItems="center"
                justifyContent="space-around"
              >
                <Box sx={{ textAlign: 'center' }}>
                  {summaryLoading ? (
                    <Skeleton width={80} height={60} sx={{ mx: 'auto' }} />
                  ) : (
                    <>
                      <Typography variant="h3" fontWeight={700}>
                        {ratingSummary?.averageRating.toFixed(1) ?? '0.0'}
                      </Typography>
                      <Rating
                        value={ratingSummary?.averageRating ?? 0}
                        precision={0.1}
                        readOnly
                      />
                      <Typography variant="body2" color="text.secondary" mt={1}>
                        Average Rating
                      </Typography>
                    </>
                  )}
                </Box>

                <Stack direction="row" spacing={6}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight={700} color="primary">
                      {reviewsReceived?.length ?? 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Reviews Received
                    </Typography>
                  </Box>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" fontWeight={700}>
                      {reviewsGiven?.length ?? 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Reviews Given
                    </Typography>
                  </Box>
                </Stack>
              </Stack>
            </CardContent>
          </Card>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
                <Tab
                  icon={<Star sx={{ fontSize: 20 }} />}
                  iconPosition="start"
                  label={`Received (${reviewsReceived?.length ?? 0})`}
                />
                <Tab
                  icon={<RateReview sx={{ fontSize: 20 }} />}
                  iconPosition="start"
                  label={`Given (${reviewsGiven?.length ?? 0})`}
                />
              </Tabs>
            </Box>

            <CardContent>
              <TabPanel value={activeTab} index={0}>
                <ReceivedReviewsContent
                  isLoading={receivedLoading}
                  reviews={reviewsReceived}
                  onRespond={handleOpenResponse}
                />
              </TabPanel>

              <TabPanel value={activeTab} index={1}>
                <GivenReviewsContent isLoading={givenLoading} reviews={reviewsGiven} />
              </TabPanel>
            </CardContent>
          </Card>
        </motion.div>
      </motion.div>

      <Dialog
        open={responseDialogOpen}
        onClose={() => setResponseDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Respond to Review</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Your response will be visible to everyone viewing this review.
          </Typography>
          <TextField
            multiline
            rows={4}
            fullWidth
            placeholder="Thank the reviewer or address their feedback..."
            value={responseText}
            onChange={(e) => setResponseText(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setResponseDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleSubmitResponse}
            disabled={!responseText.trim() || addResponse.isPending}
          >
            {addResponse.isPending ? 'Submitting...' : 'Submit Response'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  )
}
