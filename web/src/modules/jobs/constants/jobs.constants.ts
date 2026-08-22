import type { JobItemStatus, JobPriority, JobStatus, JobType } from '../types'

export const JOB_STATUS_LABELS: Record<JobStatus, string> = {
  Initializing: 'Initializing',
  Pending: 'Pending',
  Processing: 'Processing',
  Completed: 'Completed',
  CompletedWithErrors: 'Completed w/ Errors',
  Failed: 'Failed',
  Cancelled: 'Cancelled',
}

export const JOB_STATUS_COLORS: Record<
  JobStatus,
  'default' | 'info' | 'primary' | 'success' | 'warning' | 'error'
> = {
  Initializing: 'default',
  Pending: 'info',
  Processing: 'primary',
  Completed: 'success',
  CompletedWithErrors: 'warning',
  Failed: 'error',
  Cancelled: 'default',
}

export const JOB_TYPE_LABELS: Record<JobType, string> = {
  AuctionExport: 'Auction Export',
  AuctionImport: 'Auction Import',
  ReportGeneration: 'Report Generation',
  DataMigration: 'Data Migration',
  BulkUpdate: 'Bulk Update',
  BulkDelete: 'Bulk Delete',
  Cleanup: 'Cleanup',
}

export const JOB_PRIORITY_LABELS: Record<JobPriority, string> = {
  Low: 'Low',
  Normal: 'Normal',
  High: 'High',
  Critical: 'Critical',
}

export const JOB_PRIORITY_COLORS: Record<JobPriority, 'default' | 'info' | 'warning' | 'error'> = {
  Low: 'default',
  Normal: 'info',
  High: 'warning',
  Critical: 'error',
}

export const JOB_ITEM_STATUS_LABELS: Record<JobItemStatus, string> = {
  Pending: 'Pending',
  Processing: 'Processing',
  Completed: 'Completed',
  Failed: 'Failed',
  Skipped: 'Skipped',
}

export const JOB_ITEM_STATUS_COLORS: Record<
  JobItemStatus,
  'default' | 'primary' | 'success' | 'error' | 'warning'
> = {
  Pending: 'default',
  Processing: 'primary',
  Completed: 'success',
  Failed: 'error',
  Skipped: 'warning',
}
