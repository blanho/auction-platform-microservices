import type { SettingCategory } from '../types'

export const SETTING_CATEGORY = {
  PLATFORM: 'Platform',
  AUCTION: 'Auction',
  NOTIFICATION: 'Notification',
  SECURITY: 'Security',
  EMAIL: 'Email',
} as const

export const SETTING_CATEGORY_LABELS: Record<SettingCategory, string> = {
  Platform: 'Platform Settings',
  Auction: 'Auction Settings',
  Notification: 'Notification Settings',
  Security: 'Security Settings',
  Email: 'Email Settings',
}

export const SETTING_DATA_TYPES = [
  'string',
  'number',
  'boolean',
  'json',
  'email',
  'url',
  'percentage',
  'currency',
] as const

export const SETTING_DATA_TYPE_LABELS: Record<string, string> = {
  string: 'Text',
  number: 'Number',
  boolean: 'Yes/No',
  json: 'JSON',
  email: 'Email',
  url: 'URL',
  percentage: 'Percentage',
  currency: 'Currency',
}
