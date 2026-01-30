import type { QueryParameters } from '@/shared/types'

export interface Notification {
  id: string
  userId: string
  type: NotificationType
  title: string
  message: string
  data?: string
  status: NotificationStatus
  readAt?: string
  auctionId?: string
  bidId?: string
  createdAt: string
}

export type NotificationType =
  | 'bid_placed'
  | 'bid_outbid'
  | 'auction_won'
  | 'auction_lost'
  | 'auction_ending'
  | 'auction_ended'
  | 'payment_received'
  | 'payment_failed'
  | 'system'
  | 'promotional'

export type NotificationStatus = 'unread' | 'read' | 'archived'

export interface NotificationSummary {
  unreadCount: number
  totalCount: number
  recentNotifications: Notification[]
}

export interface NotificationFilters extends QueryParameters {
  type?: NotificationType
  status?: NotificationStatus
}

export interface NotificationPreferences {
  id: string
  userId: string
  emailEnabled: boolean
  pushEnabled: boolean
  bidUpdates: boolean
  auctionUpdates: boolean
  promotionalEmails: boolean
  systemAlerts: boolean
}

export interface CreateNotificationDto {
  userId: string
  type: NotificationType
  title: string
  message: string
  data?: string
  auctionId?: string
  bidId?: string
}

export interface BroadcastNotificationDto {
  type: NotificationType
  title: string
  message: string
  targetRole?: string
}

export interface AdminNotificationFilters extends QueryParameters {
  userId?: string
  type?: NotificationType
  status?: NotificationStatus
}

export interface NotificationStatsDto {
  totalNotifications: number
  unreadNotifications: number
  todayCount: number
  byType: Record<string, number>
}
