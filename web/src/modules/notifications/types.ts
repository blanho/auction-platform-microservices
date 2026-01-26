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

export interface NotificationFilters {
  type?: NotificationType
  status?: NotificationStatus
  page?: number
  pageSize?: number
}

export interface NotificationPreferences {
  emailEnabled: boolean
  pushEnabled: boolean
  bidUpdates: boolean
  auctionUpdates: boolean
  promotionalEmails: boolean
  systemAlerts: boolean
}
