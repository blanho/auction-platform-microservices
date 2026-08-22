import { getCurrentLocale } from '@/i18n'

export function formatCurrency(amount: number, currency = 'USD'): string {
  return new Intl.NumberFormat(getCurrentLocale(), {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount)
}

export function formatRelativeTime(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const deltaInSeconds = Math.round((date.getTime() - now.getTime()) / 1000)
  const absoluteSeconds = Math.abs(deltaInSeconds)

  const formatter = new Intl.RelativeTimeFormat(getCurrentLocale(), {
    numeric: 'auto',
    style: 'short',
  })

  if (absoluteSeconds < 60) {
    return formatter.format(deltaInSeconds, 'second')
  }

  if (absoluteSeconds < 60 * 60) {
    return formatter.format(Math.round(deltaInSeconds / 60), 'minute')
  }

  if (absoluteSeconds < 60 * 60 * 24) {
    return formatter.format(Math.round(deltaInSeconds / (60 * 60)), 'hour')
  }

  if (absoluteSeconds < 60 * 60 * 24 * 7) {
    return formatter.format(Math.round(deltaInSeconds / (60 * 60 * 24)), 'day')
  }

  if (absoluteSeconds < 60 * 60 * 24 * 30) {
    return formatter.format(Math.round(deltaInSeconds / (60 * 60 * 24 * 7)), 'week')
  }

  if (absoluteSeconds < 60 * 60 * 24 * 365) {
    return formatter.format(Math.round(deltaInSeconds / (60 * 60 * 24 * 30)), 'month')
  }

  return formatter.format(Math.round(deltaInSeconds / (60 * 60 * 24 * 365)), 'year')
}

export function formatNumber(num: number): string {
  return new Intl.NumberFormat(getCurrentLocale()).format(num)
}

export function formatDate(dateString: string, options?: Intl.DateTimeFormatOptions): string {
  const date = new Date(dateString)
  return date.toLocaleDateString(
    getCurrentLocale(),
    options ?? {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    }
  )
}

export function formatDateTime(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleString(getCurrentLocale(), {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTime(dateString: string): string {
  return new Date(dateString).toLocaleTimeString(getCurrentLocale(), {
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatPercentage(value: number, decimals = 1): string {
  return `${value.toFixed(decimals)}%`
}
