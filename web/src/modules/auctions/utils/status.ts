import type { AuctionStatus } from '../types'
import { AUCTION_STATUS_COLORS, AUCTION_STATUS_LABELS } from '../constants'

export function getStatusColor(
  status: AuctionStatus
): 'success' | 'warning' | 'info' | 'error' | 'default' {
  return AUCTION_STATUS_COLORS[status] || 'default'
}

export function getStatusLabel(status: AuctionStatus): string {
  return AUCTION_STATUS_LABELS[status] || status
}

export function capitalizeStatus(status: string): string {
  return status.replace('-', ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}
