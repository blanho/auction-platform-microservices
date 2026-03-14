import type { AuctionStatus } from './auction.types'
import type { QueryParameters } from '@/shared/types'

export interface AuctionFilter {
  search?: string
  searchTerm?: string
  categoryId?: string
  category?: string
  status?: AuctionStatus
  minPrice?: number
  maxPrice?: number
  sellerId?: string
  seller?: string
  winner?: string
  isFeatured?: boolean
}

export interface AuctionFilters extends QueryParameters {
  search?: string
  searchTerm?: string
  categoryId?: string
  category?: string
  status?: AuctionStatus
  minPrice?: number
  maxPrice?: number
  sellerId?: string
  seller?: string
  winner?: string
  isFeatured?: boolean
}

export interface CreateAuctionFileInput {
  fileId: string
  fileType?: string
  displayOrder?: number
  isPrimary?: boolean
}

export interface CreateAuctionRequest {
  title: string
  description: string
  condition?: string
  yearManufactured?: number
  attributes?: Record<string, string>
  files?: CreateAuctionFileInput[]
  reservePrice: number
  buyNowPrice?: number
  auctionEnd: string
  categoryId?: string
  brandId?: string
  isFeatured?: boolean
  currency?: string
}

export interface UpdateAuctionRequest {
  title?: string
  description?: string
  condition?: string
  yearManufactured?: number
  attributes?: Record<string, string>
}
