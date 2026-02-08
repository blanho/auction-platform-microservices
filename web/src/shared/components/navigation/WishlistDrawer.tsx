import { Link } from 'react-router-dom'
import { motion, AnimatePresence } from 'framer-motion'
import {
  Drawer,
  Box,
  Typography,
  IconButton,
  Button,
  Skeleton,
  Divider,
} from '@mui/material'
import {
  Close,
  FavoriteBorder,
  Favorite,
  Timer,
  East,
} from '@mui/icons-material'
import { useWatchlist, useRemoveFromWatchlist, useWatchlistCount } from '@/modules/auctions/hooks'
import type { WatchlistItem } from '@/modules/auctions/api/bookmarks.api'
import { palette } from '@/shared/theme/tokens'
import { formatCurrency } from '@/shared/utils/formatters'

interface WishlistDrawerProps {
  open: boolean
  onClose: () => void
}

interface WishlistItemCardProps {
  item: WatchlistItem
  onRemove: (auctionId: string) => void
  isRemoving: boolean
  onClose: () => void
}

function formatTimeLeft(endTime: string): string {
  const end = new Date(endTime)
  const now = new Date()
  const diffMs = end.getTime() - now.getTime()

  if (diffMs <= 0) {
    return 'Ended'
  }

  const days = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))

  if (days > 0) {
    return `${days}d ${hours}h left`
  }
  if (hours > 0) {
    return `${hours}h ${minutes}m left`
  }
  return `${minutes}m left`
}

function WishlistItemCard({ item, onRemove, isRemoving, onClose }: Readonly<WishlistItemCardProps>) {
  const { auction } = item

  return (
    <motion.div
      initial={{ opacity: 0, x: 20 }}
      animate={{ opacity: 1, x: 0 }}
      exit={{ opacity: 0, x: -20 }}
      transition={{ duration: 0.2, ease: 'easeOut' }}
    >
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          py: 2.5,
          borderBottom: `1px solid ${palette.neutral[100]}`,
          '&:last-child': { borderBottom: 'none' },
        }}
      >
        <Box
          component={Link}
          to={`/auctions/${auction.id}`}
          onClick={onClose}
          sx={{
            width: 100,
            height: 120,
            flexShrink: 0,
            bgcolor: palette.neutral[50],
            backgroundImage: auction.imageUrl ? `url(${auction.imageUrl})` : undefined,
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            cursor: 'pointer',
            transition: 'opacity 0.2s ease',
            '&:hover': { opacity: 0.85 },
          }}
        >
          {!auction.imageUrl && (
            <FavoriteBorder sx={{ fontSize: 32, color: palette.neutral[300] }} />
          )}
        </Box>

        <Box sx={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column' }}>
          <Typography
            component={Link}
            to={`/auctions/${auction.id}`}
            onClick={onClose}
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontSize: '0.9375rem',
              fontWeight: 500,
              color: palette.neutral[900],
              textDecoration: 'none',
              lineHeight: 1.4,
              mb: 0.5,
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              transition: 'color 0.2s ease',
              '&:hover': { color: palette.neutral[600] },
            }}
          >
            {auction.title}
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
            <Timer sx={{ fontSize: 14, color: palette.neutral[500] }} />
            <Typography
              sx={{
                fontSize: '0.75rem',
                color: auction.status === 'ending-soon' ? palette.semantic.error : palette.neutral[500],
                fontWeight: auction.status === 'ending-soon' ? 600 : 400,
              }}
            >
              {formatTimeLeft(auction.endTime)}
            </Typography>
          </Box>

          <Box sx={{ mt: 'auto' }}>
            <Typography
              sx={{
                fontSize: '1rem',
                fontWeight: 600,
                color: palette.neutral[900],
                mb: 0.25,
              }}
            >
              {formatCurrency(auction.currentPrice)}
            </Typography>
            <Typography sx={{ fontSize: '0.75rem', color: palette.neutral[500] }}>
              {auction.bidCount} {auction.bidCount === 1 ? 'bid' : 'bids'}
            </Typography>
          </Box>
        </Box>

        <IconButton
          size="small"
          onClick={() => onRemove(auction.id)}
          disabled={isRemoving}
          sx={{
            alignSelf: 'flex-start',
            mt: 0.5,
            color: palette.semantic.error,
            transition: 'all 0.2s ease',
            '&:hover': {
              bgcolor: 'rgba(239, 68, 68, 0.08)',
              transform: 'scale(1.1)',
            },
          }}
        >
          <Favorite sx={{ fontSize: 18 }} />
        </IconButton>
      </Box>
    </motion.div>
  )
}

function WishlistSkeleton() {
  return (
    <Box sx={{ py: 2.5, borderBottom: `1px solid ${palette.neutral[100]}` }}>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <Skeleton variant="rectangular" width={100} height={120} />
        <Box sx={{ flex: 1 }}>
          <Skeleton variant="text" width="80%" height={20} />
          <Skeleton variant="text" width="50%" height={16} sx={{ mt: 0.5 }} />
          <Skeleton variant="text" width="40%" height={24} sx={{ mt: 'auto' }} />
        </Box>
      </Box>
    </Box>
  )
}

