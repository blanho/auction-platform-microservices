import type { Notification, NotificationType } from '../types'
import { NOTIFICATION_COLORS, NOTIFICATION_LABELS } from '../constants'

export function formatTimeAgo(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now.getTime() - date.getTime()

  const minutes = Math.floor(diff / 60000)
  const hours = Math.floor(diff / 3600000)
  const days = Math.floor(diff / 86400000)

  if (minutes < 1) return 'Just now'
  if (minutes < 60) return `${minutes}m ago`
  if (hours < 24) return `${hours}h ago`
  if (days < 7) return `${days}d ago`
  return date.toLocaleDateString()
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

export function isUnread(notification: Notification): boolean {
  return notification.status === 'unread'
}

export function groupNotificationsByDate(notifications: Notification[]): Map<string, Notification[]> {
  const groups = new Map<string, Notification[]>()
  const today = new Date()
  const yesterday = new Date(today)
  yesterday.setDate(yesterday.getDate() - 1)

  notifications.forEach((notification) => {
    const date = new Date(notification.createdAt)
    let groupKey: string

    if (date.toDateString() === today.toDateString()) {
      groupKey = 'Today'
    } else if (date.toDateString() === yesterday.toDateString()) {
      groupKey = 'Yesterday'
    } else if (date > new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000)) {
      groupKey = 'This Week'
    } else {
      groupKey = 'Older'
    }

    if (!groups.has(groupKey)) {
      groups.set(groupKey, [])
    }
    groups.get(groupKey)!.push(notification)
  })

  return groups
}

export function filterNotifications(
  notifications: Notification[],
  filters: {
    type?: NotificationType
    unreadOnly?: boolean
    search?: string
  }
): Notification[] {
  return notifications.filter((notification) => {
    if (filters.type && notification.type !== filters.type) return false
    if (filters.unreadOnly && !isUnread(notification)) return false
    if (filters.search) {
      const searchLower = filters.search.toLowerCase()
      return (
        notification.title.toLowerCase().includes(searchLower) ||
        notification.message.toLowerCase().includes(searchLower)
      )
    }
    return true
  })
}

export function sortNotifications(
  notifications: Notification[],
  sortBy: 'date' | 'type' = 'date'
): Notification[] {
  return [...notifications].sort((a, b) => {
    if (sortBy === 'date') {
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    }
    return a.type.localeCompare(b.type)
  })
}

export function getUnreadCount(notifications: Notification[]): number {
  return notifications.filter(isUnread).length
}

export function hasUnreadNotifications(notifications: Notification[]): boolean {
  return getUnreadCount(notifications) > 0
}
