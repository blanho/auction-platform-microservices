export const NOTIFICATION_TYPES = {
  BID_PLACED: 'bid_placed',
  BID_OUTBID: 'bid_outbid',
  AUCTION_WON: 'auction_won',
  AUCTION_LOST: 'auction_lost',
  AUCTION_ENDING: 'auction_ending',
  AUCTION_ENDED: 'auction_ended',
  PAYMENT_RECEIVED: 'payment_received',
  PAYMENT_FAILED: 'payment_failed',
  SYSTEM: 'system',
  PROMOTIONAL: 'promotional',
} as const

export const NOTIFICATION_STATUS = {
  UNREAD: 'unread',
  READ: 'read',
  ARCHIVED: 'archived',
} as const

export const NOTIFICATION_COLORS = {
  bid_placed: '#3B82F6',
  bid_outbid: '#F59E0B',
  auction_won: '#22C55E',
  auction_lost: '#EF4444',
  auction_ending: '#F59E0B',
  auction_ended: '#78716C',
  payment_received: '#22C55E',
  payment_failed: '#EF4444',
  promotional: '#CA8A04',
  system: '#78716C',
} as const

export const NOTIFICATION_ROUTES = {
  LIST: '/notifications',
  SETTINGS: '/settings',
} as const

export const NOTIFICATION_CONFIG = {
  DEFAULT_PAGE_SIZE: 20,
  REFRESH_INTERVAL_MS: 60 * 1000,
  MAX_SUMMARY_ITEMS: 5,
} as const

export const NOTIFICATION_LABELS = {
  bid_placed: 'Bid Placed',
  bid_outbid: 'Outbid',
  auction_won: 'Auction Won',
  auction_lost: 'Auction Lost',
  auction_ending: 'Ending Soon',
  auction_ended: 'Auction Ended',
  payment_received: 'Payment Received',
  payment_failed: 'Payment Failed',
  promotional: 'Promotional',
  system: 'System',
} as const
