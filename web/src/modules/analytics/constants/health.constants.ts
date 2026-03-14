export const HEALTH_STATUS = {
  HEALTHY: 'healthy',
  CONNECTED: 'connected',
  DEGRADED: 'degraded',
  UNKNOWN: 'unknown',
  DISCONNECTED: 'disconnected',
  SERVICE_UNAVAILABLE: 'service_unavailable',
} as const

export const HEALTH_STATUS_LABELS = {
  [HEALTH_STATUS.HEALTHY]: 'Healthy',
  [HEALTH_STATUS.CONNECTED]: 'Connected',
  [HEALTH_STATUS.DEGRADED]: 'Degraded',
  [HEALTH_STATUS.UNKNOWN]: 'Unknown',
  [HEALTH_STATUS.DISCONNECTED]: 'Disconnected',
  [HEALTH_STATUS.SERVICE_UNAVAILABLE]: 'Service Unavailable',
} as const

export const HEALTH_STATUS_COLORS = {
  [HEALTH_STATUS.HEALTHY]: 'success.main',
  [HEALTH_STATUS.CONNECTED]: 'success.main',
  [HEALTH_STATUS.DEGRADED]: 'warning.main',
  [HEALTH_STATUS.UNKNOWN]: 'warning.main',
  [HEALTH_STATUS.DISCONNECTED]: 'error.main',
  [HEALTH_STATUS.SERVICE_UNAVAILABLE]: 'error.main',
} as const
