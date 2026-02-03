import { http } from '@/services/http'
import type {
  BackgroundJobProgress,
  BackgroundJobResponse,
  BackgroundJobStartResponse,
  BackgroundJobStatus,
  ExportJobRequest,
  ReportJobRequest,
} from '@/shared/types/background-jobs.types'

function mapResponseToProgress(response: BackgroundJobResponse): BackgroundJobProgress {
  return {
    jobId: response.id,
    jobType: response.jobType,
    name: getJobDisplayName(response.jobType),
    status: response.status as BackgroundJobStatus,
    totalItems: response.totalItems,
    processedItems: response.processedItems,
    progressPercentage: response.progressPercentage,
    estimatedSecondsRemaining: response.estimatedSecondsRemaining,
    resultFileName: response.resultFileName,
    errorMessage: response.errorMessage,
    createdAt: response.createdAt,
    startedAt: response.startedAt,
    completedAt: response.completedAt,
  }
}

function getJobDisplayName(jobType: string): string {
  const displayNames: Record<string, string> = {
    'export-auctions': 'Export Auctions',
    'export-users': 'Export Users',
    'generate-report': 'Generate Report',
    'bulk-import': 'Bulk Import',
  }
  return displayNames[jobType] || jobType.replace(/-/g, ' ').replace(/\b\w/g, (l) => l.toUpperCase())
}

export const backgroundJobsApi = {
  async getMyJobs(type?: string): Promise<BackgroundJobProgress[]> {
    const response = await http.get<BackgroundJobResponse[]>('/jobs', {
      params: type ? { jobType: type } : undefined,
    })
    return response.data.map(mapResponseToProgress)
  },

  async getJobStatus(jobId: string): Promise<BackgroundJobProgress> {
    const response = await http.get<BackgroundJobResponse>(`/jobs/${jobId}`)
    return mapResponseToProgress(response.data)
  },

  async cancelJob(jobId: string): Promise<void> {
    await http.post(`/jobs/${jobId}/cancel`)
  },

  async downloadJobResult(jobId: string): Promise<Blob> {
    const response = await http.get<Blob>(`/jobs/${jobId}/download`, {
      responseType: 'blob',
    })
    return response.data
  },

  async startExportJob(request: ExportJobRequest): Promise<BackgroundJobStartResponse> {
    const response = await http.post<BackgroundJobStartResponse>('/jobs/export', request)
    return response.data
  },

  async startReportJob(request: ReportJobRequest): Promise<BackgroundJobStartResponse> {
    const response = await http.post<BackgroundJobStartResponse>('/jobs/report', request)
    return response.data
  },

  downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
  },
}
