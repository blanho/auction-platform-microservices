import type { TFunction } from 'i18next'

export function formatTimeLeft(endTime: string, t: TFunction<'users'>): string {
  if (!endTime) {
    return '--'
  }
  const diff = new Date(endTime).getTime() - Date.now()
  if (diff <= 0) {
    return t('myAuctions.time.ended')
  }
  const days = Math.floor(diff / (24 * 60 * 60 * 1000))
  const hours = Math.floor((diff % (24 * 60 * 60 * 1000)) / (60 * 60 * 1000))
  if (days > 0) {
    return t('myAuctions.time.daysHours', { days, hours })
  }
  const minutes = Math.floor((diff % (60 * 60 * 1000)) / (60 * 1000))
  return t('myAuctions.time.hoursMinutes', { hours, minutes })
}
