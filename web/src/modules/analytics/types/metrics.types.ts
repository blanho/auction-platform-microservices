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

export interface MetricItem {
  label: string
  value: number
  total: number
  color: 'primary' | 'success' | 'warning' | 'error' | 'info'
}
