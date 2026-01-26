import type { ReportStatus, ReportType, ReportPriority } from '../types'

export const REPORT_STATUS = {
  PENDING: 'Pending',
  UNDER_REVIEW: 'UnderReview',
  RESOLVED: 'Resolved',
  DISMISSED: 'Dismissed',
} as const

export const REPORT_TYPE = {
  FRAUD: 'Fraud',
  FAKE_ITEM: 'FakeItem',
  NON_PAYMENT: 'NonPayment',
  HARASSMENT: 'Harassment',
  INAPPROPRIATE_CONTENT: 'InappropriateContent',
  SUSPICIOUS_ACTIVITY: 'SuspiciousActivity',
  OTHER: 'Other',
} as const

export const REPORT_PRIORITY = {
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
  CRITICAL: 'Critical',
} as const

export const REPORT_STATUS_LABELS: Record<ReportStatus, string> = {
  Pending: 'Pending',
  UnderReview: 'Under Review',
  Resolved: 'Resolved',
  Dismissed: 'Dismissed',
}

export const REPORT_STATUS_COLORS: Record<ReportStatus, 'default' | 'warning' | 'success' | 'error'> = {
  Pending: 'warning',
  UnderReview: 'default',
  Resolved: 'success',
  Dismissed: 'error',
}

export const REPORT_TYPE_LABELS: Record<ReportType, string> = {
  Fraud: 'Fraud',
  FakeItem: 'Fake Item',
  NonPayment: 'Non-Payment',
  Harassment: 'Harassment',
  InappropriateContent: 'Inappropriate Content',
  SuspiciousActivity: 'Suspicious Activity',
  Other: 'Other',
}

export const REPORT_PRIORITY_LABELS: Record<ReportPriority, string> = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
}

export const REPORT_PRIORITY_COLORS: Record<
  ReportPriority,
  'default' | 'info' | 'warning' | 'error'
> = {
  Low: 'default',
  Medium: 'info',
  High: 'warning',
  Critical: 'error',
}
