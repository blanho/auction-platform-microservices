import i18n from '@/i18n'
import { formatRelativeTime } from '@/shared/utils/formatters'
import { MILLISECONDS_PER_DAY, MILLISECONDS_PER_HOUR, MILLISECONDS_PER_MINUTE } from '../constants'

export function formatTimeLeft(endTime: string | Date): string {
  const endDate = typeof endTime === 'string' ? new Date(endTime) : endTime
  const diff = endDate.getTime() - Date.now()

  if (diff <= 0) {
    return i18n.t('common:time.ended')
  }

  const days = Math.floor(diff / MILLISECONDS_PER_DAY)
  const hours = Math.floor((diff % MILLISECONDS_PER_DAY) / MILLISECONDS_PER_HOUR)
  const minutes = Math.floor((diff % MILLISECONDS_PER_HOUR) / MILLISECONDS_PER_MINUTE)

  if (days > 0) {
    return i18n.t('common:time.daysHoursLeft', { days, hours })
  }
  if (hours > 0) {
    return i18n.t('common:time.hoursMinutesLeft', { hours, minutes })
  }
  return i18n.t('common:time.minutesLeft', { minutes })
}

export function formatTimeAgo(dateString: string | Date): string {
  return formatRelativeTime(typeof dateString === 'string' ? dateString : dateString.toISOString())
}
