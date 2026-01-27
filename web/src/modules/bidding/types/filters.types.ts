import type { BidStatus } from './bid.types'

export interface BidFilters {
  status?: BidStatus
  page?: number
  pageSize?: number
}

export interface WinningBidsFilters {
  page?: number
  pageSize?: number
}

export interface BidHistoryFilters {
  auctionId?: string
  status?: BidStatus
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export interface AutoBidFilters {
  activeOnly?: boolean
  page?: number
  pageSize?: number
}
