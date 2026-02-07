import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import {
  Box,
  Container,
  Typography,
  Button,
  Grid,
  Chip,
  IconButton,
  Skeleton,
  Alert,
} from '@mui/material'
import { Timer, Favorite, FavoriteBorder, KeyboardArrowRight, Refresh } from '@mui/icons-material'
import { useFeaturedAuctions, useWatchlist, useToggleWatchlist } from '../../../auctions/hooks'
import { formatTimeLeft, formatCurrency } from '../../../auctions/utils'
import { GlassCard } from '@/shared/components/ui'
import { colors, typography, transitions, shadows } from '@/shared/theme/tokens'

export const FeaturedAuctionsSection = () => {
  const {
    data: featuredData,
    isLoading: featuredLoading,
    isError,
    error,
    refetch,
  } = useFeaturedAuctions(4)
  const featuredAuctions = featuredData?.items ?? []

  const { data: watchlistData } = useWatchlist()
  const watchlistIds = new Set(watchlistData?.map((item) => item.auctionId) ?? [])
  const toggleWatchlist = useToggleWatchlist()

  const handleToggleWatchlist = (auctionId: string) => {
    toggleWatchlist.mutate({
      auctionId,
      isInWatchlist: watchlistIds.has(auctionId),
    })
  }

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: colors.background.secondary }}>
      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
        >
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-end',
              mb: 6,
              flexWrap: 'wrap',
              gap: 2,
            }}
          >
            <Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                <Typography
                  variant="overline"
                  sx={{
                    color: colors.gold.primary,
                    letterSpacing: 4,
                    fontFamily: typography.fontFamily.sans,
                  }}
                >
                  FEATURED
                </Typography>
                <Chip
                  icon={
                    <Box
                      sx={{
                        width: 6,
                        height: 6,
                        borderRadius: '50%',
                        bgcolor: colors.accent.green,
                        mr: -0.5,
                      }}
                    />
                  }
                  label="Live"
                  size="small"
                  sx={{
                    bgcolor: colors.accent.greenLight,
                    color: colors.accent.green,
                    fontWeight: typography.fontWeight.semibold,
                    height: 24,
                  }}
                />
              </Box>
              <Typography
                variant="h2"
                sx={{
                  fontFamily: typography.fontFamily.serif,
                  color: colors.text.primary,
                  fontWeight: typography.fontWeight.regular,
                  fontSize: { xs: '2rem', md: '3rem' },
                }}
              >
                Live Auctions
              </Typography>
            </Box>
            <Button
              endIcon={<KeyboardArrowRight />}
              component={Link}
              to="/auctions"
              sx={{
                color: colors.text.secondary,
                textTransform: 'none',
                fontWeight: typography.fontWeight.medium,
                fontFamily: typography.fontFamily.sans,
                '&:hover': { color: colors.gold.primary },
              }}
            >
              View All Auctions
            </Button>
          </Box>
        </motion.div>

        <Grid container spacing={3}>
          {isError ? (
            <Grid size={{ xs: 12 }}>
              <Alert
                severity="error"
                action={
                  <Button
                    color="inherit"
                    size="small"
                    startIcon={<Refresh />}
                    onClick={() => refetch()}
                  >
                    Retry
                  </Button>
                }
                sx={{
                  bgcolor: 'rgba(239, 68, 68, 0.1)',
                  color: colors.accent.red,
                  '& .MuiAlert-icon': { color: colors.accent.red },
                }}
              >
                Failed to load auctions.{' '}
                {error instanceof Error ? error.message : 'Please try again.'}
              </Alert>
            </Grid>
          ) : featuredLoading && (
            Array.from({ length: 4 }).map((_, index) => (
              <Grid size={{ xs: 12, sm: 6, lg: 3 }} key={index}>
                <Skeleton
                  variant="rectangular"
                  sx={{ aspectRatio: '1', borderRadius: 3, bgcolor: colors.border.light }}
                />
              </Grid>
            ))
          )}
          {!isError && !featuredLoading && featuredAuctions.length === 0 && (
            <Grid size={{ xs: 12 }}>
              <Typography color="text.secondary" align="center" sx={{ py: 8 }}>
                No featured auctions available
              </Typography>
            </Grid>
          )}
          {!isError && !featuredLoading && featuredAuctions.length > 0 && (
            featuredAuctions.map((auction, index) => (
              <Grid size={{ xs: 12, sm: 6, lg: 3 }} key={auction.id}>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1 }}
                >
                  <GlassCard
                    sx={{
                      overflow: 'hidden',
                      cursor: 'pointer',
                      transition: `transform ${transitions.normal}, box-shadow ${transitions.normal}`,
                      '&:hover': {
                        transform: 'translateY(-8px)',
                        boxShadow: shadows.card,
                      },
                      '&:hover .auction-image': { transform: 'scale(1.05)' },
                    }}
                  >
                    <Box sx={{ position: 'relative', overflow: 'hidden' }}>
                      {auction.primaryImageUrl ? (
                        <Box
                          component="img"
                          className="auction-image"
                          src={auction.primaryImageUrl}
                          alt={auction.title}
                          sx={{
                            width: '100%',
                            aspectRatio: '1',
                            objectFit: 'cover',
                            transition: `transform ${transitions.slower}`,
                          }}
                        />
                      ) : (
                        <Box
                          className="auction-image"
                          sx={{
                            width: '100%',
                            aspectRatio: '1',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            background: `linear-gradient(135deg, ${colors.background.secondary}, ${colors.border.light})`,
                            color: colors.text.secondary,
                            fontFamily: typography.fontFamily.display,
                            fontSize: '1.5rem',
                            transition: `transform ${transitions.slower}`,
                          }}
                        >
                          {auction.title.charAt(0)}
                        </Box>
                      )}
                      <IconButton
                        onClick={(e) => {
                          e.preventDefault()
                          handleToggleWatchlist(auction.id)
                        }}
                        disabled={toggleWatchlist.isPending}
                        sx={{
                          position: 'absolute',
                          top: 12,
                          right: 12,
                          bgcolor: colors.overlay.dark,
                          backdropFilter: 'blur(10px)',
                          '&:hover': { bgcolor: colors.overlay.darker },
                        }}
                      >
                        {watchlistIds.has(auction.id) ? (
                          <Favorite sx={{ color: colors.accent.red, fontSize: 20 }} />
                        ) : (
                          <FavoriteBorder sx={{ color: colors.text.primary, fontSize: 20 }} />
                        )}
                      </IconButton>
                      <Chip
                        icon={<Timer sx={{ fontSize: 14 }} />}
                        label={formatTimeLeft(auction.endTime)}
                        size="small"
                        sx={{
                          position: 'absolute',
                          bottom: 12,
                          left: 12,
                          bgcolor: colors.overlay.darkest,
                          color: colors.gold.light,
                          fontWeight: typography.fontWeight.semibold,
                          backdropFilter: 'blur(10px)',
                          '& .MuiChip-icon': { color: colors.gold.light },
                        }}
                      />
                    </Box>
                    <Box sx={{ p: 2.5 }}>
                      <Typography variant="caption" sx={{ color: colors.text.subtle }}>
                        {auction.categoryName}
                      </Typography>
                      <Typography
                        variant="subtitle1"
                        component={Link}
                        to={`/auctions/${auction.id}`}
                        sx={{
                          color: colors.text.primary,
                          fontWeight: typography.fontWeight.semibold,
                          fontFamily: typography.fontFamily.sans,
                          textDecoration: 'none',
                          display: 'block',
                          my: 1,
                          lineHeight: 1.4,
                          '&:hover': { color: colors.gold.primary },
                        }}
                      >
                        {auction.title}
                      </Typography>
                      <Box
                        sx={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center',
                          mt: 2,
                        }}
                      >
                        <Box>
                          <Typography variant="caption" sx={{ color: colors.text.subtle }}>
                            Current Bid
                          </Typography>
                          <Typography
                            variant="h6"
                            sx={{
                              color: colors.gold.primary,
                              fontWeight: typography.fontWeight.bold,
                            }}
                          >
                            {formatCurrency(auction.currentBid || auction.startingPrice)}
                          </Typography>
                        </Box>
                        <Chip
                          label={`${auction.bidCount} bids`}
                          size="small"
                          sx={{ bgcolor: colors.border.light, color: colors.text.secondary }}
                        />
                      </Box>
                    </Box>
                  </GlassCard>
                </motion.div>
              </Grid>
            ))
          )}
        </Grid>
      </Container>
    </Box>
  )
}
