import { Grid, Skeleton, Box } from '@mui/material'
import { AuctionCard } from './AuctionCard'
import type { AuctionListItem } from '../types'

interface AuctionGridProps {
  auctions: AuctionListItem[]
  isLoading?: boolean
  emptyMessage?: string
}

const SKELETON_ITEMS = Array.from({ length: 8 }, (_, index) => index)

export const AuctionGrid = ({
  auctions,
  isLoading,
  emptyMessage = 'No auctions found',
}: AuctionGridProps) => {
  if (isLoading) {
    return (
      <Grid container spacing={3}>
        {SKELETON_ITEMS.map((index) => (
          <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={index}>
            <Skeleton variant="rectangular" height={320} sx={{ borderRadius: 2 }} />
          </Grid>
        ))}
      </Grid>
    )
  }

  if (auctions.length === 0) {
    return <Box sx={{ textAlign: 'center', py: 8, color: 'text.secondary' }}>{emptyMessage}</Box>
  }

  return (
    <Grid container spacing={3}>
      {auctions.map((auction) => (
        <Grid size={{ xs: 12, sm: 6, md: 4, lg: 3 }} key={auction.id}>
          <AuctionCard auction={auction} />
        </Grid>
      ))}
    </Grid>
  )
}
