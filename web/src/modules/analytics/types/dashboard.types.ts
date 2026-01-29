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

export interface QuickStats {
  totalAuctions: number
  totalBids: number
  totalUsers: number
  activeAuctions: number
}
