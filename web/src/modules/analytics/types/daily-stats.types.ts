export interface DailyAuctionStats {
  dateKey: string
  eventType: string
  eventCount: number
  uniqueSellers: number
  uniqueWinners: number
  totalRevenue: number | null
  avgSalePrice: number | null
  minSalePrice: number | null
  maxSalePrice: number | null
  soldCount: number
  unsoldCount: number
}

export interface DailyBidStats {
  dateKey: string
  totalBids: number
  uniqueBidders: number
  auctionsWithBids: number
  totalBidValue: number
  avgBidAmount: number | null
  minBidAmount: number | null
  maxBidAmount: number | null
}

export interface DailyRevenueStats {
  dateKey: string
  eventType: string
  transactionCount: number
  uniqueBuyers: number
  uniqueSellers: number
  totalRevenue: number
  avgTransactionAmount: number | null
  refundedAmount: number | null
  refundCount: number
}

export interface DailyStatsSummary {
  totalAuctionEvents: number
  totalAuctionRevenue: number
  totalBids: number
  totalBidValue: number
  totalTransactions: number
  totalRevenue: number
  totalRefunds: number
}

export interface AggregatedDailyStats {
  startDate: string | null
  endDate: string | null
  auctionStats: DailyAuctionStats[]
  bidStats: DailyBidStats[]
  revenueStats: DailyRevenueStats[]
  summary: DailyStatsSummary
}

export interface ChartDataPoint {
  date: string
  auctions: number
  bids: number
  revenue: number
}
