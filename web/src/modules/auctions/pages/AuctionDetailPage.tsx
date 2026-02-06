import { useState, useCallback } from 'react'
import { useParams } from 'react-router-dom'
import {
  Box,
  Container,
  Grid,
  Typography,
  Breadcrumbs,
  Chip,
  Stack,
  Skeleton,
  Snackbar,
  IconButton,
  Tooltip,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import {
  NavigateNext,
  Visibility,
  ContentCopy,
  Facebook,
  Twitter,
  Pinterest,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { palette } from '@/shared/theme/tokens'
import { ImageGallery, ImageGallerySkeleton } from '../components/ImageGallery'
import { BidSection, BidSectionSkeleton } from '../components/BidSection'
import { SellerInfo, SellerInfoSkeleton } from '../components/SellerInfo'
import { ProductTabs, ProductTabsSkeleton } from '../components/ProductTabs'
import { ReviewsSection } from '@/modules/users/components/ReviewsSection'
import { useAuctionSignalR, useAuction } from '../hooks'
import { useAutoBidForAuction } from '@/modules/bidding/hooks'
import { useAuth } from '@/app/providers'

export function AuctionDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: auction, isLoading } = useAuction(id)
  const { isAuthenticated } = useAuth()
  const { data: autoBid } = useAutoBidForAuction(id, isAuthenticated && !isLoading)
  const [snackbar, setSnackbar] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error' | 'info'
  }>({
    open: false,
    message: '',
    severity: 'info',
  })

  useAuctionSignalR({ auctionId: id ?? '', enabled: !isLoading && !!id })

  const handleToggleFavorite = useCallback(() => {
    setSnackbar({
      open: true,
      message: auction?.isWatching ? 'Removed from watchlist' : 'Added to watchlist',
      severity: 'success',
    })
  }, [auction?.isWatching])

  const handleShare = useCallback(() => {
    navigator.clipboard.writeText(window.location.href)
    setSnackbar({
      open: true,
      message: 'Link copied to clipboard',
      severity: 'success',
    })
  }, [])

  const handlePlaceBid = useCallback(async () => {
    setSnackbar({
      open: true,
      message: 'Bid placed successfully',
      severity: 'success',
    })
  }, [])

  const handleBuyNow = useCallback(async () => {
    setSnackbar({
      open: true,
      message: 'Redirecting to checkout...',
      severity: 'info',
    })
  }, [])

  if (isLoading || !auction) {
    return <AuctionDetailPageSkeleton />
  }

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="xl" sx={{ pt: 3 }}>
        <Breadcrumbs
          separator={<NavigateNext fontSize="small" />}
          sx={{
            mb: 3,
            '& a': {
              color: palette.neutral[500],
              textDecoration: 'none',
              fontSize: '0.875rem',
              '&:hover': {
                color: palette.neutral[900],
                textDecoration: 'underline',
              },
            },
          }}
        >
          <Link to="/">Home</Link>
          <Link to="/auctions">Auctions</Link>
          {auction.category.parentName && (
            <Link to={`/categories/${auction.category.parentId}`}>
              {auction.category.parentName}
            </Link>
          )}
          <Link to={`/categories/${auction.category.id}`}>{auction.category.name}</Link>
          <Typography sx={{ color: palette.neutral[900], fontSize: '0.875rem' }}>
            {auction.title.length > 40 ? `${auction.title.slice(0, 40)}...` : auction.title}
          </Typography>
        </Breadcrumbs>

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, lg: 7 }}>
            <ImageGallery
              images={auction.images}
              title={auction.title}
              isFavorite={auction.isWatching}
              onToggleFavorite={handleToggleFavorite}
              onShare={handleShare}
            />
          </Grid>

          <Grid size={{ xs: 12, lg: 5 }}>
            <Box sx={{ position: 'sticky', top: 100 }}>
              <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
                <Chip
                  label={auction.categoryName}
                  size="small"
                  component={Link}
                  to={`/categories/${auction.categoryId}`}
                  sx={{
                    cursor: 'pointer',
                    bgcolor: palette.neutral[100],
                    color: palette.neutral[700],
                    '&:hover': { bgcolor: palette.neutral[100] },
                  }}
                />
                {auction.status === 'ending-soon' && (
                  <Chip
                    label="Ending Soon"
                    size="small"
                    sx={{ bgcolor: palette.semantic.errorLight, color: palette.semantic.error }}
                  />
                )}
              </Stack>

              <Typography
                variant="h4"
                component="h1"
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontWeight: 600,
                  color: palette.neutral[900],
                  lineHeight: 1.2,
                  mb: 1,
                }}
              >
                {auction.title}
              </Typography>

              <Stack direction="row" alignItems="center" spacing={2} sx={{ mb: 3 }}>
                <Stack
                  direction="row"
                  alignItems="center"
                  spacing={0.5}
                  sx={{ color: palette.neutral[500] }}
                >
                  <Visibility fontSize="small" />
                  <Typography variant="body2">{auction.watcherCount} watching</Typography>
                </Stack>

                <Stack direction="row" spacing={0.5}>
                  <Tooltip title="Share on Facebook">
                    <IconButton
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#1877F2' } }}
                    >
                      <Facebook fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Share on Twitter">
                    <IconButton
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#1DA1F2' } }}
                    >
                      <Twitter fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Pin on Pinterest">
                    <IconButton
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#E60023' } }}
                    >
                      <Pinterest fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Copy Link">
                    <IconButton
                      size="small"
                      onClick={handleShare}
                      sx={{
                        color: palette.neutral[500],
                        '&:hover': { color: palette.neutral[900] },
                      }}
                    >
                      <ContentCopy fontSize="small" />
                    </IconButton>
                  </Tooltip>
                </Stack>
              </Stack>

              <BidSection
                auctionId={auction.id}
                auctionTitle={auction.title}
                currentBid={auction.currentBid}
                startingPrice={auction.startingPrice}
                reservePrice={auction.reservePrice}
                buyNowPrice={auction.buyNowPrice}
                bidCount={auction.bidCount}
                endTime={auction.endTime}
                status={auction.status}
                userBid={auction.userBid}
                existingAutoBid={
                  autoBid ? { maxAmount: autoBid.maxAmount, isActive: autoBid.isActive } : undefined
                }
                onPlaceBid={handlePlaceBid}
                onBuyNow={handleBuyNow}
              />

              <Box sx={{ mt: 3 }}>
                <SellerInfo
                  seller={auction.seller}
                  onContact={() =>
                    setSnackbar({
                      open: true,
                      message: 'Opening chat with seller...',
                      severity: 'info',
                    })
                  }
                />
              </Box>
            </Box>
          </Grid>
        </Grid>

        <Box sx={{ mt: 6 }}>
          <ProductTabs
            description={auction.description}
            bids={auction.bids}
            specifications={{
              Dimensions: '78"W x 18"D x 32"H',
              Material: 'Solid Teak',
              Era: '1960s',
              Origin: 'Denmark',
              Condition: 'Professionally Restored',
              Style: 'Mid-Century Modern',
            }}
            shippingInfo={{
              method: 'White Glove Delivery',
              cost: 250,
              estimatedDays: '7-14 business days',
              locations: ['United States', 'Canada'],
            }}
            returnPolicy={{
              accepted: true,
              period: 14,
              conditions:
                'Item must be returned in original condition. Buyer responsible for return shipping costs. Refund will be processed within 5 business days of receiving the item.',
            }}
          />
        </Box>

        <Box sx={{ mt: 6 }}>
          <Typography
            variant="h5"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 600,
              color: palette.neutral[900],
              mb: 3,
            }}
          >
            Seller Reviews
          </Typography>
          <ReviewsSection sellerId={auction.seller.id} auctionId={auction.id} />
        </Box>
      </Container>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <InlineAlert
          severity={snackbar.severity}
          sx={{ width: '100%' }}
        >
          {snackbar.message}
        </InlineAlert>
      </Snackbar>
    </Box>
  )
}

function AuctionDetailPageSkeleton() {
  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh', pb: 8 }}>
      <Container maxWidth="xl" sx={{ pt: 3 }}>
        <Skeleton width={300} height={20} sx={{ mb: 3 }} />

        <Grid container spacing={4}>
          <Grid size={{ xs: 12, lg: 7 }}>
            <ImageGallerySkeleton />
          </Grid>

          <Grid size={{ xs: 12, lg: 5 }}>
            <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
              <Skeleton width={80} height={24} sx={{ borderRadius: 3 }} />
            </Stack>
            <Skeleton width="100%" height={40} sx={{ mb: 1 }} />
            <Skeleton width="80%" height={40} sx={{ mb: 1 }} />
            <Skeleton width={150} height={20} sx={{ mb: 3 }} />
            <BidSectionSkeleton />
            <Box sx={{ mt: 3 }}>
              <SellerInfoSkeleton />
            </Box>
          </Grid>
        </Grid>

        <Box sx={{ mt: 6 }}>
          <ProductTabsSkeleton />
        </Box>
      </Container>
    </Box>
  )
}
