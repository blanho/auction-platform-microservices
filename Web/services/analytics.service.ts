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
};
