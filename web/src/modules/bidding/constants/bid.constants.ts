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
