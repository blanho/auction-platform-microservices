import { http } from '@/services/http'
import type {
  Notification,
  NotificationSummary,
  NotificationFilters,
  NotificationPreferences,
  CreateNotificationDto,
  BroadcastNotificationDto,
  AdminNotificationFilters,
  NotificationStatsDto,
} from '../types/notification.types'
import type { PaginatedResponse } from '@/shared/types'

export const notificationsApi = {
  async getNotifications(filters: NotificationFilters): Promise<Notification[]> {
    const response = await http.get<Notification[]>('/notifications', {
      params: {
        type: filters.type,
        status: filters.status,
      },
    })
    return response.data
  },

  async getSummary(): Promise<NotificationSummary> {
    const response = await http.get<NotificationSummary>('/notifications/summary')
    return response.data
  },

  async getUnreadNotifications(): Promise<Notification[]> {
    const response = await http.get<Notification[]>('/notifications/unread')
    return response.data
  },

  async markAsRead(id: string): Promise<void> {
    await http.put(`/notifications/${id}/read`)
  },

  async markAllAsRead(): Promise<void> {
    await http.put('/notifications/read-all')
  },

  async archive(id: string): Promise<void> {
    await http.post(`/notifications/${id}/archive`)
  },

  async delete(id: string): Promise<void> {
    await http.delete(`/notifications/${id}`)
  },

  async getPreferences(): Promise<NotificationPreferences> {
    const response = await http.get<NotificationPreferences>('/notifications/preferences')
    return response.data
  },

  async updatePreferences(
    preferences: Partial<NotificationPreferences>
  ): Promise<NotificationPreferences> {
    const response = await http.put<NotificationPreferences>(
      '/notifications/preferences',
      preferences
    )
    return response.data
  },

  async createNotification(data: CreateNotificationDto): Promise<Notification> {
    const response = await http.post<Notification>('/notifications', data)
    return response.data
  },

  async getAllNotifications(
    page: number,
    pageSize: number,
    filters?: AdminNotificationFilters
  ): Promise<PaginatedResponse<Notification>> {
    const response = await http.get<PaginatedResponse<Notification>>('/notifications/admin/all', {
      params: {
        page,
        pageSize,
        userId: filters?.userId,
        type: filters?.type,
        status: filters?.status,
      },
    })
    return response.data
  },

  async broadcastNotification(data: BroadcastNotificationDto): Promise<void> {
    await http.post('/notifications/admin/broadcast', data)
  },

  async getNotificationStats(): Promise<NotificationStatsDto> {
    const response = await http.get<NotificationStatsDto>('/notifications/admin/stats')
    return response.data
  },
}
