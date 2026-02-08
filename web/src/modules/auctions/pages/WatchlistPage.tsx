import { useState, useMemo } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import {
  Container,
  Typography,
  Box,
  Grid,
  Card,
  Button,
  IconButton,
  Skeleton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  FormControl,
  InputLabel,
  Select,
  Divider,
} from '@mui/material'
import { InlineAlert, StatusBadge } from '@/shared/ui'
import {
  Favorite,
  FavoriteBorder,
  Timer,
  Gavel,
  MoreVert,
  Delete,
  Visibility,
  NotificationsActive,
  NotificationsOff,
  GridView,
  ViewList,
  Sort,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { useWatchlist, useRemoveFromWatchlist } from '../hooks'
import type { WatchlistItem } from '../api/bookmarks.api'
import { fadeInUp, staggerContainer, staggerItem, cardHover } from '@/shared/lib/animations'
import { formatTimeLeft } from '../utils'

interface WatchlistCardProps {
  item: WatchlistItem
  onRemove: (auctionId: string) => void
  isRemoving: boolean
}

function WatchlistCard({ item, onRemove, isRemoving }: Readonly<WatchlistCardProps>) {
  const auction = item.auction
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [notificationsEnabled, setNotificationsEnabled] = useState(true)

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
  }

  const toggleNotifications = () => {
    setNotificationsEnabled((prev) => !prev)
    handleMenuClose()
  }

  return (
    <motion.div
      variants={staggerItem}
      layout
      initial={{ opacity: 0, scale: 0.9 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 0.9, transition: { duration: 0.2 } }}
    >
      <Card
        component={motion.div}
        whileHover={cardHover.hover}
        sx={{
          height: '100%',
          display: 'flex',
          flexDirection: 'column',
          cursor: 'pointer',
          position: 'relative',
          overflow: 'visible',
        }}
      >
        <Box sx={{ position: 'relative' }}>
          <Box
            sx={{
              height: 200,
              bgcolor: 'grey.100',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              backgroundImage: auction.imageUrl ? `url(${auction.imageUrl})` : undefined,
              backgroundSize: 'cover',
              backgroundPosition: 'center',
            }}
          >
            {!auction.imageUrl && <Gavel sx={{ fontSize: 64, color: 'grey.400' }} />}
          </Box>

          <IconButton
            size="small"
            onClick={(e) => {
              e.preventDefault()
              onRemove(auction.id)
            }}
            disabled={isRemoving}
            sx={{
              position: 'absolute',
              top: 8,
              right: 8,
              bgcolor: 'background.paper',
              boxShadow: 1,
              '&:hover': { bgcolor: 'error.light', color: 'white' },
            }}
          >
            <Favorite sx={{ color: 'error.main' }} fontSize="small" />
          </IconButton>

          <StatusBadge
            status={auction.status.replace('-', ' ')}
            sx={{
              position: 'absolute',
              top: 8,
              left: 8,
              textTransform: 'capitalize',
            }}
          />

          {auction.status === 'ending-soon' && (
            <Box
              sx={{
                position: 'absolute',
                bottom: 0,
                left: 0,
                right: 0,
                bgcolor: 'warning.main',
                color: 'warning.contrastText',
                py: 0.5,
                px: 2,
                display: 'flex',
                alignItems: 'center',
                gap: 1,
              }}
            >
              <Timer fontSize="small" />
              <Typography variant="caption" fontWeight={600}>
                {formatTimeLeft(auction.endTime)}
              </Typography>
            </Box>
          )}
        </Box>

        <Box
          component={Link}
          to={`/auctions/${auction.id}`}
          sx={{
            flexGrow: 1,
            display: 'flex',
            flexDirection: 'column',
            p: 2,
            textDecoration: 'none',
            color: 'inherit',
          }}
        >
          <Typography variant="subtitle1" fontWeight={600} noWrap gutterBottom>
            {auction.title}
          </Typography>

          {auction.status !== 'ending-soon' && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
              <Timer fontSize="small" color="action" />
              <Typography variant="body2" color="text.secondary">
                {formatTimeLeft(auction.endTime)}
              </Typography>
            </Box>
          )}

          <Box sx={{ mt: 'auto' }}>
            <Typography variant="h6" fontWeight={700} color="primary.main">
              ${auction.currentPrice.toLocaleString()}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {auction.bidCount} bids
            </Typography>
          </Box>
        </Box>

        <Divider />

        <Box
          sx={{ p: 1.5, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
        >
          <Button
            size="small"
            variant="contained"
            component={Link}
            to={`/auctions/${auction.id}`}
            sx={{ flexGrow: 1, mr: 1 }}
          >
            Place Bid
          </Button>
          <IconButton size="small" onClick={handleMenuOpen}>
            <MoreVert fontSize="small" />
          </IconButton>
          <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={handleMenuClose}>
            <MenuItem component={Link} to={`/auctions/${auction.id}`}>
              <ListItemIcon>
                <Visibility fontSize="small" />
              </ListItemIcon>
              <ListItemText>View Details</ListItemText>
            </MenuItem>
            <MenuItem onClick={toggleNotifications}>
              <ListItemIcon>
                {notificationsEnabled ? (
                  <NotificationsOff fontSize="small" />
                ) : (
                  <NotificationsActive fontSize="small" />
                )}
              </ListItemIcon>
              <ListItemText>
                {notificationsEnabled ? 'Disable Alerts' : 'Enable Alerts'}
              </ListItemText>
            </MenuItem>
            <Divider />
            <MenuItem
              onClick={() => {
                onRemove(auction.id)
                handleMenuClose()
              }}
              sx={{ color: 'error.main' }}
            >
              <ListItemIcon>
                <Delete fontSize="small" color="error" />
              </ListItemIcon>
              <ListItemText>Remove</ListItemText>
            </MenuItem>
          </Menu>
        </Box>
      </Card>
    </motion.div>
  )
}

export function WatchlistPage() {
  const [sortBy, setSortBy] = useState<'ending-soon' | 'newest' | 'price-low' | 'price-high'>(
    'ending-soon'
  )
  const skeletonKeys = useMemo(
    () => Array.from({ length: 6 }, () => crypto.randomUUID()),
    []
  )
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid')

  const { data, isLoading, error } = useWatchlist()

  const removeMutation = useRemoveFromWatchlist()

  const handleRemove = (auctionId: string) => {
    removeMutation.mutate(auctionId)
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box sx={{ mb: 4 }}>
            <Typography
              variant="h4"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                color: 'primary.main',
                mb: 1,
              }}
            >
              My Watchlist
            </Typography>
            <Typography variant="body1" color="text.secondary">
              Keep track of auctions you're interested in
            </Typography>
          </Box>
        </motion.div>

        <motion.div variants={staggerItem}>
          <Card sx={{ p: 2, mb: 3 }}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                flexWrap: 'wrap',
                gap: 2,
              }}
            >
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <FormControl size="small" sx={{ minWidth: 160 }}>
                  <InputLabel>Sort By</InputLabel>
                  <Select
                    value={sortBy}
                    label="Sort By"
                    onChange={(e) => {
                      const value = e.target.value
                      if (
                        value === 'ending-soon' ||
                        value === 'newest' ||
                        value === 'price-low' ||
                        value === 'price-high'
                      ) {
                        setSortBy(value)
                      }
                    }}
                    startAdornment={<Sort sx={{ mr: 1, color: 'action.active' }} />}
                  >
                    <MenuItem value="ending-soon">Ending Soon</MenuItem>
                    <MenuItem value="newest">Recently Added</MenuItem>
                    <MenuItem value="price-low">Price: Low to High</MenuItem>
                    <MenuItem value="price-high">Price: High to Low</MenuItem>
                  </Select>
                </FormControl>
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography variant="body2" color="text.secondary" sx={{ mr: 1 }}>
                  {data?.length || 0} items
                </Typography>
                <IconButton
                  size="small"
                  onClick={() => setViewMode('grid')}
                  color={viewMode === 'grid' ? 'primary' : 'default'}
                >
                  <GridView />
                </IconButton>
                <IconButton
                  size="small"
                  onClick={() => setViewMode('list')}
                  color={viewMode === 'list' ? 'primary' : 'default'}
                >
                  <ViewList />
                </IconButton>
              </Box>
            </Box>
          </Card>
        </motion.div>

        {error && (
          <motion.div variants={staggerItem}>
            <InlineAlert severity="error" sx={{ mb: 3 }}>
              Failed to load watchlist. Please try again.
            </InlineAlert>
          </motion.div>
        )}

        {isLoading && (
          <Grid container spacing={3}>
            {skeletonKeys.map((key) => (
              <Grid key={key} size={{ xs: 12, sm: 6, md: 4 }}>
                <Card>
                  <Skeleton variant="rectangular" height={200} />
                  <Box sx={{ p: 2 }}>
                    <Skeleton variant="text" width="80%" />
                    <Skeleton variant="text" width="60%" />
                    <Skeleton variant="text" width="40%" />
                  </Box>
                </Card>
              </Grid>
            ))}
          </Grid>
        )}
        {!isLoading && data?.length === 0 && (
          <motion.div variants={staggerItem}>
            <Card sx={{ p: 6, textAlign: 'center' }}>
              <FavoriteBorder sx={{ fontSize: 64, color: 'grey.300', mb: 2 }} />
              <Typography variant="h6" gutterBottom>
                Your watchlist is empty
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Start adding auctions you're interested in to keep track of them
              </Typography>
              <Button variant="contained" component={Link} to="/auctions">
                Browse Auctions
              </Button>
            </Card>
          </motion.div>
        )}
        {!isLoading && data && data.length > 0 && (
          <AnimatePresence mode="popLayout">
            <Grid container spacing={3}>
              {data?.map((item: WatchlistItem) => (
                <Grid key={item.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <WatchlistCard
                    item={item}
                    onRemove={handleRemove}
                    isRemoving={removeMutation.isPending}
                  />
                </Grid>
              ))}
            </Grid>
          </AnimatePresence>
        )}
      </motion.div>
    </Container>
  )
}
