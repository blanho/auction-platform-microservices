import type { BidStatus } from './bid.types'
import type { QueryParameters } from '@/shared/types'

export interface BidFilters extends QueryParameters {
  status?: BidStatus
}

export interface WinningBidsFilters extends QueryParameters {}

export interface BidHistoryFilters extends QueryParameters {
  auctionId?: string
  status?: BidStatus
  fromDate?: string
  toDate?: string
}

export interface AutoBidFilters extends QueryParameters {
  activeOnly?: boolean
}
