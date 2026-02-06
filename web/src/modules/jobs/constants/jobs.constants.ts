import type { JobStatus, JobType, JobPriority, JobItemStatus } from '../types'

export const JOB_STATUS = {
  INITIALIZING: 'Initializing',
  PENDING: 'Pending',
  PROCESSING: 'Processing',
  COMPLETED: 'Completed',
  COMPLETED_WITH_ERRORS: 'CompletedWithErrors',
  FAILED: 'Failed',
  CANCELLED: 'Cancelled',
} as const

export const JOB_TYPE = {
  AUCTION_EXPORT: 'AuctionExport',
  AUCTION_IMPORT: 'AuctionImport',
  REPORT_GENERATION: 'ReportGeneration',
  DATA_MIGRATION: 'DataMigration',
  BULK_UPDATE: 'BulkUpdate',
  BULK_DELETE: 'BulkDelete',
  CLEANUP: 'Cleanup',
} as const

export const JOB_PRIORITY = {
  LOW: 'Low',
  NORMAL: 'Normal',
  HIGH: 'High',
  CRITICAL: 'Critical',
} as const

export const JOB_ITEM_STATUS = {
  PENDING: 'Pending',
  PROCESSING: 'Processing',
  COMPLETED: 'Completed',
  FAILED: 'Failed',
  SKIPPED: 'Skipped',
} as const

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

export const JOB_PRIORITY_COLORS: Record<
  JobPriority,
  'default' | 'info' | 'warning' | 'error'
> = {
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
