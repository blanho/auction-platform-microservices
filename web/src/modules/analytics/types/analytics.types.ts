import type { QueryParameters } from '@/shared/types'
import type {
  OverviewMetrics,
  AuctionMetrics,
  BidMetrics,
  RevenueMetrics,
  UserMetrics,
} from './metrics.types'

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

export interface TrendingSearch {
  query: string
  count: number
}

export interface AnalyticsQueryParams extends QueryParameters {
  startDate?: string
  endDate?: string
  period?: string
  categoryId?: string
}

export interface TrendsQueryParams extends QueryParameters {
  startDate?: string
  endDate?: string
  granularity?: 'hour' | 'day' | 'week' | 'month'
}
