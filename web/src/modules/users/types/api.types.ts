import type { QueryParameters } from '@/shared/types'

export interface UpdateProfileRequest {
  fullName?: string
  bio?: string
  location?: string
  phoneNumber?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export interface BecomeSellerRequest {
  acceptTerms: boolean
}

export interface UserFilters extends QueryParameters {
  search?: string
  role?: string
  isActive?: boolean
  isSuspended?: boolean
}

export interface UpdateUserRolesRequest {
  roles: string[]
}

export interface SuspendUserRequest {
  id: string
  reason: string
}
