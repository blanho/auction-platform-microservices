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
import { Timer, Favorite, FavoriteBorder, East, Refresh } from '@mui/icons-material'
import { useFeaturedAuctions, useWatchlist, useToggleWatchlist } from '../../../auctions/hooks'
import { formatTimeLeft, formatCurrency } from '../../../auctions/utils'
import { colors, typography } from '@/shared/theme/tokens'

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
  const watchlistIds = new Set(
    Array.isArray(watchlistData) ? watchlistData.map((item) => item.auctionId) : []
  )
  const toggleWatchlist = useToggleWatchlist()

  const handleToggleWatchlist = (auctionId: string) => {
    toggleWatchlist.mutate({
      auctionId,
      isInWatchlist: watchlistIds.has(auctionId),
    })
  }

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FFFFFF', position: 'relative' }}>
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, height: '1px', bgcolor: '#E7E5E4' }} />

      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
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
              <Typography
                sx={{
                  color: '#78716C',
                  letterSpacing: '0.2em',
                  fontFamily: typography.fontFamily.body,
                  fontSize: '0.6875rem',
                  fontWeight: typography.fontWeight.medium,
                  textTransform: 'uppercase',
                  mb: 2,
                }}
              >
                Featured
              </Typography>
              <Typography
                variant="h2"
                sx={{
                  fontFamily: typography.fontFamily.display,
                  color: '#1C1917',
                  fontWeight: typography.fontWeight.regular,
                  fontSize: { xs: '1.75rem', md: '2.5rem' },
                }}
              >
                Live Auctions
              </Typography>
            </Box>
            <Button
              endIcon={<East />}
              component={Link}
              to="/auctions"
              sx={{
                color: '#1C1917',
                textTransform: 'uppercase',
                fontWeight: typography.fontWeight.medium,
                fontFamily: typography.fontFamily.body,
                fontSize: '0.75rem',
                letterSpacing: '0.1em',
                '&:hover': { color: '#78716C' },
              }}
            >
              View All
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
                  sx={{ aspectRatio: '3/4', bgcolor: '#F5F5F4' }}
                />
              </Grid>
            ))
          )}
          {!isError && !featuredLoading && featuredAuctions.length === 0 && (
            <Grid size={{ xs: 12 }}>
              <Typography sx={{ color: '#A8A29E', textAlign: 'center', py: 8 }}>
                No featured auctions available
              </Typography>
            </Grid>
          )}
          {!isError && !featuredLoading && featuredAuctions.length > 0 && (
            featuredAuctions.map((auction, index) => (
              <Grid size={{ xs: 12, sm: 6, lg: 3 }} key={auction.id}>
                <motion.div
                  initial={{ opacity: 0, y: 25 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1, duration: 0.5 }}
                >
                  <Box
                    sx={{
                      cursor: 'pointer',
                      transition: 'opacity 0.3s ease',
                      '&:hover': { opacity: 0.9 },
                      '&:hover .auction-image': { transform: 'scale(1.03)' },
                    }}
                  >
                    <Box sx={{ position: 'relative', overflow: 'hidden', mb: 2 }}>
                      {auction.primaryImageUrl ? (
                        <Box
                          component="img"
                          className="auction-image"
                          src={auction.primaryImageUrl}
                          alt={auction.title}
                          sx={{
                            width: '100%',
                            aspectRatio: '3/4',
                            objectFit: 'cover',
                            transition: 'transform 0.6s ease',
                            display: 'block',
                          }}
                        />
                      ) : (
                        <Box
                          className="auction-image"
                          sx={{
                            width: '100%',
                            aspectRatio: '3/4',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            bgcolor: '#F5F5F4',
                            color: '#A8A29E',
                            fontFamily: typography.fontFamily.display,
                            fontSize: '2.5rem',
                            fontWeight: typography.fontWeight.light,
                            transition: 'transform 0.6s ease',
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
                          bgcolor: 'rgba(255,255,255,0.9)',
                          '&:hover': { bgcolor: '#FFFFFF' },
                        }}
                      >
                        {watchlistIds.has(auction.id) ? (
                          <Favorite sx={{ color: colors.accent.red, fontSize: 18 }} />
                        ) : (
                          <FavoriteBorder sx={{ color: '#1C1917', fontSize: 18 }} />
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
                          bgcolor: 'rgba(255,255,255,0.9)',
                          color: '#1C1917',
                          fontWeight: typography.fontWeight.medium,
                          fontSize: '0.7rem',
                          '& .MuiChip-icon': { color: '#1C1917' },
                        }}
                      />
                    </Box>
                    <Typography
                      sx={{
                        color: '#78716C',
                        letterSpacing: '0.1em',
                        textTransform: 'uppercase',
                        fontSize: '0.625rem',
                        fontWeight: typography.fontWeight.medium,
                        mb: 0.5,
                      }}
                    >
                      {auction.categoryName}
                    </Typography>
                    <Typography
                      component={Link}
                      to={`/auctions/${auction.id}`}
                      sx={{
                        color: '#1C1917',
                        fontWeight: typography.fontWeight.medium,
                        fontFamily: typography.fontFamily.body,
                        textDecoration: 'none',
                        display: 'block',
                        lineHeight: 1.4,
                        fontSize: '0.9rem',
                        mb: 1,
                        '&:hover': { textDecoration: 'underline' },
                      }}
                    >
                      {auction.title}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Typography
                        sx={{
                          color: '#1C1917',
                          fontWeight: typography.fontWeight.semibold,
                          fontSize: '0.9rem',
                        }}
                      >
                        {formatCurrency(auction.currentBid || auction.startingPrice)}
                      </Typography>
                      <Typography
                        sx={{
                          color: '#A8A29E',
                          fontSize: '0.75rem',
                        }}
                      >
                        {auction.bidCount} bids
                      </Typography>
                    </Box>
                  </Box>
                </motion.div>
              </Grid>
            ))
          )}
        </Grid>
      </Container>
    </Box>
  )
}
