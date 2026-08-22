import i18n, { getCurrentLocale } from '@/i18n'
import { SETTING_CATEGORY_LABELS, SETTING_DATA_TYPE_LABELS } from '../constants'
import type { SettingCategory } from '../types'

export function getSettingCategoryLabel(category: SettingCategory): string {
  return i18n.t(`analytics:settings.categoryLabels.${category}`, {
    defaultValue: SETTING_CATEGORY_LABELS[category] || category,
  })
}

export function getSettingDataTypeLabel(dataType: string): string {
  return i18n.t(`analytics:settings.dataTypes.${dataType}`, {
    defaultValue: SETTING_DATA_TYPE_LABELS[dataType] || dataType,
  })
}

export function formatSettingValue(value: string, dataType?: string): string {
  if (!dataType) {
    return value
  }

  switch (dataType) {
    case 'boolean':
      return i18n.t(value === 'true' ? 'common:yes' : 'common:no')
    case 'percentage':
      return `${value}%`
    case 'currency':
      return new Intl.NumberFormat(getCurrentLocale(), {
        style: 'currency',
        currency: 'USD',
      }).format(parseFloat(value) || 0)
    case 'json':
      try {
        return JSON.stringify(JSON.parse(value), null, 2)
      } catch {
        return value
      }
    default:
      return value
  }
}

export function validateSettingValue(value: string, dataType?: string): string | null {
  if (!dataType) {
    return null
  }

  switch (dataType) {
    case 'number':
    case 'percentage':
    case 'currency':
      if (isNaN(parseFloat(value))) {
        return i18n.t('common:validation.validNumber')
      }
      break
    case 'boolean':
      if (value !== 'true' && value !== 'false') {
        return i18n.t('common:validation.boolean')
      }
      break
    case 'email':
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        return i18n.t('common:validation.validEmail')
      }
      break
    case 'url':
      try {
        new URL(value)
      } catch {
        return i18n.t('common:validation.validUrl')
      }
      break
    case 'json':
      try {
        JSON.parse(value)
      } catch {
        return i18n.t('common:validation.validJson')
      }
      break
  }

  return null
}

export function formatSettingTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  return new Intl.DateTimeFormat(getCurrentLocale(), {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}
