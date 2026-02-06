import { http } from '@/services/http'
import type { PaginatedResponse } from '@/shared/types'
import type {
  JobDto,
  JobSummaryDto,
  JobItemDto,
  JobFilterParams,
  JobItemFilterParams,
} from '../types'

export const jobsApi = {
  async getJobs(params?: JobFilterParams): Promise<PaginatedResponse<JobSummaryDto>> {
    const query = params as Record<string, unknown> | undefined
    const response = await http.get<PaginatedResponse<JobSummaryDto>>('/jobs', query)
    return response.data
  },

  async getJob(id: string): Promise<JobDto> {
    const response = await http.get<JobDto>(`/jobs/${id}`)
    return response.data
  },

  async getJobItems(
    jobId: string,
    params?: JobItemFilterParams
  ): Promise<PaginatedResponse<JobItemDto>> {
    const query = params as Record<string, unknown> | undefined
    const response = await http.get<PaginatedResponse<JobItemDto>>(`/jobs/${jobId}/items`, query)
    return response.data
  },

  async cancelJob(id: string): Promise<void> {
    await http.post(`/jobs/${id}/cancel`)
  },

  async retryJob(id: string): Promise<void> {
    await http.post(`/jobs/${id}/retry`)
  },
}
