import apiClient from '@/lib/api/axios';
import { API_ENDPOINTS } from '@/constants/api';

export interface AdminDashboardStats {
    totalRevenue: number;
    revenueChange: number;
    activeUsers: number;
    activeUsersChange: number;
    liveAuctions: number;
    liveAuctionsChange: number;
    pendingReports: number;
    pendingReportsChange: number;
}

export interface AdminRecentActivity {
    id: string;
    type: 'auction' | 'report' | 'payment' | 'user';
    message: string;
    timestamp: string;
    status: 'success' | 'warning' | 'info' | 'error';
    relatedEntityId?: string;
}

export interface PlatformHealth {
    apiStatus: 'healthy' | 'degraded' | 'down';
    databaseStatus: 'connected' | 'disconnected' | 'error';
    cacheStatus: 'active' | 'inactive' | 'error';
    queueJobCount: number;
    queueStatus: 'healthy' | 'warning' | 'error';
}

interface ApiResponse<T> {
    isSuccess: boolean;
    data: T;
    errorMessage?: string;
}

export const adminDashboardService = {
    async getStats(): Promise<AdminDashboardStats> {
        const response = await apiClient.get<ApiResponse<AdminDashboardStats>>(
            API_ENDPOINTS.ADMIN.DASHBOARD_STATS
        );
        return response.data.data;
    },

    async getRecentActivity(limit: number = 10): Promise<AdminRecentActivity[]> {
        const response = await apiClient.get<ApiResponse<AdminRecentActivity[]>>(
            API_ENDPOINTS.ADMIN.RECENT_ACTIVITY,
            { params: { limit } }
        );
        return response.data.data;
    },

    async getPlatformHealth(): Promise<PlatformHealth> {
        const response = await apiClient.get<ApiResponse<PlatformHealth>>(
            API_ENDPOINTS.ADMIN.PLATFORM_HEALTH
        );
        return response.data.data;
    }
};
