import { MILLISECONDS_PER_MINUTE, MILLISECONDS_PER_HOUR, MILLISECONDS_PER_DAY, TIME_CONSTANTS } from '../constants'

export function formatTimeLeft(endTime: string | Date): string {
  const endDate = typeof endTime === 'string' ? new Date(endTime) : endTime
  const diff = endDate.getTime() - Date.now()
  
  if (diff <= 0) return 'Ended'
  
  const days = Math.floor(diff / MILLISECONDS_PER_DAY)
  const hours = Math.floor((diff % MILLISECONDS_PER_DAY) / MILLISECONDS_PER_HOUR)
  const minutes = Math.floor((diff % MILLISECONDS_PER_HOUR) / MILLISECONDS_PER_MINUTE)
  
  if (days > 0) return `${days}d ${hours}h left`
  if (hours > 0) return `${hours}h ${minutes}m left`
  return `${minutes}m left`
}

export function formatTimeAgo(dateString: string | Date): string {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  const diff = Date.now() - date.getTime()
  const diffDays = Math.floor(diff / MILLISECONDS_PER_DAY)
  
  if (diffDays < 1) return 'today'
  if (diffDays === 1) return 'yesterday'
  if (diffDays < TIME_CONSTANTS.DAYS_PER_WEEK) return `${diffDays} days ago`
  if (diffDays < TIME_CONSTANTS.DAYS_PER_MONTH) {
    const weeks = Math.floor(diffDays / TIME_CONSTANTS.DAYS_PER_WEEK)
    return `${weeks} ${weeks === 1 ? 'week' : 'weeks'} ago`
  }
  if (diffDays < TIME_CONSTANTS.DAYS_PER_YEAR) {
    const months = Math.floor(diffDays / TIME_CONSTANTS.DAYS_PER_MONTH)
    return `${months} ${months === 1 ? 'month' : 'months'} ago`
  }
  const years = Math.floor(diffDays / TIME_CONSTANTS.DAYS_PER_YEAR)
  return `${years} ${years === 1 ? 'year' : 'years'} ago`
}

export function isEndingSoon(endTime: string | Date, hoursThreshold: number = 24): boolean {
  const endDate = typeof endTime === 'string' ? new Date(endTime) : endTime
  const diff = endDate.getTime() - Date.now()
  return diff > 0 && diff <= hoursThreshold * MILLISECONDS_PER_HOUR
}
