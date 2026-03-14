import type { ChipProps } from '@mui/material'
import { USER_ROLES, type UserRole } from './roles'

export type RoleChipColor = ChipProps['color']

export const ROLE_COLORS: Record<UserRole, RoleChipColor> = {
  [USER_ROLES.USER]: 'default',
  [USER_ROLES.SELLER]: 'info',
  [USER_ROLES.ADMIN]: 'secondary',
  [USER_ROLES.MODERATOR]: 'warning',
}

export const STAT_COLORS = {
  TOTAL_USERS: '#7C3AED',
  ACTIVE_USERS: '#10B981',
  VERIFIED_USERS: '#3B82F6',
  SUSPENDED_USERS: '#EF4444',
} as const
