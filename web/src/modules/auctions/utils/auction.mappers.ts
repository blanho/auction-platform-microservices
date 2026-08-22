import type {
  AuctionDetails,
  AuctionImage,
  AuctionListItem,
  AuctionStatus,
} from '../types/auction.types'
import type { BackendAuctionDto, BackendAuctionFileDto } from '../types/backend-dto.types'

const STORAGE_BASE_URL = import.meta.env.VITE_STORAGE_URL || '/api/files'

function mapAuctionStatus(status: string): AuctionStatus {
  const statusMap: Record<string, AuctionStatus> = {
    Draft: 'draft',
    Scheduled: 'pending',
    Pending: 'pending',
    Live: 'active',
    Active: 'active',
    Finished: 'ended',
    Ended: 'ended',
    ReservedNotMet: 'ended',
    Inactive: 'cancelled',
    Cancelled: 'cancelled',
    ReservedForBuyNow: 'sold',
    Sold: 'sold',
  }
  return statusMap[status] || 'draft'
}

function isEndingSoon(endTime: string, status: string): boolean {
  if (status !== 'Active') {
    return false
  }
  const end = new Date(endTime)
  const now = new Date()
  const hoursRemaining = (end.getTime() - now.getTime()) / (1000 * 60 * 60)
  return hoursRemaining > 0 && hoursRemaining <= 1
}

function mapAuctionFile(file: BackendAuctionFileDto): AuctionImage {
  return {
    id: file.fileId,
    url: `${STORAGE_BASE_URL}/${file.fileId}`,
    alt: '',
    isPrimary: file.isPrimary,
    order: file.displayOrder,
  }
}

export function mapAuctionDto(dto: BackendAuctionDto): AuctionDetails {
  const baseStatus = mapAuctionStatus(dto.status)

  return {
    id: dto.id,
    title: dto.title,
    description: dto.description,
    condition: dto.condition,
    yearManufactured: dto.yearManufactured,
    startingPrice: dto.reservePrice,
    currentBid: dto.currentHighBid ?? dto.reservePrice,
    reservePrice: dto.reservePrice,
    buyNowPrice: dto.buyNowPrice,
    status: isEndingSoon(dto.auctionEnd, dto.status) ? 'ending-soon' : baseStatus,
    startTime: dto.createdAt,
    endTime: dto.auctionEnd,
    sellerId: dto.sellerId,
    sellerName: dto.seller,
    categoryId: dto.categoryId ?? '',
    categoryName: dto.categoryName ?? '',
    images: dto.files.map(mapAuctionFile),
    bidCount: 0,
    watcherCount: 0,
    createdAt: dto.createdAt,
    updatedAt: dto.updatedAt,
    seller: {
      id: dto.sellerId,
      username: dto.seller,
      displayName: dto.seller,
    },
    category: {
      id: dto.categoryId ?? '',
      name: dto.categoryName ?? '',
    },
    bids: [],
    isWatching: false,
  }
}

export function mapAuctionListDto(dto: BackendAuctionDto): AuctionListItem {
  const baseStatus = mapAuctionStatus(dto.status)
  const primaryFile = dto.files.find((file) => file.isPrimary) ?? dto.files[0]

  return {
    id: dto.id,
    title: dto.title,
    currentBid: dto.currentHighBid ?? dto.reservePrice,
    startingPrice: dto.reservePrice,
    status: isEndingSoon(dto.auctionEnd, dto.status) ? 'ending-soon' : baseStatus,
    endTime: dto.auctionEnd,
    bidCount: 0,
    categoryName: dto.categoryName ?? '',
    sellerName: dto.seller ?? '',
    primaryImageUrl: primaryFile ? `${STORAGE_BASE_URL}/${primaryFile.fileId}` : undefined,
  }
}

export function mapAuctionListDtos(dtos: BackendAuctionDto[]): AuctionListItem[] {
  return dtos.map(mapAuctionListDto)
}
