import { HEALTH_STATUS_LABELS, HEALTH_STATUS_COLORS } from '../constants'

export function getHealthStatusLabel(status: string): string {
  const normalizedStatus = status.toLowerCase()
  return HEALTH_STATUS_LABELS[normalizedStatus as keyof typeof HEALTH_STATUS_LABELS] ?? status
}

export function getHealthStatusColor(status: string): string {
  const normalizedStatus = status.toLowerCase()
  return HEALTH_STATUS_COLORS[normalizedStatus as keyof typeof HEALTH_STATUS_COLORS] ?? 'text.secondary'
}

export function isHealthy(status: string): boolean {
  const normalizedStatus = status.toLowerCase()
  return normalizedStatus === 'healthy' || normalizedStatus === 'connected'
}

export function isDegraded(status: string): boolean {
  const normalizedStatus = status.toLowerCase()
  return normalizedStatus === 'degraded' || normalizedStatus === 'unknown'
}

export function isUnhealthy(status: string): boolean {
  const normalizedStatus = status.toLowerCase()
  return (
    normalizedStatus === 'disconnected' ||
    normalizedStatus === 'service_unavailable' ||
    normalizedStatus === 'error'
  )
}
