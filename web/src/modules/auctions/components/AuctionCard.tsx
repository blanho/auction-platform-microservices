import { memo } from 'react'
import { Box, Card, CardContent, CardMedia, Typography, Button } from '@mui/material'
import { Link } from 'react-router-dom'
import { Timer, Gavel } from '@mui/icons-material'
import type { AuctionListItem } from '../types'
import { StatusBadge } from '@/shared/ui'
import { formatTimeLeft, capitalizeStatus, formatCurrency } from '../utils'

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
        transition: 'transform 0.2s, box-shadow 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <Box sx={{ position: 'relative' }}>
        <CardMedia
          component="div"
          sx={{
            height: 180,
            bgcolor: 'grey.200',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            backgroundImage: auction.primaryImageUrl
              ? `url(${auction.primaryImageUrl})`
              : undefined,
            backgroundSize: 'cover',
            backgroundPosition: 'center',
          }}
        >
          {!auction.primaryImageUrl && <Gavel sx={{ fontSize: 64, color: 'grey.400' }} />}
        </CardMedia>
        <StatusBadge
          status={capitalizeStatus(auction.status)}
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
          }}
        />
      </Box>
      <CardContent sx={{ flexGrow: 1 }}>
        <Typography variant="subtitle1" fontWeight={600} noWrap gutterBottom>
          {auction.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" gutterBottom>
          {auction.categoryName}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
          <Timer fontSize="small" color="action" />
          <Typography variant="body2" color="text.secondary">
            {formatTimeLeft(auction.endTime)}
          </Typography>
        </Box>
        <Typography variant="h6" fontWeight={700} color="primary">
          {formatCurrency(auction.currentBid)}
        </Typography>
        <Typography variant="caption" color="text.secondary">
          {auction.bidCount} bids
        </Typography>
      </CardContent>
      <Box sx={{ p: 2, pt: 0 }}>
        <Button variant="contained" fullWidth component={Link} to={`/auctions/${auction.id}`}>
          View Details
        </Button>
      </Box>
    </Card>
  )
})
