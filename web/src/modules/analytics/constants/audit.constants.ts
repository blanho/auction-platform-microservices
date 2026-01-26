import type { AuditAction } from '../types'

export const AUDIT_ACTION = {
  CREATED: 'Created',
  UPDATED: 'Updated',
  DELETED: 'Deleted',
  STATUS_CHANGED: 'StatusChanged',
  PRICE_CHANGED: 'PriceChanged',
  BID_PLACED: 'BidPlaced',
  AUCTION_ENDED: 'AuctionEnded',
  PAYMENT_PROCESSED: 'PaymentProcessed',
  USER_REGISTERED: 'UserRegistered',
  USER_LOGGED_IN: 'UserLoggedIn',
  USER_LOGGED_OUT: 'UserLoggedOut',
  PERMISSION_CHANGED: 'PermissionChanged',
  SETTING_CHANGED: 'SettingChanged',
} as const

export const AUDIT_ACTION_LABELS: Record<AuditAction, string> = {
  Created: 'Created',
  Updated: 'Updated',
  Deleted: 'Deleted',
  StatusChanged: 'Status Changed',
  PriceChanged: 'Price Changed',
  BidPlaced: 'Bid Placed',
  AuctionEnded: 'Auction Ended',
  PaymentProcessed: 'Payment Processed',
  UserRegistered: 'User Registered',
  UserLoggedIn: 'User Logged In',
  UserLoggedOut: 'User Logged Out',
  PermissionChanged: 'Permission Changed',
  SettingChanged: 'Setting Changed',
}

export const AUDIT_ACTION_COLORS: Record<AuditAction, 'success' | 'info' | 'warning' | 'error' | 'default'> = {
  Created: 'success',
  Updated: 'info',
  Deleted: 'error',
  StatusChanged: 'warning',
  PriceChanged: 'info',
  BidPlaced: 'success',
  AuctionEnded: 'warning',
  PaymentProcessed: 'success',
  UserRegistered: 'success',
  UserLoggedIn: 'info',
  UserLoggedOut: 'default',
  PermissionChanged: 'warning',
  SettingChanged: 'info',
}

export const ENTITY_TYPE_LABELS: Record<string, string> = {
  Auction: 'Auction',
  Bid: 'Bid',
  User: 'User',
  Order: 'Order',
  Payment: 'Payment',
  Category: 'Category',
  Brand: 'Brand',
  Report: 'Report',
  Notification: 'Notification',
  Setting: 'Setting',
}

export const SERVICE_NAME_LABELS: Record<string, string> = {
  AuctionService: 'Auction Service',
  BidService: 'Bid Service',
  IdentityService: 'Identity Service',
  PaymentService: 'Payment Service',
  NotificationService: 'Notification Service',
  AnalyticsService: 'Analytics Service',
  SearchService: 'Search Service',
  StorageService: 'Storage Service',
}
