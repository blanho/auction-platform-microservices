/**
 * DTO Mappers - Transform backend DTOs to frontend UI types
 * This layer prevents contract drift and provides a single place for field mapping
 */

import type {
  BackendAuctionDto,
  BackendAuctionFileDto,
  BackendAuctionListDto,
} from '../types/backend-dto.types'
import type { Auction, AuctionImage, AuctionListItem, AuctionStatus } from '../types/auction.types'

const STORAGE_BASE_URL = import.meta.env.VITE_STORAGE_URL || '/api/storage'

/**
 * Maps backend status string to frontend AuctionStatus
 */
function mapAuctionStatus(status: string): AuctionStatus {
  const statusMap: Record<string, AuctionStatus> = {
    Draft: 'draft',
    Pending: 'pending',
    Active: 'active',
    Ended: 'ended',
    Sold: 'sold',
    Cancelled: 'cancelled',
  }
  return statusMap[status] || 'draft'
}

/**
 * Determines if auction is ending soon (within 1 hour)
 */
function isEndingSoon(endTime: string, status: string): boolean {
  if (status !== 'Active') {return false}
  const end = new Date(endTime)
  const now = new Date()
  const hoursRemaining = (end.getTime() - now.getTime()) / (1000 * 60 * 60)
  return hoursRemaining > 0 && hoursRemaining <= 1
}

/**
 * Maps backend file DTO to frontend AuctionImage
 */
function mapAuctionFile(file: BackendAuctionFileDto): AuctionImage {
  return {
    id: file.fileId,
    url: `${STORAGE_BASE_URL}/files/${file.fileId}`,
    alt: '',
    isPrimary: file.isPrimary,
    order: file.displayOrder,
  }
}

/**
 * Maps full BackendAuctionDto to frontend Auction type
 */
export function mapAuctionDto(dto: BackendAuctionDto): Auction {
  const baseStatus = mapAuctionStatus(dto.status)

  return {
    id: dto.id,
    title: dto.title,
    description: dto.description,
    startingPrice: dto.reservePrice,
    currentBid: dto.currentHighBid ?? dto.reservePrice,
    reservePrice: dto.reservePrice,
    buyNowPrice: dto.buyNowPrice,
    status: isEndingSoon(dto.auctionEnd, dto.status) ? 'ending-soon' : baseStatus,
    startTime: dto.createdAt, // Using createdAt as startTime
    endTime: dto.auctionEnd,
    sellerId: dto.sellerId,
    sellerName: dto.seller,
    categoryId: dto.categoryId ?? '',
    categoryName: dto.categoryName ?? '',
    images: dto.files.map(mapAuctionFile),
    bidCount: 0, // Not in backend DTO
    watcherCount: 0, // Not in backend DTO
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
  }
}

/**
 * Maps BackendAuctionListDto to frontend AuctionListItem
 */
export function mapAuctionListDto(dto: BackendAuctionListDto): AuctionListItem {
  const baseStatus = mapAuctionStatus(dto.status)

  return {
    id: dto.id,
    title: dto.title,
    currentBid: dto.currentHighBid ?? dto.reservePrice,
    startingPrice: dto.reservePrice,
    status: isEndingSoon(dto.auctionEnd, dto.status) ? 'ending-soon' : baseStatus,
    endTime: dto.auctionEnd,
    bidCount: dto.bidCount ?? 0,
    categoryName: dto.categoryName ?? '',
    sellerName: dto.seller ?? '',
    primaryImageUrl: dto.primaryImageUrl,
  }
}

/**
 * Maps array of auction list DTOs
 */
export function mapAuctionListDtos(dtos: BackendAuctionListDto[]): AuctionListItem[] {
  return dtos.map(mapAuctionListDto)
}
