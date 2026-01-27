import { http } from '@/services/http'
import type { 
  NotificationRecord, 
  NotificationRecordFilterDto, 
  NotificationRecordStatsDto 
} from '../types/template.types'
import type { PaginatedResponse } from '@/shared/types'

export const recordsApi = {
  async getRecords(filter: NotificationRecordFilterDto): Promise<PaginatedResponse<NotificationRecord>> {
    const response = await http.get<PaginatedResponse<NotificationRecord>>('/notifications/records', {
      params: filter
    })
    return response.data
  },

  async getRecordById(id: string): Promise<NotificationRecord> {
    const response = await http.get<NotificationRecord>(`/notifications/records/${id}`)
    return response.data
  },

  async getRecordsByUser(userId: string, limit: number = 50): Promise<NotificationRecord[]> {
    const response = await http.get<NotificationRecord[]>(`/notifications/records/user/${userId}`, {
      params: { limit }
    })
    return response.data
  },

  async getRecordStats(): Promise<NotificationRecordStatsDto> {
    const response = await http.get<NotificationRecordStatsDto>('/notifications/records/stats')
    return response.data
  },
}
