import type { ChipProps } from '@mui/material'
import type { AdminUser } from '../types'

export type UserStatusLabel = 'Suspended' | 'Active' | 'Inactive'
export type UserStatusColor = 'error' | 'success' | 'warning'

export interface UserStatus {
  label: UserStatusLabel
  color: UserStatusColor
}

export function getUserStatus(user: AdminUser): UserStatus {
  if (user.isSuspended) {
    return { label: 'Suspended', color: 'error' }
  }
  if (user.isActive) {
    return { label: 'Active', color: 'success' }
  }
  return { label: 'Inactive', color: 'warning' }
}

export function get2FAStatusColor(enabled: boolean): ChipProps['color'] {
  return enabled ? 'success' : 'default'
}

export function getRecoveryCodesColor(hasRecoveryCodes: boolean): ChipProps['color'] {
  return hasRecoveryCodes ? 'success' : 'warning'
}
