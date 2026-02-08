import { memo } from 'react'
import { Box, Card, CardContent, CardMedia, Typography, Button } from '@mui/material'
import { Link } from 'react-router-dom'
import { Timer, Gavel } from '@mui/icons-material'
import type { AuctionListItem } from '../types'
import { StatusBadge } from '@/shared/ui'
import { formatTimeLeft, capitalizeStatus, formatCurrency } from '../utils'
import { typography } from '@/shared/theme/tokens'

interface AuctionCardProps {
  auction: AuctionListItem
}

export const AuctionCard = memo(({ auction }: AuctionCardProps) => {
  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        border: '1px solid #E7E5E4',
        transition: 'border-color 0.3s ease',
        '&:hover': {
          borderColor: '#1C1917',
        },
        '&:hover .auction-card-image': {
          transform: 'scale(1.03)',
        },
      }}
    >
      <Box sx={{ position: 'relative', overflow: 'hidden' }}>
        <CardMedia
          className="auction-card-image"
          component="div"
          sx={{
            height: 240,
            bgcolor: '#F5F5F4',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundImage: auction.primaryImageUrl
              ? `url(${auction.primaryImageUrl})`
              : undefined,
            backgroundSize: 'cover',
            backgroundPosition: 'center',
            transition: 'transform 0.6s ease',
          }}
        >
          {!auction.primaryImageUrl && <Gavel sx={{ fontSize: 48, color: '#D6D3D1' }} />}
        </CardMedia>
        <StatusBadge
          status={capitalizeStatus(auction.status)}
          sx={{
            position: 'absolute',
            top: 12,
            right: 12,
          }}
        />
      </Box>
      <CardContent sx={{ flexGrow: 1, p: 2.5 }}>
        <Typography
          sx={{
            color: '#78716C',
            fontSize: '0.625rem',
            letterSpacing: '0.1em',
            textTransform: 'uppercase',
            fontWeight: typography.fontWeight.medium,
            mb: 0.5,
          }}
        >
          {auction.categoryName}
        </Typography>
        <Typography
          sx={{
            fontWeight: typography.fontWeight.medium,
            fontSize: '0.9rem',
            color: '#1C1917',
            lineHeight: 1.4,
            mb: 1.5,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {auction.title}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1.5 }}>
          <Timer sx={{ fontSize: 14, color: '#A8A29E' }} />
          <Typography sx={{ fontSize: '0.75rem', color: '#78716C' }}>
            {formatTimeLeft(auction.endTime)}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
          <Typography sx={{ fontWeight: typography.fontWeight.semibold, fontSize: '1rem', color: '#1C1917' }}>
            {formatCurrency(auction.currentBid)}
          </Typography>
          <Typography sx={{ fontSize: '0.75rem', color: '#A8A29E' }}>
            {auction.bidCount} bids
          </Typography>
        </Box>
      </CardContent>
      <Box sx={{ px: 2.5, pb: 2.5, pt: 0 }}>
        <Button
          variant="outlined"
          fullWidth
          component={Link}
          to={`/auctions/${auction.id}`}
          sx={{
            borderColor: '#1C1917',
            color: '#1C1917',
            fontSize: '0.75rem',
            fontWeight: typography.fontWeight.medium,
            letterSpacing: '0.1em',
            textTransform: 'uppercase',
            borderRadius: 0,
            py: 1,
            '&:hover': {
              bgcolor: '#1C1917',
              color: '#FFFFFF',
              borderColor: '#1C1917',
            },
          }}
        >
          View Details
        </Button>
      </Box>
    </Card>
  )
})
