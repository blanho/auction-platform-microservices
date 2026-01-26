import { http } from '@/services/http'
import type { PaginatedResponse } from '@/shared/types'
import type {
  AdminDashboardStats,
  AdminRecentActivity,
  PlatformHealth,
  PlatformAnalytics,
  AuctionMetrics,
  BidMetrics,
  RevenueMetrics,
  TrendDataPoint,
  CategoryBreakdown,
  TopPerformers,
  RealTimeStats,
  AnalyticsQueryParams,
  TrendsQueryParams,
  UserDashboardStats,
  SellerAnalytics,
  QuickStats,
  TrendingSearch,
  Report,
  CreateReportRequest,
  UpdateReportStatusRequest,
  ReportQueryParams,
  ReportStats,
  AuditLog,
  AuditLogQueryParams,
  PlatformSetting,
  CreateSettingRequest,
  UpdateSettingRequest,
  SettingCategory,
  BulkUpdateSettingsRequest,
} from '../types'

export const dashboardApi = {
  async getStats(): Promise<AdminDashboardStats> {
    const response = await http.get<AdminDashboardStats>('/dashboard/stats')
    return response.data
  },

  async getActivity(limit?: number): Promise<AdminRecentActivity[]> {
    const response = await http.get<AdminRecentActivity[]>('/dashboard/activity', {
      params: { limit },
    })
    return response.data
  },

  async getHealth(): Promise<PlatformHealth> {
    const response = await http.get<PlatformHealth>('/dashboard/health')
    return response.data
  },
}

export const analyticsApi = {
  async getPlatformAnalytics(params?: AnalyticsQueryParams): Promise<PlatformAnalytics> {
    const response = await http.get<PlatformAnalytics>('/analytics', { params })
    return response.data
  },

  async getRealTimeStats(): Promise<RealTimeStats> {
    const response = await http.get<RealTimeStats>('/analytics/realtime')
    return response.data
  },

  async getTopPerformers(limit?: number, period?: string): Promise<TopPerformers> {
    const response = await http.get<TopPerformers>('/analytics/top-performers', {
      params: { limit, period },
    })
    return response.data
  },

  async getRevenueTrends(params?: TrendsQueryParams): Promise<TrendDataPoint[]> {
    const response = await http.get<TrendDataPoint[]>('/analytics/trends/revenue', { params })
    return response.data
  },

  async getAuctionTrends(params?: TrendsQueryParams): Promise<TrendDataPoint[]> {
    const response = await http.get<TrendDataPoint[]>('/analytics/trends/auctions', { params })
    return response.data
  },

  async getCategoryPerformance(startDate?: string, endDate?: string): Promise<CategoryBreakdown[]> {
    const response = await http.get<CategoryBreakdown[]>('/analytics/categories', {
      params: { startDate, endDate },
    })
    return response.data
  },

  async getAuctionMetrics(params?: AnalyticsQueryParams): Promise<AuctionMetrics> {
    const response = await http.get<AuctionMetrics>('/analytics/auctions', { params })
    return response.data
  },

  async getBidMetrics(params?: AnalyticsQueryParams): Promise<BidMetrics> {
    const response = await http.get<BidMetrics>('/analytics/bids', { params })
    return response.data
  },

  async getRevenueMetrics(params?: AnalyticsQueryParams): Promise<RevenueMetrics> {
    const response = await http.get<RevenueMetrics>('/analytics/revenue', { params })
    return response.data
  },
}

export const userAnalyticsApi = {
  async getDashboard(): Promise<UserDashboardStats> {
    const response = await http.get<UserDashboardStats>('/analytics/user/dashboard')
    return response.data
  },

  async getSellerAnalytics(timeRange?: string): Promise<SellerAnalytics> {
    const response = await http.get<SellerAnalytics>('/analytics/user/seller', {
      params: { timeRange },
    })
    return response.data
  },

  async getQuickStats(): Promise<QuickStats> {
    const response = await http.get<QuickStats>('/analytics/user/quick-stats')
    return response.data
  },

  async getTrendingSearches(limit?: number): Promise<TrendingSearch[]> {
    const response = await http.get<{ searches: TrendingSearch[] }>('/analytics/user/trending-searches', {
      params: { limit },
    })
    return response.data.searches
  },
}

export const reportsApi = {
  async getReports(params?: ReportQueryParams): Promise<PaginatedResponse<Report>> {
    const response = await http.get<PaginatedResponse<Report>>('/reports', { params })
    return response.data
  },

  async getReport(id: string): Promise<Report> {
    const response = await http.get<Report>(`/reports/${id}`)
    return response.data
  },

  async createReport(data: CreateReportRequest): Promise<Report> {
    const response = await http.post<Report>('/reports', data)
    return response.data
  },

  async updateReportStatus(id: string, data: UpdateReportStatusRequest): Promise<void> {
    await http.put(`/reports/${id}/status`, data)
  },

  async deleteReport(id: string): Promise<void> {
    await http.delete(`/reports/${id}`)
  },

  async getStats(): Promise<ReportStats> {
    const response = await http.get<ReportStats>('/reports/stats')
    return response.data
  },
}

export const auditLogsApi = {
  async getAuditLogs(params?: AuditLogQueryParams): Promise<PaginatedResponse<AuditLog>> {
    const response = await http.get<PaginatedResponse<AuditLog>>('/auditlogs', { params })
    return response.data
  },

  async getAuditLog(id: string): Promise<AuditLog> {
    const response = await http.get<AuditLog>(`/auditlogs/${id}`)
    return response.data
  },

  async getEntityAuditHistory(entityType: string, entityId: string): Promise<AuditLog[]> {
    const response = await http.get<AuditLog[]>(`/auditlogs/entity/${entityType}/${entityId}`)
    return response.data
  },
}

export const settingsApi = {
  async getSettings(category?: SettingCategory): Promise<PlatformSetting[]> {
    const response = await http.get<PlatformSetting[]>('/settings', {
      params: category ? { category } : undefined,
    })
    return response.data
  },

  async getSetting(id: string): Promise<PlatformSetting> {
    const response = await http.get<PlatformSetting>(`/settings/${id}`)
    return response.data
  },

  async getSettingByKey(key: string): Promise<PlatformSetting> {
    const response = await http.get<PlatformSetting>(`/settings/key/${key}`)
    return response.data
  },

  async createSetting(data: CreateSettingRequest): Promise<PlatformSetting> {
    const response = await http.post<PlatformSetting>('/settings', data)
    return response.data
  },

  async updateSetting(id: string, data: UpdateSettingRequest): Promise<PlatformSetting> {
    const response = await http.put<PlatformSetting>(`/settings/${id}`, data)
    return response.data
  },

  async deleteSetting(id: string): Promise<void> {
    await http.delete(`/settings/${id}`)
  },

  async bulkUpdateSettings(data: BulkUpdateSettingsRequest): Promise<void> {
    await http.put('/settings/bulk', data)
  },
}
