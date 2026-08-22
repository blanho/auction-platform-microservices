import type { QueryParameters } from '@/shared/types'
import type { BidStatus } from './bid.types'

export type WinningBidsFilters = QueryParameters

export interface BidHistoryFilters extends QueryParameters {
  auctionId?: string
  status?: BidStatus
  fromDate?: string
  toDate?: string
}
