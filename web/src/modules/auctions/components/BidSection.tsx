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
  Alert,
  CircularProgress,
  Skeleton,
} from '@mui/material'
import {
  Gavel,
  Timer,
  TrendingUp,
  LocalOffer,
  InfoOutlined,
} from '@mui/icons-material'

interface BidSectionProps {
  auctionId: string
  currentBid: number
  startingPrice: number
  reservePrice?: number
  buyNowPrice?: number
  bidCount: number
  endTime: string
  status: string
  userBid?: {
    amount: number
    isWinning: boolean
  }
  onPlaceBid: (amount: number) => Promise<void>
  onBuyNow?: () => Promise<void>
}

export function BidSection({
  currentBid,
  startingPrice,
  bidCount,
  endTime,
  status,
  userBid,
  buyNowPrice,
  onPlaceBid,
  onBuyNow,
}: BidSectionProps) {
  const [bidAmount, setBidAmount] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [timeLeft, setTimeLeft] = useState('')
  const [isUrgent, setIsUrgent] = useState(false)

  const minimumBid = useMemo(() => {
    const base = currentBid > 0 ? currentBid : startingPrice
    const increment = base < 100 ? 1 : base < 1000 ? 5 : base < 5000 ? 25 : 100
    return base + increment
  }, [currentBid, startingPrice])

  const suggestedBids = useMemo(() => {
    const base = minimumBid
    const increment = base < 100 ? 5 : base < 1000 ? 25 : base < 5000 ? 100 : 500
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
    const amount = parseFloat(bidAmount)
    if (isNaN(amount) || amount < minimumBid) {
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
        border: '1px solid #E5E5E5',
        p: 3,
      }}
    >
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          mb: 1,
          color: isUrgent ? '#DC2626' : '#44403C',
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
          color: '#1C1917',
          mb: 0.5,
        }}
      >
        ${currentBid > 0 ? currentBid.toLocaleString() : startingPrice.toLocaleString()}
      </Typography>

      <Stack direction="row" spacing={2} alignItems="center" sx={{ mb: 3 }}>
        <Typography variant="body2" sx={{ color: '#78716C' }}>
          {bidCount} {bidCount === 1 ? 'bid' : 'bids'}
        </Typography>
        {currentBid === 0 && (
          <Chip
            label="No bids yet"
            size="small"
            sx={{
              bgcolor: '#FEF3C7',
              color: '#92400E',
              fontSize: '0.75rem',
            }}
          />
        )}
      </Stack>

      {userBid && (
        <Alert
          severity={userBid.isWinning ? 'success' : 'warning'}
          icon={userBid.isWinning ? <TrendingUp /> : <InfoOutlined />}
          sx={{ mb: 2 }}
        >
          {userBid.isWinning
            ? `You're the highest bidder at $${userBid.amount.toLocaleString()}`
            : `You've been outbid. Your bid: $${userBid.amount.toLocaleString()}`}
        </Alert>
      )}

      <Collapse in={!!error}>
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      </Collapse>

      <Collapse in={!!success}>
        <Alert severity="success" onClose={() => setSuccess(null)} sx={{ mb: 2 }}>
          {success}
        </Alert>
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
            InputProps={{
              startAdornment: <InputAdornment position="start">$</InputAdornment>,
            }}
            sx={{
              mb: 1.5,
              '& .MuiOutlinedInput-root': {
                '&:hover fieldset': {
                  borderColor: '#1C1917',
                },
                '&.Mui-focused fieldset': {
                  borderColor: '#1C1917',
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
                  borderColor: '#D4D4D4',
                  '&:hover': {
                    bgcolor: '#FAFAF9',
                    borderColor: '#1C1917',
                  },
                  ...(bidAmount === amount.toString() && {
                    bgcolor: '#1C1917',
                    color: 'white',
                    '&:hover': {
                      bgcolor: '#44403C',
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
              bgcolor: '#1C1917',
              color: 'white',
              py: 1.5,
              fontSize: '1rem',
              fontWeight: 600,
              textTransform: 'none',
              borderRadius: 1,
              '&:hover': {
                bgcolor: '#44403C',
              },
              '&.Mui-disabled': {
                bgcolor: '#E5E5E5',
                color: '#A3A3A3',
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
                borderColor: '#CA8A04',
                color: '#CA8A04',
                '&:hover': {
                  borderColor: '#A16207',
                  bgcolor: '#FFFBEB',
                },
              }}
            >
              Buy Now — ${buyNowPrice.toLocaleString()}
            </Button>
          )}
        </>
      )}

      {!isAuctionActive && (
        <Alert severity="info">
          This auction has ended.
        </Alert>
      )}

      <Box
        sx={{
          mt: 3,
          pt: 2,
          borderTop: '1px solid #E5E5E5',
        }}
      >
        <Typography variant="body2" sx={{ color: '#78716C', fontSize: '0.8125rem' }}>
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
