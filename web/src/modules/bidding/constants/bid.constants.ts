import type { BidHistoryFilters, WinningBidsFilters } from '../types'

export const BID_CONSTANTS = {
  MIN_BID_AMOUNT: 1,
  DEFAULT_PAGE_SIZE: 20,
  PAGE_SIZE: 12,
  MAX_PAGE_SIZE: 100,
  BID_DEBOUNCE_MS: 500,
  AUTO_REFRESH_INTERVAL_MS: 30000,
  QUERY_KEYS: {
    bids: ['bids'] as const,
    bidById: (id: string) => ['bids', id] as const,
    bidsForAuction: (auctionId: string) => ['bids', 'auction', auctionId] as const,
    myBids: ['bids', 'my'] as const,
    winningBids: (filters: WinningBidsFilters) => ['bids', 'winning', filters] as const,
    bidHistory: (filters: BidHistoryFilters) => ['bids', 'history', filters] as const,
    bidIncrement: (currentBid: number) => ['bids', 'increment', currentBid] as const,
    autoBids: ['autoBids'] as const,
    autoBidById: (id: string) => ['autoBids', id] as const,
    myAutoBids: (activeOnly?: boolean, page?: number, pageSize?: number) =>
      ['autoBids', 'my', { activeOnly, page, pageSize }] as const,
  },
} as const

export const BID_INCREMENT_RANGES = [
  { max: 100, increment: 5 },
  { max: 500, increment: 10 },
  { max: 1000, increment: 25 },
  { max: 5000, increment: 50 },
  { max: 10000, increment: 100 },
  { max: Infinity, increment: 250 },
] as const

export const BID_STATUS_LABELS = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Rejected: 'Rejected',
  Retracted: 'Retracted',
  Outbid: 'Outbid',
} as const

export const BID_STATUS_COLORS = {
  Pending: { bg: 'rgba(249, 115, 22, 0.1)', color: '#EA580C' },
  Accepted: { bg: 'rgba(34, 197, 94, 0.1)', color: '#16A34A' },
  Rejected: { bg: 'rgba(239, 68, 68, 0.1)', color: '#DC2626' },
  Retracted: { bg: 'rgba(107, 114, 128, 0.1)', color: '#6B7280' },
  Outbid: { bg: 'rgba(59, 130, 246, 0.1)', color: '#3B82F6' },
} as const

