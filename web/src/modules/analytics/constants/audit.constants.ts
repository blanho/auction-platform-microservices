import type { AuditAction } from '../types'

export const AUDIT_ACTION = {
  CREATED: 'Created',
  UPDATED: 'Updated',
  DELETED: 'Deleted',
  SOFT_DELETED: 'SoftDeleted',
  RESTORED: 'Restored',
} as const

export const AUDIT_ACTION_LABELS: Record<AuditAction, string> = {
  Created: 'Created',
  Updated: 'Updated',
  Deleted: 'Deleted',
  SoftDeleted: 'Soft Deleted',
  Restored: 'Restored',
}

export const AUDIT_ACTION_COLORS: Record<
  AuditAction,
  'success' | 'info' | 'warning' | 'error' | 'default'
> = {
  Created: 'success',
  Updated: 'info',
  Deleted: 'error',
  SoftDeleted: 'warning',
  Restored: 'success',
}

export const ENTITY_TYPE_LABELS: Record<string, string> = {
  AuctionAuditData: 'Auction',
  BidAuditData: 'Bid',
  AutoBidAuditData: 'Auto Bid',
  UserAuditData: 'User',
  AuthAuditData: 'Authentication',
  TwoFactorAuditData: 'Two-Factor Authentication',
  OrderAuditData: 'Order',
  WalletAuditData: 'Wallet',
  WalletTransactionAuditData: 'Wallet Transaction',
  JobAuditData: 'Job',
  StoredFileAuditData: 'File',
  NotificationAuditData: 'Notification',
  NotificationPreferenceAuditData: 'Notification Preferences',
  NotificationTemplateAuditData: 'Notification Template',
}

export const SERVICE_NAME_LABELS: Record<string, string> = {
  'auction-service': 'Auction Service',
  'bidding-service': 'Bidding Service',
  'identity-service': 'Identity Service',
  'payment-service': 'Payment Service',
  'notification-service': 'Notification Service',
  'job-service': 'Job Service',
  'storage-service': 'Storage Service',
}

export const AUDIT_ENTITY_TYPES = Object.keys(ENTITY_TYPE_LABELS)
export const AUDIT_SERVICE_NAMES = Object.keys(SERVICE_NAME_LABELS)
