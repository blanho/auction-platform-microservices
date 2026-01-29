import type { AuctionStatus } from '../types'

export const AUCTION_STATUS = {
  DRAFT: 'draft',
  PENDING: 'pending',
  ACTIVE: 'active',
  ENDING_SOON: 'ending-soon',
  ENDED: 'ended',
  SOLD: 'sold',
  CANCELLED: 'cancelled',
} as const

export const AUCTION_STATUS_LABELS: Record<AuctionStatus, string> = {
  draft: 'Draft',
  pending: 'Pending',
  active: 'Active',
  'ending-soon': 'Ending Soon',
  ended: 'Ended',
  sold: 'Sold',
  cancelled: 'Cancelled',
}

export const AUCTION_STATUS_COLORS: Record<
  AuctionStatus,
  'success' | 'warning' | 'info' | 'error' | 'default'
> = {
  active: 'success',
  'ending-soon': 'warning',
  pending: 'info',
  draft: 'default',
  ended: 'default',
  sold: 'success',
  cancelled: 'error',
}
