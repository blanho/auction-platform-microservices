import { useState, useEffect, useMemo } from 'react'
import {
  Box,
  Typography,
  Button,
  TextField,
  InputAdornment,
  Stack,
  Chip,
  Collapse,
  CircularProgress,
  Skeleton,
} from '@mui/material'
import { InlineAlert } from '@/shared/ui'
import { Gavel, Timer, LocalOffer, AutoMode } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { AutoBidDialog } from '@/modules/bidding/components/AutoBidDialog'

interface BidSectionProps {
  auctionId: string
  auctionTitle: string
  currentBid: number
  startingPrice: number
  buyNowPrice?: number
  bidCount: number
  endTime: string
  status: string
  minBidIncrement?: number
  userBid?: {
    amount: number
    isWinning: boolean
  }
  existingAutoBid?: {
    maxAmount: number
    isActive: boolean
  }
  onPlaceBid: (amount: number) => Promise<void>
  onBuyNow?: () => Promise<void>
}

export function BidSection({
  auctionId,
  auctionTitle,
  currentBid,
  startingPrice,
  bidCount,
  endTime,
  status,
  minBidIncrement = 1,
  userBid,
  buyNowPrice,
  existingAutoBid,
  onPlaceBid,
  onBuyNow,
}: Readonly<BidSectionProps>) {
  const [bidAmount, setBidAmount] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [timeLeft, setTimeLeft] = useState('')
  const [isUrgent, setIsUrgent] = useState(false)
  const [autoBidDialogOpen, setAutoBidDialogOpen] = useState(false)

  const minimumBid = useMemo(() => {
    const base = currentBid > 0 ? currentBid : startingPrice

    const getIncrement = (amount: number): number => {
      if (amount < 100) {return 1}
      if (amount < 1000) {return 5}
      if (amount < 5000) {return 25}
      return 100
    }

    return base + getIncrement(base)
  }, [currentBid, startingPrice])

  const suggestedBids = useMemo(() => {
    const base = minimumBid

    const getSuggestedIncrement = (amount: number): number => {
      if (amount < 100) {return 5}
      if (amount < 1000) {return 25}
      if (amount < 5000) {return 100}
      return 500
    }

    const increment = getSuggestedIncrement(base)
    return [base, base + increment, base + increment * 2]
  }, [minimumBid])

  useEffect(() => {
    const calculateTimeLeft = () => {
      const end = new Date(endTime).getTime()
      const now = Date.now()
      const diff = end - now

      if (diff <= 0) {
        setTimeLeft('Auction ended')
        setIsUrgent(false)
        return
      }

      const days = Math.floor(diff / (1000 * 60 * 60 * 24))
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60))
      const seconds = Math.floor((diff % (1000 * 60)) / 1000)

      setIsUrgent(diff < 1000 * 60 * 60)

      if (days > 0) {
        setTimeLeft(`${days}d ${hours}h ${minutes}m`)
      } else if (hours > 0) {
        setTimeLeft(`${hours}h ${minutes}m ${seconds}s`)
      } else if (minutes > 0) {
        setTimeLeft(`${minutes}m ${seconds}s`)
      } else {
        setTimeLeft(`${seconds}s`)
      }
    }

    calculateTimeLeft()
    const interval = setInterval(calculateTimeLeft, 1000)
    return () => clearInterval(interval)
  }, [endTime])

  const handleSubmitBid = async () => {
    const amount = Number.parseFloat(bidAmount)
    if (Number.isNaN(amount) || amount < minimumBid) {
      setError(`Minimum bid is $${minimumBid.toLocaleString()}`)
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      await onPlaceBid(amount)
      setSuccess(`Bid of $${amount.toLocaleString()} placed successfully!`)
      setBidAmount('')
    } catch {
      setError('Failed to place bid. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleQuickBid = (amount: number) => {
    setBidAmount(amount.toString())
  }

  const isAuctionActive = status === 'active' || status === 'ending-soon'

  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: `1px solid ${palette.neutral[100]}`,
        p: 3,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          mb: 1,
          color: isUrgent ? palette.semantic.error : palette.neutral[700],
        }}
      >
        <Timer fontSize="small" />
        <Typography
          variant="body2"
          sx={{
            fontWeight: 500,
            animation: isUrgent ? 'pulse 1s infinite' : 'none',
            '@keyframes pulse': {
              '0%, 100%': { opacity: 1 },
              '50%': { opacity: 0.6 },
            },
          }}
        >
          {isAuctionActive ? `Ends in ${timeLeft}` : timeLeft}
        </Typography>
      </Box>

      <Typography
        variant="h3"
        sx={{
          fontFamily: '"Playfair Display", serif',
          fontWeight: 600,
          color: palette.neutral[900],
          mb: 0.5,
        }}
      >
        ${currentBid > 0 ? currentBid.toLocaleString() : startingPrice.toLocaleString()}
      </Typography>

      <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Typography variant="body2" sx={{ color: palette.neutral[500] }}>
          {bidCount} {bidCount === 1 ? 'bid' : 'bids'}
        </Typography>
        {currentBid === 0 && (
          <Chip
            label="No bids yet"
            size="small"
            sx={{
              bgcolor: palette.brand.muted,
              color: palette.brand.dark,
              fontSize: '0.75rem',
            }}
          />
        )}
      </Stack>

      {userBid && (
        <InlineAlert
          severity={userBid.isWinning ? 'success' : 'warning'}
          sx={{ mb: 2 }}
        >
          {userBid.isWinning
            ? `You're the highest bidder at $${userBid.amount.toLocaleString()}`
            : `You've been outbid. Your bid: $${userBid.amount.toLocaleString()}`}
        </InlineAlert>
      )}

      <Collapse in={!!error}>
        <InlineAlert severity="error" sx={{ mb: 2 }}>
          {error}
        </InlineAlert>
      </Collapse>

      <Collapse in={!!success}>
        <InlineAlert severity="success" sx={{ mb: 2 }}>
          {success}
        </InlineAlert>
      </Collapse>

      {isAuctionActive && (
        <>
          <TextField
            fullWidth
            type="number"
            label="Your Bid"
            value={bidAmount}
            onChange={(e) => setBidAmount(e.target.value)}
            placeholder={`$${minimumBid.toLocaleString()} or more`}
            slotProps={{
              input: {
                startAdornment: <InputAdornment position="start">$</InputAdornment>,
              },
            }}
            sx={{
              mb: 1.5,
              '& .MuiOutlinedInput-root': {
                '&:hover fieldset': {
                  borderColor: palette.neutral[900],
                },
                '&.Mui-focused fieldset': {
                  borderColor: palette.neutral[900],
                },
              },
            }}
          />

          <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
            {suggestedBids.map((amount) => (
              <Chip
                key={amount}
                label={`$${amount.toLocaleString()}`}
                onClick={() => handleQuickBid(amount)}
                variant={bidAmount === amount.toString() ? 'filled' : 'outlined'}
                sx={{
                  cursor: 'pointer',
                  borderColor: palette.neutral[100],
                  '&:hover': {
                    bgcolor: palette.neutral[50],
                    borderColor: palette.neutral[900],
                  },
                  ...(bidAmount === amount.toString() && {
                    bgcolor: palette.neutral[900],
                    color: 'white',
                    '&:hover': {
                      bgcolor: palette.neutral[700],
                    },
                  }),
                }}
              />
            ))}
          </Stack>

          <Button
            fullWidth
            variant="contained"
            size="large"
            onClick={handleSubmitBid}
            disabled={isSubmitting || !bidAmount}
            startIcon={isSubmitting ? <CircularProgress size={20} color="inherit" /> : <Gavel />}
            sx={{
              bgcolor: palette.neutral[900],
              color: 'white',
              py: 1.5,
              fontSize: '1rem',
              fontWeight: 600,
              textTransform: 'none',
              borderRadius: 1,
              '&:hover': {
                bgcolor: palette.neutral[700],
              },
              '&.Mui-disabled': {
                bgcolor: palette.neutral[100],
                color: palette.neutral[500],
              },
            }}
          >
            {isSubmitting ? 'Placing Bid...' : 'Place Bid'}
          </Button>

          {buyNowPrice && onBuyNow && (
            <Button
              fullWidth
              variant="outlined"
              size="large"
              onClick={onBuyNow}
              startIcon={<LocalOffer />}
              sx={{
                mt: 1.5,
                py: 1.5,
                fontSize: '1rem',
                fontWeight: 600,
                textTransform: 'none',
                borderRadius: 1,
                borderColor: palette.brand.primary,
                color: palette.brand.primary,
                '&:hover': {
                  borderColor: palette.brand.hover,
                  bgcolor: palette.brand.muted,
                },
              }}
            >
              Buy Now — ${buyNowPrice.toLocaleString()}
            </Button>
          )}

          <Button
            fullWidth
            variant="outlined"
            size="large"
            onClick={() => setAutoBidDialogOpen(true)}
            startIcon={<AutoMode />}
            sx={{
              mt: 1.5,
              py: 1.5,
              fontSize: '1rem',
              fontWeight: 600,
              textTransform: 'none',
              borderRadius: 1,
              borderColor: existingAutoBid?.isActive
                ? palette.semantic.success
                : palette.neutral[100],
              color: existingAutoBid?.isActive ? palette.semantic.success : palette.neutral[700],
              bgcolor: existingAutoBid?.isActive ? palette.semantic.successLight : 'transparent',
              '&:hover': {
                borderColor: palette.neutral[900],
                bgcolor: palette.neutral[50],
              },
            }}
          >
            {existingAutoBid?.isActive
              ? `Auto-Bid Active (Max: $${existingAutoBid.maxAmount.toLocaleString()})`
              : 'Set Up Auto-Bid'}
          </Button>

          <AutoBidDialog
            open={autoBidDialogOpen}
            onClose={() => setAutoBidDialogOpen(false)}
            auctionId={auctionId}
            auctionTitle={auctionTitle}
            currentBid={currentBid || startingPrice}
            minBidIncrement={minBidIncrement}
            existingAutoBid={existingAutoBid}
          />
        </>
      )}

      {!isAuctionActive && <InlineAlert severity="info">This auction has ended.</InlineAlert>}

      <Box
        sx={{
          mt: 3,
          pt: 2,
          borderTop: `1px solid ${palette.neutral[100]}`,
        }}
      >
        <Typography variant="body2" sx={{ color: palette.neutral[500], fontSize: '0.8125rem' }}>
          By placing a bid, you agree to our terms of service. All bids are binding.
        </Typography>
      </Box>
    </Box>
  )
}

export function BidSectionSkeleton() {
  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: '1px solid #E5E5E5',
        p: 3,
      }}
    >
      <Skeleton width={120} height={20} sx={{ mb: 1 }} />
      <Skeleton width={180} height={48} sx={{ mb: 0.5 }} />
      <Skeleton width={80} height={20} sx={{ mb: 3 }} />
      <Skeleton variant="rectangular" height={56} sx={{ borderRadius: 1, mb: 1.5 }} />
      <Stack direction="row" spacing={1} sx={{ mb: 2 }}>
        {[1, 2, 3].map((i) => (
          <Skeleton key={i} width={80} height={32} sx={{ borderRadius: 4 }} />
        ))}
      </Stack>
      <Skeleton variant="rectangular" height={52} sx={{ borderRadius: 1 }} />
    </Box>
  )
}
