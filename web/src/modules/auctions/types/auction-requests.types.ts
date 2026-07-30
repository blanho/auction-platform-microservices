import type { AuctionStatus } from './auction.types'
import type { QueryParameters } from '@/shared/types'

export interface AuctionFilters extends QueryParameters {
  searchTerm?: string
  category?: string
  status?: AuctionStatus
  seller?: string
  winner?: string
  isFeatured?: boolean
  orderBy?: 'price' | 'enddate' | 'createdat' | 'title'
  descending?: boolean
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
