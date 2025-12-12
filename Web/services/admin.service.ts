import apiClient from '@/lib/api/axios';

export interface AdminUser {
    id: string;
    username: string;
    email: string;
    fullName?: string;
    isActive: boolean;
    isSuspended: boolean;
    suspensionReason?: string;
    createdAt: string;
    lastLoginAt?: string;
    roles: string[];
}

export interface AdminUserListResponse {
    users: AdminUser[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface AdminStats {
    totalUsers: number;
    activeUsers: number;
    suspendedUsers: number;
    newUsersThisMonth: number;
}

export interface GetUsersParams {
    search?: string;
    role?: string;
    isActive?: boolean;
    isSuspended?: boolean;
    pageNumber?: number;
    pageSize?: number;
}

interface ApiResponse<T> {
    isSuccess: boolean;
    data: T;
    errorMessage?: string;
}

export const adminService = {
    async getUsers(params: GetUsersParams = {}): Promise<AdminUserListResponse> {
        const response = await apiClient.get<ApiResponse<AdminUserListResponse>>('/identity/api/admin/users', {
            params: {
                search: params.search || undefined,
                role: params.role && params.role !== 'all' ? params.role : undefined,
                isActive: params.isActive,
                isSuspended: params.isSuspended,
                pageNumber: params.pageNumber || 1,
                pageSize: params.pageSize || 20,
            },
        });
        return response.data.data;
    },

    async getUser(userId: string): Promise<AdminUser> {
        const response = await apiClient.get<ApiResponse<AdminUser>>(`/identity/api/admin/users/${userId}`);
        return response.data.data;
    },

    async getStats(): Promise<AdminStats> {
        const response = await apiClient.get<ApiResponse<AdminStats>>('/identity/api/admin/users/stats');
        return response.data.data;
    },

    async suspendUser(userId: string, reason: string): Promise<void> {
        await apiClient.post(`/identity/api/admin/users/${userId}/suspend`, { reason });
    },

    async activateUser(userId: string): Promise<void> {
        await apiClient.post(`/identity/api/admin/users/${userId}/activate`);
    },

    async updateUserRoles(userId: string, roles: string[]): Promise<void> {
        await apiClient.put(`/identity/api/admin/users/${userId}/roles`, { roles });
    },

    async deleteUser(userId: string): Promise<void> {
        await apiClient.delete(`/identity/api/admin/users/${userId}`);
    },
};
