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

export const SETTING_CATEGORY_DESCRIPTIONS: Record<SettingCategory, string> = {
  Platform: 'General platform configuration',
  Auction: 'Auction-related settings and fees',
  Notification: 'Notification preferences and templates',
  Security: 'Security and authentication settings',
  Email: 'Email server and template configuration',
}

export const SETTING_CATEGORY_ICONS: Record<SettingCategory, string> = {
  Platform: 'Settings',
  Auction: 'Gavel',
  Notification: 'Notifications',
  Security: 'Security',
  Email: 'Email',
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
