import { formatRelativeTime } from '@/shared/utils/formatters'
import { NOTIFICATION_COLORS, NOTIFICATION_LABELS } from '../constants'
import type { Notification, NotificationType } from '../types'

export function formatTimeAgo(dateString: string): string {
  return formatRelativeTime(dateString)
}

export function getNotificationColor(type: NotificationType): string {
  return NOTIFICATION_COLORS[type] || NOTIFICATION_COLORS.system
}

export function getNotificationLabel(type: NotificationType): string {
  return NOTIFICATION_LABELS[type] || 'Notification'
}

export function getNotificationLink(notification: Notification): string | undefined {
  if (notification.auctionId) {
    return `/auctions/${notification.auctionId}`
  }
  if (notification.bidId) {
    return `/bids/${notification.bidId}`
  }
  return undefined
}
