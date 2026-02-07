import { useState, useMemo } from 'react'
import { useParams, Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Box,
  Container,
  Typography,
  Avatar,
  Card,
  CardContent,
  Grid,
  Chip,
  Rating,
  LinearProgress,
  Tabs,
  Tab,
  Divider,
  Stack,
  Skeleton,
  Button,
} from '@mui/material'
import {
  Verified,
  Star,
  Storefront,
  CalendarMonth,
  LocalShipping,
  ThumbUp,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { useReviewsForUser, useUserRatingSummary } from '../hooks/useReviews'
import { useAuctions } from '@/modules/auctions/hooks'
import { InlineAlert } from '@/shared/ui'
import { formatRelativeTime } from '@/shared/utils/formatters'

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

function RatingDistributionBar({
  rating,
  count,
  total,
}: Readonly<{ rating: number; count: number; total: number }>) {
  const percentage = total > 0 ? (count / total) * 100 : 0

  return (
    <Stack direction="row" alignItems="center" spacing={1.5}>
      <Typography variant="body2" sx={{ minWidth: 60 }}>
        {rating} stars
      </Typography>
      <Box sx={{ flex: 1 }}>
        <LinearProgress
          variant="determinate"
          value={percentage}
          sx={{
            height: 8,
            borderRadius: 4,
            bgcolor: 'grey.100',
            '& .MuiLinearProgress-bar': {
              bgcolor: palette.semantic.warning,
              borderRadius: 4,
            },
          }}
        />
      </Box>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 30 }}>
        {count}
      </Typography>
    </Stack>
  )
}

function ReviewCard({
  review,
}: Readonly<{
  review: {
    id: string
    reviewerUsername: string
    rating: number
    title?: string
    comment: string
    createdAt: string
    sellerResponse?: string
  }
}>) {
  return (
    <Card variant="outlined" sx={{ mb: 2 }}>
      <CardContent>
        <Stack direction="row" justifyContent="space-between" alignItems="flex-start" mb={1}>
          <Stack direction="row" spacing={2} alignItems="center">
            <Avatar sx={{ width: 40, height: 40, bgcolor: palette.brand.primary }}>
              {review.reviewerUsername.charAt(0).toUpperCase()}
            </Avatar>
            <Box>
              <Typography variant="subtitle2">{review.reviewerUsername}</Typography>
              <Typography variant="caption" color="text.secondary">
                {formatRelativeTime(review.createdAt)}
              </Typography>
            </Box>
          </Stack>
          <Rating value={review.rating} readOnly size="small" />
        </Stack>

        {review.title && (
          <Typography variant="subtitle1" fontWeight={600} mt={2}>
            {review.title}
          </Typography>
        )}
        <Typography variant="body2" color="text.secondary" mt={1}>
          {review.comment}
        </Typography>

        {review.sellerResponse && (
          <Box sx={{ mt: 2, pl: 2, borderLeft: `3px solid ${palette.brand.primary}` }}>
            <Typography variant="caption" fontWeight={600} color="primary">
              Seller Response
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {review.sellerResponse}
            </Typography>
          </Box>
        )}
      </CardContent>
    </Card>
  )
}