export function WishlistDrawer({ open, onClose }: Readonly<WishlistDrawerProps>) {
  const { data: watchlist, isLoading } = useWatchlist()
  const { data: watchlistCount } = useWatchlistCount()
  const removeMutation = useRemoveFromWatchlist()

  const handleRemove = (auctionId: string) => {
    removeMutation.mutate(auctionId)
  }

  const itemCount = watchlistCount ?? watchlist?.length ?? 0

  return (
    <Drawer
      anchor="right"
      open={open}
      onClose={onClose}
      PaperProps={{
        sx: {
          width: { xs: '100%', sm: 420 },
          maxWidth: '100%',
          boxShadow: '-8px 0 32px rgba(0, 0, 0, 0.12)',
        },
      }}
      slotProps={{
        backdrop: {
          sx: { bgcolor: 'rgba(0, 0, 0, 0.4)' },
        },
      }}
    >
      <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Box
          sx={{
            px: 3,
            py: 2.5,
            borderBottom: `1px solid ${palette.neutral[100]}`,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
          }}
        >
          <Box>
            <Typography
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontSize: '1.5rem',
                fontWeight: 500,
                fontStyle: 'italic',
                color: palette.neutral[900],
              }}
            >
              Wishlist
            </Typography>
            <Typography sx={{ fontSize: '0.8125rem', color: palette.neutral[500], mt: 0.25 }}>
              {itemCount} {itemCount === 1 ? 'item' : 'items'} saved
            </Typography>
          </Box>
          <IconButton
            onClick={onClose}
            sx={{
              color: palette.neutral[900],
              '&:hover': { bgcolor: palette.neutral[50] },
            }}
          >
            <Close />
          </IconButton>
        </Box>

        <Box sx={{ flex: 1, overflow: 'auto', px: 3 }}>
          {isLoading && (
            <>
              <WishlistSkeleton />
              <WishlistSkeleton />
              <WishlistSkeleton />
            </>
          )}

          {!isLoading && (!watchlist || watchlist.length === 0) && (
            <Box
              sx={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                py: 8,
              }}
            >
              <Box
                sx={{
                  width: 80,
                  height: 80,
                  borderRadius: '50%',
                  bgcolor: palette.neutral[50],
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  mb: 3,
                }}
              >
                <FavoriteBorder sx={{ fontSize: 36, color: palette.neutral[300] }} />
              </Box>
              <Typography
                sx={{
                  fontFamily: '"Playfair Display", serif',
                  fontSize: '1.125rem',
                  fontWeight: 500,
                  color: palette.neutral[900],
                  mb: 1,
                }}
              >
                Your wishlist is empty
              </Typography>
              <Typography
                sx={{
                  fontSize: '0.875rem',
                  color: palette.neutral[500],
                  textAlign: 'center',
                  maxWidth: 280,
                  mb: 3,
                }}
              >
                Save items you love by clicking the heart icon on any auction
              </Typography>
              <Button
                component={Link}
                to="/auctions"
                onClick={onClose}
                variant="outlined"
                sx={{
                  borderColor: palette.neutral[900],
                  color: palette.neutral[900],
                  borderRadius: 0,
                  px: 4,
                  py: 1.25,
                  fontSize: '0.875rem',
                  fontWeight: 500,
                  textTransform: 'none',
                  '&:hover': {
                    borderColor: palette.neutral[900],
                    bgcolor: palette.neutral[900],
                    color: palette.neutral[0],
                  },
                }}
              >
                Explore Auctions
              </Button>
            </Box>
          )}

          {!isLoading && watchlist && watchlist.length > 0 && (
            <AnimatePresence mode="popLayout">
              {watchlist.map((item) => (
                <WishlistItemCard
                  key={item.id}
                  item={item}
                  onRemove={handleRemove}
                  isRemoving={removeMutation.isPending}
                  onClose={onClose}
                />
              ))}
            </AnimatePresence>
          )}
        </Box>

        {!isLoading && watchlist && watchlist.length > 0 && (
          <Box
            sx={{
              p: 3,
              borderTop: `1px solid ${palette.neutral[100]}`,
              bgcolor: palette.neutral[0],
            }}
          >
            <Button
              component={Link}
              to="/watchlist"
              onClick={onClose}
              fullWidth
              variant="contained"
              endIcon={<East />}
              sx={{
                bgcolor: palette.neutral[900],
                color: palette.neutral[0],
                borderRadius: 0,
                py: 1.5,
                fontSize: '0.9375rem',
                fontWeight: 500,
                textTransform: 'none',
                boxShadow: 'none',
                '&:hover': {
                  bgcolor: palette.neutral[800],
                  boxShadow: 'none',
                },
              }}
            >
              View All Saved Items
            </Button>
          </Box>
        )}
      </Box>
    </Drawer>
  )
}
