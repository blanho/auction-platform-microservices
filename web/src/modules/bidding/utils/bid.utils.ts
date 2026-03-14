import type { BidStatus } from '../types'
import { BID_INCREMENT_RANGES, BID_STATUS_COLORS } from '../constants'

export const calculateBidIncrement = (currentBid: number): number => {
  const range = BID_INCREMENT_RANGES.find((r) => currentBid <= r.max)
  return range?.increment ?? 250
}

export const calculateMinimumNextBid = (currentBid: number, increment?: number): number => {
  const bidIncrement = increment ?? calculateBidIncrement(currentBid)
  return currentBid + bidIncrement
}

export const validateBidAmount = (
  amount: number,
  currentBid: number,
  minimumBid?: number
): { valid: boolean; error?: string } => {
  if (amount <= 0) {
    return { valid: false, error: 'Bid amount must be greater than 0' }
  }

  const minRequired = minimumBid ?? calculateMinimumNextBid(currentBid)
  if (amount < minRequired) {
    return {
      valid: false,
      error: `Bid must be at least $${minRequired.toFixed(2)}`,
    }
  }

  return { valid: true }
}

export const getBidStatusColor = (status: BidStatus) => {
  return BID_STATUS_COLORS[status] ?? { bg: 'rgba(148, 163, 184, 0.1)', color: '#94A3B8' }
}

export const isBidActive = (bidTime: string, auctionEndTime: string): boolean => {
  const bidDate = new Date(bidTime)
  const endDate = new Date(auctionEndTime)
  return bidDate < endDate
}

export const getBidTimeRemaining = (auctionEndTime: string): number => {
  const now = new Date()
  const endDate = new Date(auctionEndTime)
  return Math.max(0, endDate.getTime() - now.getTime())
}

export const formatBidTimeRemaining = (milliseconds: number): string => {
  const seconds = Math.floor(milliseconds / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)
  const days = Math.floor(hours / 24)

  if (days > 0) {return `${days}d ${hours % 24}h`}
  if (hours > 0) {return `${hours}h ${minutes % 60}m`}
  if (minutes > 0) {return `${minutes}m ${seconds % 60}s`}
  return `${seconds}s`
}

export const isWinningBid = (bidAmount: number, currentHighestBid: number): boolean => {
  return bidAmount >= currentHighestBid
}

export const calculateAutoBidRemaining = (maxAmount: number, currentBid: number): number => {
  return Math.max(0, maxAmount - currentBid)
}

export const canPlaceAutoBid = (
  maxAmount: number,
  currentBid: number,
  minimumIncrement: number
): boolean => {
  return maxAmount >= currentBid + minimumIncrement
}
