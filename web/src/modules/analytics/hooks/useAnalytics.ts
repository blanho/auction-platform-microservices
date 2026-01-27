import { useQuery } from '@tanstack/react-query'
import { dashboardApi, analyticsApi, userAnalyticsApi, reportsApi, auditLogsApi, settingsApi } from '../api'
import type { AnalyticsQueryParams, TrendsQueryParams, ReportQueryParams, AuditLogQueryParams, SettingCategory } from '../types'

export const analyticsKeys = {
  all: ['analytics'] as const,
  dashboard: () => [...analyticsKeys.all, 'dashboard'] as const,
  dashboardStats: () => [...analyticsKeys.dashboard(), 'stats'] as const,
  dashboardActivity: (limit?: number) => [...analyticsKeys.dashboard(), 'activity', limit] as const,
  dashboardHealth: () => [...analyticsKeys.dashboard(), 'health'] as const,
  platform: (params?: AnalyticsQueryParams) => [...analyticsKeys.all, 'platform', params] as const,
  realtime: () => [...analyticsKeys.all, 'realtime'] as const,
  topPerformers: (limit?: number, period?: string) => [...analyticsKeys.all, 'top-performers', limit, period] as const,
  trends: () => [...analyticsKeys.all, 'trends'] as const,
  revenueTrends: (params?: TrendsQueryParams) => [...analyticsKeys.trends(), 'revenue', params] as const,
  auctionTrends: (params?: TrendsQueryParams) => [...analyticsKeys.trends(), 'auctions', params] as const,
  categories: (startDate?: string, endDate?: string) => [...analyticsKeys.all, 'categories', startDate, endDate] as const,
  metrics: () => [...analyticsKeys.all, 'metrics'] as const,
  auctionMetrics: (params?: AnalyticsQueryParams) => [...analyticsKeys.metrics(), 'auctions', params] as const,
  bidMetrics: (params?: AnalyticsQueryParams) => [...analyticsKeys.metrics(), 'bids', params] as const,
  revenueMetrics: (params?: AnalyticsQueryParams) => [...analyticsKeys.metrics(), 'revenue', params] as const,
  user: () => [...analyticsKeys.all, 'user'] as const,
  userDashboard: () => [...analyticsKeys.user(), 'dashboard'] as const,
  sellerAnalytics: (timeRange?: string) => [...analyticsKeys.user(), 'seller', timeRange] as const,
  quickStats: () => [...analyticsKeys.user(), 'quick-stats'] as const,
  trendingSearches: (limit?: number) => [...analyticsKeys.user(), 'trending-searches', limit] as const,
  reports: () => [...analyticsKeys.all, 'reports'] as const,
  reportList: (params?: ReportQueryParams) => [...analyticsKeys.reports(), 'list', params] as const,
  reportDetail: (id: string) => [...analyticsKeys.reports(), 'detail', id] as const,
  reportStats: () => [...analyticsKeys.reports(), 'stats'] as const,
  auditLogs: () => [...analyticsKeys.all, 'audit-logs'] as const,
  auditLogList: (params?: AuditLogQueryParams) => [...analyticsKeys.auditLogs(), 'list', params] as const,
  auditLogDetail: (id: string) => [...analyticsKeys.auditLogs(), 'detail', id] as const,
  auditLogEntity: (entityType: string, entityId: string) => [...analyticsKeys.auditLogs(), 'entity', entityType, entityId] as const,
  settings: () => [...analyticsKeys.all, 'settings'] as const,
  settingsList: (category?: SettingCategory) => [...analyticsKeys.settings(), 'list', category] as const,
  settingDetail: (id: string) => [...analyticsKeys.settings(), 'detail', id] as const,
  settingByKey: (key: string) => [...analyticsKeys.settings(), 'key', key] as const,
}

export const useAdminDashboardStats = () => {
  return useQuery({
    queryKey: analyticsKeys.dashboardStats(),
    queryFn: dashboardApi.getStats,
    staleTime: 30000,
  })
}

export const useDashboardActivity = (limit?: number) => {
  return useQuery({
    queryKey: analyticsKeys.dashboardActivity(limit),
    queryFn: () => dashboardApi.getActivity(limit),
    staleTime: 30000,
  })
}

export const usePlatformHealth = () => {
  return useQuery({
    queryKey: analyticsKeys.dashboardHealth(),
    queryFn: dashboardApi.getHealth,
    refetchInterval: 30000,
  })
}

export const usePlatformAnalytics = (params?: AnalyticsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.platform(params),
    queryFn: () => analyticsApi.getPlatformAnalytics(params),
    staleTime: 60000,
  })
}

