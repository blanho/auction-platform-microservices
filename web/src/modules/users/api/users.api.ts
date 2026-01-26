import { http } from '@/services/http'
import type {
  UserProfile,
  SellerStatus,
  AdminUser,
  UserStats,
  User2FAStatus,
  UpdateProfileRequest,
  ChangePasswordRequest,
  UserFilters,
  UpdateUserRolesRequest,
} from '../types'
import type { PaginatedResponse } from '@/shared/types'

export const usersApi = {
  async getProfile(): Promise<UserProfile> {
    const response = await http.get<UserProfile>('/profile')
    return response.data
  },

  async updateProfile(data: UpdateProfileRequest): Promise<UserProfile> {
    const response = await http.put<UserProfile>('/profile', data)
    return response.data
  },

  async changePassword(data: ChangePasswordRequest): Promise<void> {
    await http.post('/profile/change-password', data)
  },

  async enableTwoFactor(): Promise<void> {
    await http.post('/profile/enable-2fa')
  },

  async disableTwoFactor(): Promise<void> {
    await http.post('/profile/disable-2fa')
  },

  async getSellerStatus(): Promise<SellerStatus> {
    const response = await http.get<SellerStatus>('/users/seller/status')
    return response.data
  },

  async applyForSeller(acceptTerms: boolean): Promise<SellerStatus> {
    const response = await http.post<SellerStatus>('/users/seller/apply', { acceptTerms })
    return response.data
  },

  async getUsers(filters: UserFilters): Promise<PaginatedResponse<AdminUser>> {
    const response = await http.get<PaginatedResponse<AdminUser>>('/users', { params: filters })
    return response.data
  },

  async getUserById(id: string): Promise<AdminUser> {
    const response = await http.get<AdminUser>(`/users/${id}`)
    return response.data
  },

  async suspendUser(id: string, reason: string): Promise<void> {
    await http.post(`/users/${id}/suspend`, { reason })
  },

  async unsuspendUser(id: string): Promise<void> {
    await http.post(`/users/${id}/unsuspend`)
  },

  async uploadAvatar(file: File): Promise<string> {
    const formData = new FormData()
    formData.append('file', file)
    const response = await http.post<{ url: string }>('/profile/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
    return response.data.url
  },

  async activateUser(id: string): Promise<void> {
    await http.post(`/users/${id}/activate`)
  },

  async deactivateUser(id: string): Promise<void> {
    await http.post(`/users/${id}/deactivate`)
  },

  async updateUserRoles(id: string, data: UpdateUserRolesRequest): Promise<void> {
    await http.put(`/users/${id}/roles`, data)
  },

  async deleteUser(id: string): Promise<void> {
    await http.delete(`/users/${id}`)
  },

  async getUserStats(): Promise<UserStats> {
    const response = await http.get<UserStats>('/users/stats')
    return response.data
  },

  async getUser2FAStatus(id: string): Promise<User2FAStatus> {
    const response = await http.get<User2FAStatus>(`/users/${id}/2fa/status`)
    return response.data
  },

  async resetUser2FA(id: string): Promise<void> {
    await http.post(`/users/${id}/2fa/reset`)
  },

  async disableUser2FA(id: string): Promise<void> {
    await http.post(`/users/${id}/2fa/disable`)
  },
}
