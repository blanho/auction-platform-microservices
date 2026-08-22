import i18n, { getCurrentLocale } from '@/i18n'
import {
  AUDIT_ACTION_COLORS,
  AUDIT_ACTION_LABELS,
  ENTITY_TYPE_LABELS,
  SERVICE_NAME_LABELS,
} from '../constants'
import type { AuditAction } from '../types'

export function getAuditActionLabel(action: AuditAction): string {
  return i18n.t(`analytics:auditActions.${action}`, {
    defaultValue: AUDIT_ACTION_LABELS[action] || action,
  })
}

export function getAuditActionColor(
  action: AuditAction
): 'success' | 'info' | 'warning' | 'error' | 'default' {
  return AUDIT_ACTION_COLORS[action] || 'default'
}

export function getEntityTypeLabel(entityType: string): string {
  return i18n.t(`analytics:entityTypes.${entityType}`, {
    defaultValue: ENTITY_TYPE_LABELS[entityType] || entityType,
  })
}

export function getServiceNameLabel(serviceName: string): string {
  return i18n.t(`analytics:serviceNames.${serviceName}`, {
    defaultValue: SERVICE_NAME_LABELS[serviceName] || serviceName,
  })
}

export function formatAuditTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  return new Intl.DateTimeFormat(getCurrentLocale(), {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).format(date)
}

export function parseJsonSafely(json: string | undefined | null): Record<string, unknown> | null {
  if (!json) {
    return null
  }
  try {
    return JSON.parse(json)
  } catch {
    return null
  }
}

export function formatChangedProperties(properties: string[] | undefined): string {
  if (!properties || properties.length === 0) {
    return '-'
  }
  return properties.join(', ')
}
