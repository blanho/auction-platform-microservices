export const ANALYTICS_REFRESH_INTERVALS = {
  REAL_TIME: 10000,
  DASHBOARD: 30000,
  METRICS: 60000,
} as const

export const ANALYTICS_STALE_TIMES = {
  DASHBOARD: 30000,
  METRICS: 60000,
  REPORTS: 30000,
} as const

export const ANALYTICS_DEFAULTS = {
  DEFAULT_DAYS: 30,
  DEFAULT_LIMIT: 10,
  DEFAULT_PERIOD: 'week',
  DEFAULT_GRANULARITY: 'day',
} as const

export const ANALYTICS_PERIODS = [
  { value: 'day', label: 'Day' },
  { value: 'week', label: 'Week' },
  { value: 'month', label: 'Month' },
  { value: 'year', label: 'Year' },
] as const

export const ANALYTICS_TIME_RANGES = [
  { value: '7d', label: '7D' },
  { value: '30d', label: '30D' },
  { value: '90d', label: '90D' },
  { value: '1y', label: '1Y' },
] as const

export const ANALYTICS_GRANULARITIES = [
  { value: 'hour', label: 'Hourly' },
  { value: 'day', label: 'Daily' },
  { value: 'week', label: 'Weekly' },
  { value: 'month', label: 'Monthly' },
] as const
