import apiClient from "@/lib/api/axios";

export interface TopListing {
    id: string;
    title: string;
    currentBid: number;
    views: number;
    bids: number;
}

export interface ChartDataPoint {
    date: string;
    revenue: number;
    bids?: number;
}

export interface SellerAnalyticsData {
    totalRevenue: number;
    revenueChange: number;
    itemsSold: number;
    itemsChange: number;
    activeListings: number;
    viewsToday: number;
    viewsChange: number;
    topListings: TopListing[];
    chartData: ChartDataPoint[];
}

export interface QuickStats {
    liveAuctions: number;
    liveAuctionsChange?: string;
    activeUsers: number;
    activeUsersChange?: string;
    endingSoon: number;
    endingSoonChange?: string;
}

export interface TrendingSearch {
    searchTerm: string;
    icon?: string;
    trending?: boolean;
    hot?: boolean;
    isNew?: boolean;
    count?: number;
}

export const analyticsService = {
    async getSellerAnalytics(timeRange: string = "30d"): Promise<SellerAnalyticsData> {
        const response = await apiClient.get<SellerAnalyticsData>("/auctions/analytics/seller", {
            params: { timeRange },
        });
        return response.data;
    },

    async getTopListings(limit: number = 5): Promise<TopListing[]> {
        const response = await apiClient.get<TopListing[]>("/auctions/analytics/top-listings", {
            params: { limit },
        });
        return response.data;
    },

    async getQuickStats(): Promise<QuickStats> {
        try {
            const response = await apiClient.get<QuickStats>("/auctions/analytics/quick-stats");
            return response.data;
        } catch (error) {
            return {
                liveAuctions: 0,
                activeUsers: 0,
                endingSoon: 0,
            };
        }
    },

    async getTrendingSearches(limit: number = 6): Promise<TrendingSearch[]> {
        try {
            const response = await apiClient.get<TrendingSearch[]>("/auctions/analytics/trending-searches", {
                params: { limit },
            });
            return response.data;
        } catch (error) {
            return [];
        }
    },
};
