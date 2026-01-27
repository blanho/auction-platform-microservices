import type { AuditAction } from '../types'
import {
  AUDIT_ACTION_LABELS,
  AUDIT_ACTION_COLORS,
  ENTITY_TYPE_LABELS,
  SERVICE_NAME_LABELS,
} from '../constants'

export function getAuditActionLabel(action: AuditAction): string {
  return AUDIT_ACTION_LABELS[action] || action
}

export function getAuditActionColor(
  action: AuditAction
): 'success' | 'info' | 'warning' | 'error' | 'default' {
  return AUDIT_ACTION_COLORS[action] || 'default'
}

export function getEntityTypeLabel(entityType: string): string {
  return ENTITY_TYPE_LABELS[entityType] || entityType
}

export function getServiceNameLabel(serviceName: string): string {
  return SERVICE_NAME_LABELS[serviceName] || serviceName
}

export function formatAuditTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  }).format(date)
}

export function parseJsonSafely(json: string | undefined | null): Record<string, unknown> | null {
  if (!json) return null
  try {
    return JSON.parse(json)
  } catch {
    return null
  }
}

export function formatChangedProperties(properties: string[] | undefined): string {
  if (!properties || properties.length === 0) return '-'
  return properties.join(', ')
}