export const useRealTimeStats = () => {
  return useQuery({
    queryKey: analyticsKeys.realtime(),
    queryFn: analyticsApi.getRealTimeStats,
    refetchInterval: 10000,
  })
}

export const useTopPerformers = (limit?: number, period?: string) => {
  return useQuery({
    queryKey: analyticsKeys.topPerformers(limit, period),
    queryFn: () => analyticsApi.getTopPerformers(limit, period),
    staleTime: 60000,
  })
}

export const useRevenueTrends = (params?: TrendsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.revenueTrends(params),
    queryFn: () => analyticsApi.getRevenueTrends(params),
    staleTime: 60000,
  })
}

export const useAuctionTrends = (params?: TrendsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.auctionTrends(params),
    queryFn: () => analyticsApi.getAuctionTrends(params),
    staleTime: 60000,
  })
}

export const useCategoryPerformance = (startDate?: string, endDate?: string) => {
  return useQuery({
    queryKey: analyticsKeys.categories(startDate, endDate),
    queryFn: () => analyticsApi.getCategoryPerformance(startDate, endDate),
    staleTime: 60000,
  })
}

export const useAuctionMetrics = (params?: AnalyticsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.auctionMetrics(params),
    queryFn: () => analyticsApi.getAuctionMetrics(params),
    staleTime: 60000,
  })
}

export const useBidMetrics = (params?: AnalyticsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.bidMetrics(params),
    queryFn: () => analyticsApi.getBidMetrics(params),
    staleTime: 60000,
  })
}

export const useRevenueMetrics = (params?: AnalyticsQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.revenueMetrics(params),
    queryFn: () => analyticsApi.getRevenueMetrics(params),
    staleTime: 60000,
  })
}

export const useUserDashboard = () => {
  return useQuery({
    queryKey: analyticsKeys.userDashboard(),
    queryFn: userAnalyticsApi.getDashboard,
    staleTime: 30000,
  })
}

export const useSellerAnalytics = (timeRange?: string) => {
  return useQuery({
    queryKey: analyticsKeys.sellerAnalytics(timeRange),
    queryFn: () => userAnalyticsApi.getSellerAnalytics(timeRange),
    staleTime: 60000,
  })
}

export const useQuickStats = () => {
  return useQuery({
    queryKey: analyticsKeys.quickStats(),
    queryFn: userAnalyticsApi.getQuickStats,
    staleTime: 60000,
  })
}

export const useTrendingSearches = (limit?: number) => {
  return useQuery({
    queryKey: analyticsKeys.trendingSearches(limit),
    queryFn: () => userAnalyticsApi.getTrendingSearches(limit),
    staleTime: 60000,
  })
}

export const useReportList = (params?: ReportQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.reportList(params),
    queryFn: () => reportsApi.getReports(params),
    staleTime: 30000,
  })
}

export const useReportDetail = (id: string) => {
  return useQuery({
    queryKey: analyticsKeys.reportDetail(id),
    queryFn: () => reportsApi.getReport(id),
    enabled: !!id,
  })
}

export const useReportStats = () => {
  return useQuery({
    queryKey: analyticsKeys.reportStats(),
    queryFn: reportsApi.getStats,
    staleTime: 30000,
  })
}

export const useAuditLogs = (params?: AuditLogQueryParams) => {
  return useQuery({
    queryKey: analyticsKeys.auditLogList(params),
    queryFn: () => auditLogsApi.getAuditLogs(params),
    staleTime: 30000,
  })
}

export const useAuditLogDetail = (id: string) => {
  return useQuery({
    queryKey: analyticsKeys.auditLogDetail(id),
    queryFn: () => auditLogsApi.getAuditLog(id),
    enabled: !!id,
  })
}

export const useEntityAuditHistory = (entityType: string, entityId: string) => {
  return useQuery({
    queryKey: analyticsKeys.auditLogEntity(entityType, entityId),
    queryFn: () => auditLogsApi.getEntityAuditHistory(entityType, entityId),
    enabled: !!entityType && !!entityId,
  })
}

export const useSettings = (category?: SettingCategory) => {
  return useQuery({
    queryKey: analyticsKeys.settingsList(category),
    queryFn: () => settingsApi.getSettings(category),
    staleTime: 60000,
  })
}

export const useSettingDetail = (id: string) => {
  return useQuery({
    queryKey: analyticsKeys.settingDetail(id),
    queryFn: () => settingsApi.getSetting(id),
    enabled: !!id,
  })
}

export const useSettingByKey = (key: string) => {
  return useQuery({
    queryKey: analyticsKeys.settingByKey(key),
    queryFn: () => settingsApi.getSettingByKey(key),
    enabled: !!key,
  })
}
