import type { QueryParameters } from '@/shared/types'

export type AuditAction =
  | 'Created'
  | 'Updated'
  | 'Deleted'
  | 'StatusChanged'
  | 'PriceChanged'
  | 'BidPlaced'
  | 'AuctionEnded'
  | 'PaymentProcessed'
  | 'UserRegistered'
  | 'UserLoggedIn'
  | 'UserLoggedOut'
  | 'PermissionChanged'
  | 'SettingChanged'

export interface AuditLog {
  id: string
  entityId: string
  entityType: string
  action: AuditAction
  actionName: string
  oldValues?: string
  newValues?: string
  changedProperties?: string[]
  userId: string
  username?: string
  serviceName: string
  correlationId?: string
  ipAddress?: string
  timestamp: string
}

export interface AuditLogFilter {
  entityId?: string
  entityType?: string
  userId?: string
  serviceName?: string
  action?: AuditAction
  fromDate?: string
  toDate?: string
}

export interface AuditLogQueryParams extends QueryParameters {
  entityId?: string
  entityType?: string
  userId?: string
  serviceName?: string
  action?: AuditAction
  fromDate?: string
  toDate?: string
}
