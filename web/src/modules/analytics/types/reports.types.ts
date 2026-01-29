export type ReportType =
  | 'Fraud'
  | 'FakeItem'
  | 'NonPayment'
  | 'Harassment'
  | 'InappropriateContent'
  | 'SuspiciousActivity'
  | 'Other'

export type ReportStatus = 'Pending' | 'UnderReview' | 'Resolved' | 'Dismissed'

export type ReportPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface Report {
  id: string
  reporterUsername: string
  reportedUsername: string
  auctionId?: string
  type: ReportType
  priority: ReportPriority
  reason: string
  description?: string
  status: ReportStatus
  resolution?: string
  resolvedBy?: string
  resolvedAt?: string
  createdAt: string
}

export interface CreateReportRequest {
  reportedUsername: string
  auctionId?: string
  type: ReportType
  reason: string
  description?: string
}

export interface UpdateReportStatusRequest {
  status: ReportStatus
  resolution?: string
}

export interface ReportQueryParams {
  status?: ReportStatus
  type?: ReportType
  priority?: ReportPriority
  reportedUsername?: string
  page?: number
  pageSize?: number
}

export interface ReportStats {
  totalReports: number
  pendingReports: number
  underReviewReports: number
  resolvedReports: number
  dismissedReports: number
  criticalReports: number
  highPriorityReports: number
}
