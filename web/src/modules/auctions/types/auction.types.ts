export interface Auction {
  id: string
  title: string
  description: string
  startingPrice: number
  currentBid: number
  reservePrice?: number
  buyNowPrice?: number
  status: AuctionStatus
  startTime: string
  endTime: string
  sellerId: string
  sellerName: string
  categoryId: string
  categoryName: string
  images: AuctionImage[]
  bidCount: number
  watcherCount: number
  createdAt: string
  updatedAt: string
}

export type AuctionStatus = 'draft' | 'pending' | 'active' | 'ending-soon' | 'ended' | 'sold' | 'cancelled'

export interface AuctionImage {
  id: string
  url: string
  alt: string
  isPrimary: boolean
  order: number
}

export interface AuctionListItem {
  id: string
  title: string
  currentBid: number
  startingPrice: number
  status: AuctionStatus
  endTime: string
  bidCount: number
  categoryName: string
  sellerName: string
  primaryImageUrl?: string
}

export interface AuctionDetails extends Auction {
  seller: AuctionSellerInfo
  category: CategoryInfo
  bids: BidSummary[]
  isWatching: boolean
  userBid?: UserBidInfo
}

export interface AuctionSellerInfo {
  id: string
  username: string
  displayName: string
  avatarUrl?: string
  rating: number
  totalSales: number
  reviewCount?: number
}

export interface CategoryInfo {
  id: string
  name: string
  parentId?: string
  parentName?: string
}

export interface BidSummary {
  id: string
  amount: number
  bidderName: string
  createdAt: string
}

export interface UserBidInfo {
  amount: number
  isWinning: boolean
}
