import apiClient from '@/lib/api/axios';

export interface RecentActivity {
    type: string;
    description: string;
    timestamp: string;
    relatedEntityId?: string;
    relatedEntityType?: string;
}

export interface UserDashboardStats {
    totalBids: number;
    itemsWon: number;
    watchlistCount: number;
    activeListings: number;
    totalListings: number;
    totalSpent: number;
    totalEarnings: number;
    balance: number;
    sellerRating: number;
    reviewCount: number;
    recentActivity: RecentActivity[];
}

export const dashboardService = {
    async getStats(): Promise<UserDashboardStats> {
        const response = await apiClient.get<UserDashboardStats>('/auctions/dashboard/stats');
        return response.data;
    },
};
