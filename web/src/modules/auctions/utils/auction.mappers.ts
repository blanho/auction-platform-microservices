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
 * Backend enum: Draft, Scheduled, Live, Finished, ReservedNotMet, Inactive, Cancelled, ReservedForBuyNow
 */
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
 * Maps backend OrderStatus enum to frontend OrderStatus
 * Backend enum: Pending, PaymentPending, Paid, Processing, Shipped, Delivered, Completed, Cancelled, Disputed, Refunded
 */
export function mapOrderStatus(status: string): string {
  const statusMap: Record<string, string> = {
    Pending: 'pending',
    PaymentPending: 'payment_pending',
    Paid: 'paid',
    Processing: 'processing',
    Shipped: 'shipped',
    Delivered: 'delivered',
    Completed: 'completed',
    Cancelled: 'cancelled',
    Disputed: 'disputed',
    Refunded: 'refunded',
  }
  return statusMap[status] || status.toLowerCase()
}

/**
 * Maps backend PaymentStatus enum to frontend PaymentStatus
 * Backend enum: Pending, Processing, Completed, Failed, Refunded, Cancelled
 */
export function mapPaymentStatus(status: string): string {
  const statusMap: Record<string, string> = {
    Pending: 'pending',
    Processing: 'processing',
    Completed: 'completed',
    Failed: 'failed',
    Refunded: 'refunded',
    Cancelled: 'cancelled',
  }
  return statusMap[status] || status.toLowerCase()
}

/**
 * Maps backend BidStatus enum to frontend BidStatus
 * Backend enum: Pending, Accepted, AcceptedBelowReserve, TooLow, Rejected
 */
export function mapBidStatus(status: string): string {
  const statusMap: Record<string, string> = {
    Pending: 'Pending',
    Accepted: 'Accepted',
    AcceptedBelowReserve: 'Accepted',
    TooLow: 'Rejected',
    Rejected: 'Rejected',
    Retracted: 'Retracted',
    Outbid: 'Outbid',
  }
  return statusMap[status] || status
}

/**
 * Maps backend NotificationType enum to frontend notification type string
 * Backend enum: General=0, AuctionCreated=10, AuctionUpdated=11, etc.
 */
export function mapNotificationType(type: string | number): string {
  const typeMap: Record<string | number, string> = {
    0: 'system',
    General: 'system',
    10: 'auction_created',
    AuctionCreated: 'auction_created',
    11: 'auction_updated',
    AuctionUpdated: 'auction_updated',
    12: 'auction_started',
    AuctionStarted: 'auction_started',
    13: 'auction_ending',
    AuctionEndingSoon: 'auction_ending',
    14: 'auction_ended',
    AuctionFinished: 'auction_ended',
    15: 'auction_cancelled',
    AuctionCancelled: 'auction_cancelled',
    20: 'bid_placed',
    BidPlaced: 'bid_placed',
    21: 'bid_outbid',
    BidOutbid: 'bid_outbid',
    22: 'auction_won',
    BidWon: 'auction_won',
    23: 'auction_lost',
    BidLost: 'auction_lost',
    24: 'bid_accepted',
    BidAccepted: 'bid_accepted',
    25: 'bid_rejected',
    BidRejected: 'bid_rejected',
    30: 'payment_received',
    PaymentReceived: 'payment_received',
    31: 'payment_failed',
    PaymentFailed: 'payment_failed',
    32: 'payment_refunded',
    PaymentRefunded: 'payment_refunded',
    40: 'welcome',
    WelcomeMessage: 'welcome',
    41: 'account_verified',
    AccountVerified: 'account_verified',
    42: 'password_changed',
    PasswordChanged: 'password_changed',
    50: 'system',
    SystemAlert: 'system',
    51: 'maintenance',
    Maintenance: 'maintenance',
  }
  return typeMap[type] || 'system'
}

/**
 * Maps backend NotificationStatus enum to frontend status
 * Backend enum: Pending=0, Unread=1, Read=2, Dismissed=3, Archived=4
 */
export function mapNotificationStatus(status: string | number): string {
  const statusMap: Record<string | number, string> = {
    0: 'unread',
    Pending: 'unread',
    1: 'unread',
    Unread: 'unread',
    2: 'read',
    Read: 'read',
    3: 'read',
    Dismissed: 'read',
    4: 'archived',
    Archived: 'archived',
  }
  return statusMap[status] || 'unread'
}

/**
 * Maps array of auction list DTOs
 */
export function mapAuctionListDtos(dtos: BackendAuctionListDto[]): AuctionListItem[] {
  return dtos.map(mapAuctionListDto)
}
