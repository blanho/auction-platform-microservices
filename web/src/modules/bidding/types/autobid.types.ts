export interface AutoBid {
  id: string
  auctionId: string
  userId: string
  username: string
  maxAmount: number
  currentBidAmount: number
  isActive: boolean
  createdAt: string
  lastBidAt?: string
}

export interface AutoBidDetail extends AutoBid {
  auctionTitle: string
  auctionStatus: string
  auctionEndTime: string
  currentHighestBid: number
  bidIncrement?: number
  remainingAmount: number
  totalBidsPlaced: number
}

export interface CreateAutoBidRequest {
  auctionId: string
  maxAmount: number
  bidIncrement?: number
}

export interface UpdateAutoBidRequest {
  maxAmount?: number
  bidIncrement?: number
  isActive?: boolean
}

export interface ToggleAutoBidRequest {
  activate: boolean
}

export interface AutoBidsResult {
  autoBids: AutoBid[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface CreateAutoBidResult {
  autoBidId: string
  message: string
}

export interface UpdateAutoBidResult {
  success: boolean
  message: string
}

export interface ToggleAutoBidResult {
  isActive: boolean
  message: string
}

export interface CancelAutoBidResult {
  success: boolean
  message: string
}
