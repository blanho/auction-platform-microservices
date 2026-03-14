/**
 * Backend DTO types - exact match to C# DTOs
 * These should be used for API responses before mapping to UI types
 */

export interface BackendAuctionDto {
  id: string
  reservePrice: number
  buyNowPrice?: number
  isBuyNowAvailable: boolean
  currency: string
  sellerId: string
  seller: string
  winnerId?: string
  winner?: string
  soldAmount?: number
  currentHighBid?: number
  createdAt: string
  updatedAt: string
  auctionEnd: string
  status: string
  title: string
  description: string
  condition?: string
  yearManufactured?: number
  attributes?: Record<string, string>
  categoryId?: string
  categoryName?: string
  categorySlug?: string
  categoryIcon?: string
  isFeatured: boolean
  files: BackendAuctionFileDto[]
}

export interface BackendAuctionFileDto {
  fileId: string
  fileType: string
  displayOrder: number
  isPrimary: boolean
}

export interface BackendAuctionListDto {
  id: string
  title: string
  currentHighBid?: number
  reservePrice: number
  status: string
  auctionEnd: string
  bidCount?: number
  categoryName?: string
  seller?: string
  primaryImageUrl?: string
}

export interface BackendBidDto {
  id: string
  auctionId: string
  bidderId: string
  bidderUsername: string
  amount: number
  bidTime: string
  status: string
  errorMessage?: string
  minimumNextBid: number
  minimumIncrement: number
  createdAt: string
  updatedAt?: string
}
