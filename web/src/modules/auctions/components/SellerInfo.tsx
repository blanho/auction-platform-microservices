import {
  Box,
  Typography,
  Avatar,
  Stack,
  Button,
  Rating,
  Skeleton,
} from '@mui/material'
import { Verified, Storefront, Chat } from '@mui/icons-material'
import { Link } from 'react-router-dom'
import type { AuctionSellerInfo } from '../types'

interface SellerInfoProps {
  seller: AuctionSellerInfo
  onContact?: () => void
}

export function SellerInfo({ seller, onContact }: SellerInfoProps) {
  const isVerified = seller.totalSales >= 10

  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: '1px solid #E5E5E5',
        p: 3,
      }}
    >
      <Typography
        variant="overline"
        sx={{
          color: '#78716C',
          fontWeight: 600,
          letterSpacing: 1.5,
          fontSize: '0.6875rem',
        }}
      >
        Seller Information
      </Typography>

      <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
        <Link to={`/sellers/${seller.id}`}>
          <Avatar
            src={seller.avatarUrl}
            alt={seller.displayName}
            sx={{
              width: 56,
              height: 56,
              cursor: 'pointer',
              transition: 'transform 0.2s ease',
              '&:hover': {
                transform: 'scale(1.05)',
              },
            }}
          >
            {seller.displayName.charAt(0).toUpperCase()}
          </Avatar>
        </Link>

        <Box sx={{ flex: 1 }}>
          <Stack direction="row" alignItems="center" spacing={0.5}>
            <Typography
              component={Link}
              to={`/sellers/${seller.id}`}
              sx={{
                fontWeight: 600,
                color: '#1C1917',
                textDecoration: 'none',
                '&:hover': {
                  textDecoration: 'underline',
                },
              }}
            >
              {seller.displayName}
            </Typography>
            {isVerified && (
              <Verified sx={{ fontSize: 18, color: '#2563EB' }} />
            )}
          </Stack>

          <Typography variant="body2" sx={{ color: '#78716C', fontSize: '0.8125rem' }}>
            @{seller.username}
          </Typography>

          <Stack direction="row" alignItems="center" spacing={1} sx={{ mt: 0.5 }}>
            <Rating
              value={seller.rating}
              precision={0.1}
              size="small"
              readOnly
              sx={{
                color: '#CA8A04',
                '& .MuiRating-iconEmpty': {
                  color: '#E5E5E5',
                },
              }}
            />
            <Typography variant="body2" sx={{ color: '#78716C', fontSize: '0.8125rem' }}>
              ({seller.rating.toFixed(1)}{seller.reviewCount !== undefined && ` • ${seller.reviewCount} reviews`})
            </Typography>
          </Stack>
        </Box>
      </Stack>

      <Stack
        direction="row"
        spacing={3}
        sx={{
          mt: 2,
          pt: 2,
          borderTop: '1px solid #F5F5F4',
        }}
      >
        <Box>
          <Typography
            variant="h6"
            sx={{
              fontWeight: 600,
              color: '#1C1917',
              fontSize: '1.125rem',
            }}
          >
            {seller.totalSales.toLocaleString()}
          </Typography>
          <Typography variant="body2" sx={{ color: '#78716C', fontSize: '0.75rem' }}>
            Total Sales
          </Typography>
        </Box>
        <Box>
          <Typography
            variant="h6"
            sx={{
              fontWeight: 600,
              color: '#1C1917',
              fontSize: '1.125rem',
            }}
          >
            {Math.round(seller.rating * 20)}%
          </Typography>
          <Typography variant="body2" sx={{ color: '#78716C', fontSize: '0.75rem' }}>
            Positive Feedback
          </Typography>
        </Box>
      </Stack>

      <Stack direction="row" spacing={1} sx={{ mt: 3 }}>
        <Button
          component={Link}
          to={`/sellers/${seller.id}`}
          variant="outlined"
          size="small"
          startIcon={<Storefront />}
          sx={{
            flex: 1,
            textTransform: 'none',
            borderColor: '#D4D4D4',
            color: '#1C1917',
            '&:hover': {
              borderColor: '#1C1917',
              bgcolor: '#FAFAF9',
            },
          }}
        >
          View Shop
        </Button>
        {onContact && (
          <Button
            variant="outlined"
            size="small"
            startIcon={<Chat />}
            onClick={onContact}
            sx={{
              flex: 1,
              textTransform: 'none',
              borderColor: '#D4D4D4',
              color: '#1C1917',
              '&:hover': {
                borderColor: '#1C1917',
                bgcolor: '#FAFAF9',
              },
            }}
          >
            Contact
          </Button>
        )}
      </Stack>
    </Box>
  )
}

export function SellerInfoSkeleton() {
  return (
    <Box
      sx={{
        bgcolor: 'white',
        borderRadius: 2,
        border: '1px solid #E5E5E5',
        p: 3,
      }}
    >
      <Skeleton width={100} height={16} />
      <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
        <Skeleton variant="circular" width={56} height={56} />
        <Box sx={{ flex: 1 }}>
          <Skeleton width={120} height={20} />
          <Skeleton width={80} height={16} sx={{ mt: 0.5 }} />
          <Skeleton width={100} height={20} sx={{ mt: 0.5 }} />
        </Box>
      </Stack>
      <Stack direction="row" spacing={3} sx={{ mt: 2, pt: 2, borderTop: '1px solid #F5F5F4' }}>
        <Box>
          <Skeleton width={60} height={24} />
          <Skeleton width={70} height={14} />
        </Box>
        <Box>
          <Skeleton width={50} height={24} />
          <Skeleton width={80} height={14} />
        </Box>
      </Stack>
      <Stack direction="row" spacing={1} sx={{ mt: 3 }}>
        <Skeleton variant="rectangular" height={36} sx={{ flex: 1, borderRadius: 1 }} />
        <Skeleton variant="rectangular" height={36} sx={{ flex: 1, borderRadius: 1 }} />
      </Stack>
    </Box>
  )
}
