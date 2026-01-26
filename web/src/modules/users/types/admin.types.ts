export interface AdminUser {
  id: string
  email: string
  username: string
  displayName: string
  roles: string[]
  isActive: boolean
  isSuspended: boolean
  suspendReason?: string
  emailConfirmed: boolean
  twoFactorEnabled: boolean
  createdAt: string
  lastLoginAt?: string
}

export interface UserStats {
  totalUsers: number
  activeUsers: number
  suspendedUsers: number
  newUsersToday: number
  newUsersThisWeek: number
  newUsersThisMonth: number
  verifiedUsers: number
  sellersCount: number
  twoFactorEnabledCount: number
}

export interface User2FAStatus {
  userId: string
  twoFactorEnabled: boolean
  hasRecoveryCodes: boolean
  lastVerifiedAt?: string
}

export type UserActionDialog = 'suspend' | 'delete' | 'roles' | '2fa' | 'activate' | 'deactivate' | null