function AuctionCard({
  auction,
}: Readonly<{
  auction: {
    id: string
    title: string
    currentBid: number
    endTime: string
    primaryImageUrl?: string
    bidCount: number
  }
}>) {
  return (
    <Card
      component={Link}
      to={`/auctions/${auction.id}`}
      sx={{
        textDecoration: 'none',
        display: 'block',
        transition: 'transform 0.2s, box-shadow 0.2s',
        cursor: 'pointer',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <Box
        sx={{
          height: 200,
          bgcolor: 'grey.100',
          backgroundImage: auction.primaryImageUrl
            ? `url(${auction.primaryImageUrl})`
            : undefined,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        {!auction.primaryImageUrl && (
          <Storefront sx={{ fontSize: 48, color: 'grey.400' }} />
        )}
      </Box>
      <CardContent>
        <Typography variant="subtitle1" fontWeight={600} noWrap>
          {auction.title}
        </Typography>
        <Stack direction="row" justifyContent="space-between" alignItems="center" mt={1}>
          <Typography variant="body2" color="primary" fontWeight={600}>
            ${auction.currentBid.toLocaleString()}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {auction.bidCount} bids
          </Typography>
        </Stack>
      </CardContent>
    </Card>
  )
}

interface AuctionListingItem {
  id: string
  title: string
  currentBid: number
  endTime: string
  primaryImageUrl?: string
  bidCount: number
}

function ListingsTabContent({
  isLoading,
  auctions,
}: Readonly<{
  isLoading: boolean
  auctions: AuctionListingItem[]
}>) {
  const skeletonKeys = useMemo(
    () => Array.from({ length: 4 }, () => crypto.randomUUID()),
    []
  )

  if (isLoading) {
    return (
      <Grid container spacing={3}>
        {skeletonKeys.map((key) => (
          <Grid size={{ xs: 12, sm: 6, md: 3 }} key={key}>
            <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 2 }} />
          </Grid>
        ))}
      </Grid>
    )
  }

  if (auctions.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 6 }}>
        <Storefront sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
        <Typography variant="h6" color="text.secondary">
          No active listings
        </Typography>
      </Box>
    )
  }

  return (
    <Grid container spacing={3}>
      {auctions.map((auction) => (
        <Grid size={{ xs: 12, sm: 6, md: 3 }} key={auction.id}>
          <AuctionCard auction={auction} />
        </Grid>
      ))}
    </Grid>
  )
}

interface ReviewSummaryItem {
  id: string
  reviewerUsername: string
  rating: number
  title?: string
  comment: string
  createdAt: string
  sellerResponse?: string
}

function ReviewsTabContent({
  isLoading,
  reviews,
  ratingSummary,
}: Readonly<{
  isLoading: boolean
  reviews: ReviewSummaryItem[] | undefined
  ratingSummary: {
    averageRating: number
    totalReviews: number
    ratingDistribution: Record<number, number>
  } | undefined
}>) {
  const skeletonKeys = useMemo(
    () => Array.from({ length: 3 }, () => crypto.randomUUID()),
    []
  )

  return (
    <>
      {ratingSummary && (
        <Box sx={{ mb: 4 }}>
          <Grid container spacing={4}>
            <Grid size={{ xs: 12, md: 4 }}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h2" fontWeight={700}>
                  {ratingSummary.averageRating.toFixed(1)}
                </Typography>
                <Rating
                  value={ratingSummary.averageRating}
                  precision={0.1}
                  readOnly
                  size="large"
                />
                <Typography variant="body2" color="text.secondary" mt={1}>
                  Based on {ratingSummary.totalReviews} reviews
                </Typography>
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 8 }}>
              <Stack spacing={1}>
                {[5, 4, 3, 2, 1].map((rating) => (
                  <RatingDistributionBar
                    key={rating}
                    rating={rating}
                    count={ratingSummary.ratingDistribution[rating] ?? 0}
                    total={ratingSummary.totalReviews}
                  />
                ))}
              </Stack>
            </Grid>
          </Grid>
          <Divider sx={{ my: 4 }} />
        </Box>
      )}

      {isLoading && (
        <Stack spacing={2}>
          {skeletonKeys.map((key) => (
            <Skeleton key={key} variant="rectangular" height={120} sx={{ borderRadius: 2 }} />
          ))}
        </Stack>
      )}

      {!isLoading && (!reviews || reviews.length === 0) && (
        <Box sx={{ textAlign: 'center', py: 6 }}>
          <Star sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">
            No reviews yet
          </Typography>
        </Box>
      )}

      {!isLoading && reviews && reviews.length > 0 && (
        <Stack spacing={0}>
          {reviews.map((review) => (
            <ReviewCard key={review.id} review={review} />
          ))}
        </Stack>
      )}
    </>
  )
}

