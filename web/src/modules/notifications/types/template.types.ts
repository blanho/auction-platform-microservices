import type { QueryParameters } from '@/shared/types'

export interface NotificationTemplate {
  id: string
  key: string
  name: string
  description?: string
  subject: string
  body: string
  channel: NotificationChannel
  isActive: boolean
  variables?: string[]
  createdAt: string
  updatedAt: string
}

export type NotificationChannel = 'email' | 'sms' | 'push' | 'in_app'

export interface CreateTemplateDto {
  key: string
  name: string
  description?: string
  subject: string
  body: string
  channel: NotificationChannel
  variables?: string[]
}

export interface UpdateTemplateDto {
  name?: string
  description?: string
  subject?: string
  body?: string
  channel?: NotificationChannel
  isActive?: boolean
  variables?: string[]
}

export interface NotificationRecord {
  id: string
  userId: string
  channel: NotificationChannel
  templateKey?: string
  subject?: string
  message: string
  recipient: string
  status: RecordStatus
  errorMessage?: string
  sentAt?: string
  deliveredAt?: string
  readAt?: string
  metadata?: Record<string, unknown>
  createdAt: string
}

export type RecordStatus = 'pending' | 'sent' | 'delivered' | 'failed' | 'bounced'

export interface NotificationRecordFilter {
  userId?: string
  channel?: string
  status?: string
  templateKey?: string
  fromDate?: string
  toDate?: string
}

export interface NotificationRecordFilterDto extends QueryParameters {
  userId?: string
  channel?: string
  status?: string
  templateKey?: string
  fromDate?: string
  toDate?: string
}

export interface NotificationRecordStatsDto {
  totalRecords: number
  sentCount: number
  deliveredCount: number
  failedCount: number
  byChannel: Record<string, number>
  byStatus: Record<string, number>
}
