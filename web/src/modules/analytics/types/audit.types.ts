import type { QueryParameters } from '@/shared/types'

export type AuditAction = 'Created' | 'Updated' | 'Deleted' | 'SoftDeleted' | 'Restored'

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
  metadata?: Record<string, unknown> | null
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
