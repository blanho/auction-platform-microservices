export interface AdminDashboardStats {
  totalRevenue: number
  revenueChange: number
  activeUsers: number
  activeUsersChange: number
  liveAuctions: number
  liveAuctionsChange: number
  pendingReports: number
  pendingReportsChange: number
  totalOrders: number
  completedOrders: number
}

export interface AdminRecentActivity {
  id: string
  type: string
  message: string
  timestamp: string
  status: string
  relatedEntityId?: string
}

export interface PlatformHealth {
  apiStatus: string
  databaseStatus: string
  cacheStatus: string
  queueStatus: string
  queueJobCount: number
}

export interface PlatformAnalytics {
  overview: OverviewMetrics
  auctions: AuctionMetrics
  bids: BidMetrics
  revenue: RevenueMetrics
  users: UserMetrics
  revenueChart: TrendDataPoint[]
  auctionChart: TrendDataPoint[]
  categoryPerformance: CategoryBreakdown[]
}

export interface OverviewMetrics {
  totalRevenue: number
  revenueChangePercent: number
  totalAuctions: number
  auctionChangePercent: number
  totalBids: number
  bidChangePercent: number
  activeUsers: number
  userChangePercent: number
}

export interface AuctionMetrics {
  liveAuctions: number
  completedAuctions: number
  pendingAuctions: number
  cancelledAuctions: number
  averageAuctionDuration: number
  successRate: number
  averageFinalPrice: number
  auctionsEndingToday: number
  auctionsEndingThisWeek: number
}

export interface BidMetrics {
  totalBids: number
  bidsToday: number
  bidsThisWeek: number
  bidsThisMonth: number
  uniqueBidders: number
  averageBidAmount: number
  averageBidsPerAuction: number
  averageTimeBetweenBids: string
}

export interface RevenueMetrics {
  totalRevenue: number
  totalPlatformFees: number
  totalTransactions: number
  completedOrders: number
  pendingOrders: number
  refundedOrders: number
  averageOrderValue: number
  revenueToday: number
  revenueThisWeek: number
  revenueThisMonth: number
}

export interface UserMetrics {
  totalUsers: number
  activeUsers: number
  newUsersToday: number
  newUsersThisWeek: number
  newUsersThisMonth: number
  totalSellers: number
  totalBuyers: number
  userRetentionRate: number
}

export interface TrendDataPoint {
  date: string
  value: number
  label: string
}

export interface CategoryBreakdown {
  categoryId: string
  categoryName: string
  auctionCount: number
  bidCount: number
  revenue: number
  percentage: number
}

export interface TopPerformers {
  topSellers: TopSeller[]
  topBuyers: TopBuyer[]
  topAuctions: TopAuction[]
}

export interface TopSeller {
  sellerId: string
  username: string
  totalSales: number
  orderCount: number
  averageOrderValue: number
}

export interface TopBuyer {
  buyerId: string
  username: string
  totalSpent: number
  orderCount: number
  auctionsWon: number
}

export interface TopAuction {
  auctionId: string
  title: string
  sellerUsername: string
  finalPrice: number
  bidCount: number
  viewCount: number
}

export interface RealTimeStats {
  activeAuctions: number
  onlineUsers: number
  bidsLastHour: number
  revenueLastHour: number
  recentActivity: RecentActivity[]
}

export interface RecentActivity {
  type: string
  description: string
  timestamp: string
  metadata: Record<string, string>
}

export interface AnalyticsQueryParams {
  startDate?: string
  endDate?: string
  period?: string
  categoryId?: string
}

export interface TrendsQueryParams {
  startDate?: string
  endDate?: string
  granularity?: 'hour' | 'day' | 'week' | 'month'
}

export interface UserDashboardStats {
  activeAuctions: number
  totalAuctions: number
  wonAuctions: number
  lostAuctions: number
  totalSpent: number
  totalEarned: number
  activeBids: number
  watchingCount: number
}

export interface SellerAnalytics {
  totalAuctions: number
  activeAuctions: number
  completedAuctions: number
  cancelledAuctions: number
  totalRevenue: number
  averageFinalPrice: number
  totalBidsReceived: number
  successRate: number
  dailyRevenue: DailyRevenue[]
  categoryBreakdown: SellerCategoryBreakdown[]
}

export interface DailyRevenue {
  date: string
  revenue: number
  auctionsCompleted: number
}

export interface SellerCategoryBreakdown {
  categoryName: string
  auctionCount: number
  revenue: number
}

export interface QuickStats {
  totalAuctions: number
  totalBids: number
  totalUsers: number
  activeAuctions: number
}

export interface TrendingSearch {
  query: string
  count: number
}

export type ReportType = 'Fraud' | 'FakeItem' | 'NonPayment' | 'Harassment' | 'InappropriateContent' | 'SuspiciousActivity' | 'Other'

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

export type AuditAction = 'Created' | 'Updated' | 'Deleted' | 'StatusChanged' | 'PriceChanged' | 'BidPlaced' | 'AuctionEnded' | 'PaymentProcessed' | 'UserRegistered' | 'UserLoggedIn' | 'UserLoggedOut' | 'PermissionChanged' | 'SettingChanged'

export interface AuditLog {
  id: string
  entityId: string
  entityType: string
  action: AuditAction
  actionName: string
  oldValues?: string
  newValues?: string
  changedProperties?: string[]
  userId: string
  username?: string
  serviceName: string
  correlationId?: string
  ipAddress?: string
  timestamp: string
}

export interface AuditLogQueryParams {
  entityId?: string
  entityType?: string
  userId?: string
  serviceName?: string
  action?: AuditAction
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

export type SettingCategory = 'Platform' | 'Auction' | 'Notification' | 'Security' | 'Email'

export interface PlatformSetting {
  id: string
  key: string
  value: string
  description?: string
  category: string
  dataType?: string
  isSystem: boolean
  updatedAt: string
  updatedBy?: string
}

export interface CreateSettingRequest {
  key: string
  value: string
  description?: string
  category: SettingCategory
  dataType?: string
  validationRules?: string
}

export interface UpdateSettingRequest {
  value: string
}

export interface BulkUpdateSettingsRequest {
  settings: SettingKeyValue[]
}

export interface SettingKeyValue {
  key: string
  value: string
}
