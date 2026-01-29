export type SettingCategory = 'Platform' | 'Auction' | 'Notification' | 'Security' | 'Email'

export interface PlatformSetting {
  id: string
  key: string
  value: string
  description?: string
  category: string
  dataType?: string
  isSystem: boolean
  updatedAt: string
  updatedBy?: string
}

export interface CreateSettingRequest {
  key: string
  value: string
  description?: string
  category: SettingCategory
  dataType?: string
  validationRules?: string
}

export interface UpdateSettingRequest {
  value: string
}

export interface BulkUpdateSettingsRequest {
  settings: SettingKeyValue[]
}

export interface SettingKeyValue {
  key: string
  value: string
}
