export type BackgroundJobStatus = 
  | 'Pending'
  | 'Running'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'

export interface BackgroundJobResponse {
  id: string
  jobType: string
  status: string
  progressPercentage: number
  processedItems: number
  totalItems: number
  estimatedSecondsRemaining: number
  resultFileName?: string
  errorMessage?: string
  createdAt: string
  startedAt?: string
  completedAt?: string
}

export interface BackgroundJobProgress {
  jobId: string
  jobType: string
  name: string
  status: BackgroundJobStatus
  totalItems: number
  processedItems: number
  progressPercentage: number
  estimatedSecondsRemaining: number
  resultUrl?: string
  resultFileName?: string
  resultFileSizeBytes?: number
  errorMessage?: string
  createdAt: string
  startedAt?: string
  completedAt?: string
  expiresAt?: string
}

export interface BackgroundJobStartResponse {
  jobId: string
  message: string
  statusUrl: string
}

export interface ExportJobRequest {
  format: 'excel' | 'csv' | 'json'
  status?: string
  seller?: string
  startDate?: string
  endDate?: string
}

export interface ReportJobRequest {
  reportType: string
  startDate?: string
  endDate?: string
  format: 'pdf' | 'excel'
}
