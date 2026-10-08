import { BACKEND_AUCTION_STATUS } from '../constants/backend-auction-status'
import { AUCTION_STATUS } from '@/modules/auctions/constants/auction-status'
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
    [BACKEND_AUCTION_STATUS.DRAFT]: AUCTION_STATUS.DRAFT,
    [BACKEND_AUCTION_STATUS.SCHEDULED]: AUCTION_STATUS.PENDING,
    [BACKEND_AUCTION_STATUS.PENDING]: AUCTION_STATUS.PENDING,
    [BACKEND_AUCTION_STATUS.LIVE]: AUCTION_STATUS.ACTIVE,
    [BACKEND_AUCTION_STATUS.ACTIVE]: AUCTION_STATUS.ACTIVE,
    [BACKEND_AUCTION_STATUS.FINISHED]: AUCTION_STATUS.ENDED,
    [BACKEND_AUCTION_STATUS.ENDED]: AUCTION_STATUS.ENDED,
    [BACKEND_AUCTION_STATUS.RESERVED_NOT_MET]: AUCTION_STATUS.ENDED,
    [BACKEND_AUCTION_STATUS.INACTIVE]: AUCTION_STATUS.CANCELLED,
    [BACKEND_AUCTION_STATUS.CANCELLED]: AUCTION_STATUS.CANCELLED,
    [BACKEND_AUCTION_STATUS.RESERVED_FOR_BUY_NOW]: AUCTION_STATUS.SOLD,
    [BACKEND_AUCTION_STATUS.SOLD]: AUCTION_STATUS.SOLD,
  }
  return statusMap[status] || AUCTION_STATUS.DRAFT
}

function isEndingSoon(endTime: string, status: string): boolean {
  if (status !== BACKEND_AUCTION_STATUS.ACTIVE) {
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
    status: isEndingSoon(dto.auctionEnd, dto.status) ? AUCTION_STATUS.ENDING_SOON : baseStatus,
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
    status: isEndingSoon(dto.auctionEnd, dto.status) ? AUCTION_STATUS.ENDING_SOON : baseStatus,
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