export function SellerProfilePage() {
  const { sellerId } = useParams<{ sellerId: string }>()
  const [activeTab, setActiveTab] = useState(0)

  const { data: ratingSummary, isLoading: ratingSummaryLoading } = useUserRatingSummary(
    sellerId ?? ''
  )
  const { data: reviews, isLoading: reviewsLoading } = useReviewsForUser(sellerId ?? '')
  const { data: auctionsData, isLoading: auctionsLoading } = useAuctions({
    sellerId,
    page: 1,
    pageSize: 12,
    status: 'active',
  })

  const auctions = useMemo(() => auctionsData?.items ?? [], [auctionsData])

  const stats = useMemo(
    () => [
      {
        label: 'Total Reviews',
        value: ratingSummary?.totalReviews ?? 0,
        icon: <Star />,
      },
      {
        label: 'Active Listings',
        value: auctionsData?.totalCount ?? 0,
        icon: <Storefront />,
      },
      {
        label: 'Response Rate',
        value: '98%',
        icon: <ThumbUp />,
      },
      {
        label: 'Avg. Ship Time',
        value: '2-3 days',
        icon: <LocalShipping />,
      },
    ],
    [ratingSummary, auctionsData]
  )

  if (!sellerId) {
    return (
      <Container maxWidth="lg" sx={{ py: 4 }}>
        <InlineAlert severity="error">Seller not found</InlineAlert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Card sx={{ mb: 4 }}>
            <CardContent sx={{ p: 4 }}>
              <Stack
                direction={{ xs: 'column', md: 'row' }}
                spacing={4}
                alignItems={{ xs: 'center', md: 'flex-start' }}
              >
                <Avatar
                  sx={{
                    width: 120,
                    height: 120,
                    bgcolor: palette.brand.primary,
                    fontSize: 48,
                  }}
                >
                  {sellerId.charAt(0).toUpperCase()}
                </Avatar>

                <Box sx={{ flex: 1, textAlign: { xs: 'center', md: 'left' } }}>
                  <Stack
                    direction="row"
                    spacing={1}
                    alignItems="center"
                    justifyContent={{ xs: 'center', md: 'flex-start' }}
                  >
                    <Typography variant="h4" fontWeight={700}>
                      {sellerId}
                    </Typography>
                    <Chip
                      icon={<Verified sx={{ fontSize: 16 }} />}
                      label="Verified Seller"
                      size="small"
                      color="primary"
                    />
                  </Stack>

                  <Stack
                    direction="row"
                    spacing={2}
                    alignItems="center"
                    mt={1}
                    justifyContent={{ xs: 'center', md: 'flex-start' }}
                  >
                    {ratingSummaryLoading ? (
                      <Skeleton width={150} />
                    ) : (
                      <>
                        <Rating
                          value={ratingSummary?.averageRating ?? 0}
                          precision={0.1}
                          readOnly
                          size="small"
                        />
                        <Typography variant="body2" color="text.secondary">
                          ({ratingSummary?.totalReviews ?? 0} reviews)
                        </Typography>
                      </>
                    )}
                  </Stack>

                  <Stack
                    direction="row"
                    spacing={1}
                    alignItems="center"
                    mt={2}
                    color="text.secondary"
                    justifyContent={{ xs: 'center', md: 'flex-start' }}
                  >
                    <CalendarMonth sx={{ fontSize: 18 }} />
                    <Typography variant="body2">Member since January 2024</Typography>
                  </Stack>
                </Box>

                <Button variant="outlined" color="primary">
                  Contact Seller
                </Button>
              </Stack>
            </CardContent>
          </Card>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Grid container spacing={3} sx={{ mb: 4 }}>
            {stats.map((stat) => (
              <Grid size={{ xs: 6, md: 3 }} key={stat.label}>
                <Card variant="outlined">
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Box
                      sx={{
                        width: 48,
                        height: 48,
                        borderRadius: 2,
                        bgcolor: `${palette.brand.primary}15`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        mx: 'auto',
                        mb: 1,
                        color: palette.brand.primary,
                      }}
                    >
                      {stat.icon}
                    </Box>
                    <Typography variant="h5" fontWeight={700}>
                      {stat.value}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      {stat.label}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={activeTab} onChange={(_, v) => setActiveTab(v)}>
                <Tab label={`Listings (${auctionsData?.totalCount ?? 0})`} />
                <Tab label={`Reviews (${ratingSummary?.totalReviews ?? 0})`} />
              </Tabs>
            </Box>

            <CardContent>
              <TabPanel value={activeTab} index={0}>
                <ListingsTabContent isLoading={auctionsLoading} auctions={auctions} />
              </TabPanel>

              <TabPanel value={activeTab} index={1}>
                <ReviewsTabContent
                  isLoading={reviewsLoading}
                  reviews={reviews}
                  ratingSummary={ratingSummary}
                />
              </TabPanel>
            </CardContent>
          </Card>
        </motion.div>
      </motion.div>
    </Container>
  )
}
