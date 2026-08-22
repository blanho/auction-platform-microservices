import { palette } from '@/shared/theme/tokens'

export const NOTIFICATION_COLORS = {
  bid_placed: palette.semantic.info,
  bid_outbid: palette.semantic.warning,
  auction_won: palette.semantic.success,
  auction_lost: palette.semantic.error,
  auction_ending: palette.semantic.warning,
  auction_ended: palette.neutral[500],
  payment_received: palette.semantic.success,
  payment_failed: palette.semantic.error,
  promotional: palette.brand.primary,
  system: palette.neutral[500],
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
