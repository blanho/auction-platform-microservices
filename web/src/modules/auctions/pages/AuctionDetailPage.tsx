import { useAuth } from '@/app/providers'
import { getCurrentLocale } from '@/i18n'
import { useAutoBidForAuction, useBidsForAuction } from '@/modules/bidding/hooks'
import { ReviewsSection } from '@/modules/users/components/ReviewsSection'
import { palette } from '@/shared/theme/tokens'
import { ErrorState, InlineAlert } from '@/shared/ui'
import {
  ContentCopy,
  Facebook,
  NavigateNext,
  Pinterest,
  Twitter,
  Visibility,
} from '@mui/icons-material'
import {
  Box,
  Breadcrumbs,
  Button,
  Chip,
  CircularProgress,
  Container,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  Skeleton,
  Snackbar,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { BidSection, BidSectionSkeleton } from '../components/BidSection'
import { ImageGallery, ImageGallerySkeleton } from '../components/ImageGallery'
import { ProductTabs, ProductTabsSkeleton } from '../components/ProductTabs'
import { SellerInfo, SellerInfoSkeleton } from '../components/SellerInfo'
import { useAuction } from '../hooks'
import { useAuctionDetailActions } from '../hooks/useAuctionDetailActions'
import { useAuctionSignalR } from '../hooks/useAuctionSignalR'
import { useRecordView } from '../hooks/useViews'

export function AuctionDetailPage() {
  const { t } = useTranslation('auctions')
  const { id } = useParams<{ id: string }>()
  const { data: auction, isLoading, isError, refetch } = useAuction(id ?? '')
  const { isAuthenticated } = useAuth()
  const { data: autoBid } = useAutoBidForAuction(id, isAuthenticated && !isLoading)
  const { data: bidsData } = useBidsForAuction(id ?? '')
  const bids =
    bidsData?.items.map((bid) => ({
      id: bid.id,
      amount: bid.amount,
      bidderName: bid.bidderUsername,
      createdAt: bid.bidTime,
    })) ?? []
  const recordView = useRecordView()
  const recordedAuctionIdRef = useRef<string | null>(null)

  const {
    snackbar,
    buyNowDialogOpen,
    setBuyNowDialogOpen,
    buyNowMutation,
    isInWatchlist,
    handleToggleFavorite,
    handleShare,
    handlePlaceBid,
    handleBuyNow,
    confirmBuyNow,
  } = useAuctionDetailActions(id)

  useAuctionSignalR({ auctionId: id ?? '', enabled: !isLoading && !!id })

  useEffect(() => {
    if (id && !isLoading && auction && recordedAuctionIdRef.current !== id) {
      recordedAuctionIdRef.current = id
      recordView.mutate(id)
    }
  }, [id, isLoading, auction, recordView])

  if (isLoading || !auction) {
    if (isError) {
      return <ErrorState onRetry={() => void refetch()} showHomeButton />
    }
    return <AuctionDetailPageSkeleton />
  }

  const encodedShareUrl = encodeURIComponent(globalThis.location.href)
  const encodedShareTitle = encodeURIComponent(auction.title)
  const encodedShareImage = encodeURIComponent(auction.images[0]?.url ?? '')

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
          <Link to="/">{t('common:nav.home')}</Link>
          <Link to="/auctions">{t('title')}</Link>
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
              isFavorite={isInWatchlist}
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
                    label={t('endingSoon')}
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
                  <Typography variant="body2">
                    {auction.watcherCount} {t('detail.watching')}
                  </Typography>
                </Stack>

                <Stack direction="row" spacing={0.5}>
                  <Tooltip title={t('actions.shareOnFacebook')}>
                    <IconButton
                      component="a"
                      href={`https://www.facebook.com/sharer/sharer.php?u=${encodedShareUrl}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      aria-label={t('actions.shareOnFacebook')}
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#1877F2' } }}
                    >
                      <Facebook fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={t('actions.shareOnTwitter')}>
                    <IconButton
                      component="a"
                      href={`https://twitter.com/intent/tweet?url=${encodedShareUrl}&text=${encodedShareTitle}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      aria-label={t('actions.shareOnTwitter')}
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#1DA1F2' } }}
                    >
                      <Twitter fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={t('actions.pinOnPinterest')}>
                    <IconButton
                      component="a"
                      href={`https://pinterest.com/pin/create/button/?url=${encodedShareUrl}&media=${encodedShareImage}&description=${encodedShareTitle}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      aria-label={t('actions.pinOnPinterest')}
                      size="small"
                      sx={{ color: palette.neutral[500], '&:hover': { color: '#E60023' } }}
                    >
                      <Pinterest fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={t('actions.copyLink')}>
                    <IconButton
                      size="small"
                      onClick={handleShare}
                      aria-label={t('actions.copyLink')}
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
                buyNowPrice={auction.buyNowPrice}
                bidCount={Math.max(auction.bidCount, bids.length)}
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
                  onContact={() => snackbar.show(t('messages.openingChat'), 'info')}
                />
              </Box>
            </Box>
          </Grid>
        </Grid>

        <Box sx={{ mt: 6 }}>
          <ProductTabs
            description={auction.description}
            bids={bids}
            specifications={{
              ...(auction.condition ? { Condition: auction.condition } : {}),
              ...(auction.yearManufactured
                ? { 'Year manufactured': String(auction.yearManufactured) }
                : {}),
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
            {t('reviews.title')}
          </Typography>
          <ReviewsSection sellerId={auction.seller.id} auctionId={auction.id} />
        </Box>
      </Container>

      <Dialog
        open={buyNowDialogOpen}
        onClose={() => setBuyNowDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ fontFamily: '"Playfair Display", serif', fontWeight: 600 }}>
          {t('dialog.confirmPurchase')}
        </DialogTitle>
        <DialogContent>
          <Typography sx={{ color: palette.neutral[600], mb: 2 }}>
            {t('dialog.buyNowDescription')}
          </Typography>
          <Box
            sx={{
              p: 2,
              bgcolor: palette.neutral[50],
              borderRadius: 2,
              border: `1px solid ${palette.neutral[200]}`,
            }}
          >
            <Typography sx={{ fontWeight: 600, color: palette.neutral[900], mb: 1 }}>
              {auction?.title}
            </Typography>
            <Typography
              sx={{
                fontSize: '1.5rem',
                fontWeight: 700,
                color: palette.brand.primary,
              }}
            >
              {auction?.buyNowPrice?.toLocaleString(getCurrentLocale(), {
                style: 'currency',
                currency: 'USD',
                minimumFractionDigits: 0,
              })}
            </Typography>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button
            onClick={() => setBuyNowDialogOpen(false)}
            sx={{ color: palette.neutral[500], textTransform: 'none' }}
          >
            {t('common:actions.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={confirmBuyNow}
            disabled={buyNowMutation.isPending}
            sx={{
              bgcolor: palette.brand.primary,
              textTransform: 'none',
              fontWeight: 600,
              px: 4,
              '&:hover': { bgcolor: '#A16207' },
            }}
          >
            {buyNowMutation.isPending ? (
              <CircularProgress size={20} sx={{ color: 'white' }} />
            ) : (
              t('dialog.confirmPurchaseButton')
            )}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={snackbar.close}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <InlineAlert severity={snackbar.severity} sx={{ width: '100%' }}>
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
