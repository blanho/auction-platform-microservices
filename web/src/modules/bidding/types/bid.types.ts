export const BidStatus = {
  Pending: 'Pending',
  Accepted: 'Accepted',
  Rejected: 'Rejected',
  Retracted: 'Retracted',
  Outbid: 'Outbid',
} as const

export type BidStatus = typeof BidStatus[keyof typeof BidStatus]

export interface Bid {
  id: string
  auctionId: string
  bidderId: string
  bidderUsername: string
  amount: number
  bidTime: string
  status: BidStatus
  errorMessage?: string
  minimumNextBid?: number
  minimumIncrement?: number
  createdAt: string
  updatedAt: string
}

export interface BidDetail extends Bid {
  auctionTitle: string
  auctionStatus: string
  currentHighestBid: number
  isWinning: boolean
}

export interface WinningBid {
  id: string
  auctionId: string
  auctionTitle: string
  auctionEndTime: string
  currentBid: number
  minimumNextBid: number
  bidCount: number
  isActive: boolean
}

export interface BidHistory {
  id: string
  auctionId: string
  auctionTitle: string
  amount: number
  bidTime: string
  status: BidStatus
  isWinning: boolean
}

export interface PlaceBidRequest {
  auctionId: string
  amount: number
}

export interface RetractBidRequest {
  reason: string
}

export interface RetractBidResult {
  success: boolean
  message: string
  refundAmount?: number
}

export interface BidIncrementInfo {
  currentBid: number
  minimumIncrement: number
  minimumNextBid: number
}
