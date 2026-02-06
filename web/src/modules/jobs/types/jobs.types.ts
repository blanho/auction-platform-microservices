export type JobStatus =
  | 'Initializing'
  | 'Pending'
  | 'Processing'
  | 'Completed'
  | 'CompletedWithErrors'
  | 'Failed'
  | 'Cancelled'

export type JobType =
  | 'AuctionExport'
  | 'AuctionImport'
  | 'ReportGeneration'
  | 'DataMigration'
  | 'BulkUpdate'
  | 'BulkDelete'
  | 'Cleanup'

export type JobPriority = 'Low' | 'Normal' | 'High' | 'Critical'

export type JobItemStatus = 'Pending' | 'Processing' | 'Completed' | 'Failed' | 'Skipped'

export interface JobDto {
  id: string
  jobType: JobType
  status: JobStatus
  priority: JobPriority
  correlationId: string
  totalItems: number
  completedItems: number
  failedItems: number
  progress: number
  errorMessage?: string
  createdAt: string
  startedAt?: string
  completedAt?: string
  requestedBy?: string
}

export interface JobSummaryDto {
  id: string
  jobType: JobType
  status: JobStatus
  priority: JobPriority
  totalItems: number
  completedItems: number
  failedItems: number
  progress: number
  createdAt: string
}

export interface JobItemDto {
  id: string
  jobId: string
  sequenceNumber: number
  status: JobItemStatus
  retryCount: number
  maxRetryCount: number
  errorMessage?: string
  createdAt: string
  processedAt?: string
}

export interface JobFilterParams {
  jobType?: string
  status?: string
  correlationId?: string
  page?: number
  pageSize?: number
}

export interface JobItemFilterParams {
  status?: JobItemStatus
  page?: number
  pageSize?: number
}

export interface JobStatsData {
  initializing: number
  pending: number
  processing: number
  completed: number
  completedWithErrors: number
  failed: number
  cancelled: number
  total: number
}

export type SortField = 'jobType' | 'status' | 'progress' | 'createdAt' | 'priority'
export type SortDirection = 'asc' | 'desc'
