import type { SettingCategory } from '../types'
import {
  SETTING_CATEGORY_LABELS,
  SETTING_CATEGORY_DESCRIPTIONS,
  SETTING_DATA_TYPE_LABELS,
} from '../constants'

export function getSettingCategoryLabel(category: SettingCategory): string {
  return SETTING_CATEGORY_LABELS[category] || category
}

export function getSettingCategoryDescription(category: SettingCategory): string {
  return SETTING_CATEGORY_DESCRIPTIONS[category] || ''
}

export function getSettingDataTypeLabel(dataType: string): string {
  return SETTING_DATA_TYPE_LABELS[dataType] || dataType
}

export function formatSettingValue(value: string, dataType?: string): string {
  if (!dataType) {return value}

  switch (dataType) {
    case 'boolean':
      return value === 'true' ? 'Yes' : 'No'
    case 'percentage':
      return `${value}%`
    case 'currency':
      return new Intl.NumberFormat('en-US', {
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

export function parseSettingValue(value: string, dataType?: string): unknown {
  if (!dataType) {return value}

  switch (dataType) {
    case 'number':
    case 'percentage':
    case 'currency':
      return parseFloat(value) || 0
    case 'boolean':
      return value === 'true'
    case 'json':
      try {
        return JSON.parse(value)
      } catch {
        return value
      }
    default:
      return value
  }
}

export function validateSettingValue(value: string, dataType?: string): string | null {
  if (!dataType) {return null}

  switch (dataType) {
    case 'number':
    case 'percentage':
    case 'currency':
      if (isNaN(parseFloat(value))) {
        return 'Must be a valid number'
      }
      break
    case 'boolean':
      if (value !== 'true' && value !== 'false') {
        return 'Must be true or false'
      }
      break
    case 'email':
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
        return 'Must be a valid email address'
      }
      break
    case 'url':
      try {
        new URL(value)
      } catch {
        return 'Must be a valid URL'
      }
      break
    case 'json':
      try {
        JSON.parse(value)
      } catch {
        return 'Must be valid JSON'
      }
      break
  }

  return null
}

export function formatSettingTimestamp(timestamp: string): string {
  const date = new Date(timestamp)
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date)
}
