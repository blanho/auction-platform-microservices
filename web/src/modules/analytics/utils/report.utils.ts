import type { ReportStatus, ReportType, ReportPriority } from '../types'
import {
  REPORT_STATUS_LABELS,
  REPORT_STATUS_COLORS,
  REPORT_TYPE_LABELS,
  REPORT_PRIORITY_LABELS,
  REPORT_PRIORITY_COLORS,
} from '../constants'

export function getReportStatusLabel(status: ReportStatus): string {
  return REPORT_STATUS_LABELS[status]
}

export function getReportStatusColor(
  status: ReportStatus
): 'default' | 'warning' | 'success' | 'error' {
  return REPORT_STATUS_COLORS[status]
}

export function getReportTypeLabel(type: ReportType): string {
  return REPORT_TYPE_LABELS[type]
}

export function getReportPriorityLabel(priority: ReportPriority): string {
  return REPORT_PRIORITY_LABELS[priority]
}

export function getReportPriorityColor(
  priority: ReportPriority
): 'default' | 'info' | 'warning' | 'error' {
  return REPORT_PRIORITY_COLORS[priority]
}
