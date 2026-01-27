import { http } from '@/services/http'
import type { 
  NotificationTemplate, 
  CreateTemplateDto, 
  UpdateTemplateDto 
} from '../types/template.types'
import type { PaginatedResponse } from '@/shared/types'

export const templatesApi = {
  async getTemplates(page: number = 1, pageSize: number = 20): Promise<PaginatedResponse<NotificationTemplate>> {
    const response = await http.get<PaginatedResponse<NotificationTemplate>>('/notifications/templates', {
      params: { page, pageSize }
    })
    return response.data
  },

  async getActiveTemplates(): Promise<NotificationTemplate[]> {
    const response = await http.get<NotificationTemplate[]>('/notifications/templates/active')
    return response.data
  },

  async getTemplateByKey(key: string): Promise<NotificationTemplate> {
    const response = await http.get<NotificationTemplate>(`/notifications/templates/${key}`)
    return response.data
  },

  async getTemplateById(id: string): Promise<NotificationTemplate> {
    const response = await http.get<NotificationTemplate>(`/notifications/templates/id/${id}`)
    return response.data
  },

  async createTemplate(dto: CreateTemplateDto): Promise<NotificationTemplate> {
    const response = await http.post<NotificationTemplate>('/notifications/templates', dto)
    return response.data
  },

  async updateTemplate(id: string, dto: UpdateTemplateDto): Promise<NotificationTemplate> {
    const response = await http.put<NotificationTemplate>(`/notifications/templates/${id}`, dto)
    return response.data
  },

  async deleteTemplate(id: string): Promise<void> {
    await http.delete(`/notifications/templates/${id}`)
  },

  async checkTemplateExists(key: string): Promise<boolean> {
    const response = await http.get<boolean>(`/notifications/templates/exists/${key}`)
    return response.data
  },
}
