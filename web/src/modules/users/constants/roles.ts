export const USER_ROLES = {
  USER: 'user',
  SELLER: 'seller',
  ADMIN: 'admin',
  MODERATOR: 'moderator',
} as const

export type UserRole = (typeof USER_ROLES)[keyof typeof USER_ROLES]

export const AVAILABLE_ROLES: UserRole[] = [
  USER_ROLES.USER,
  USER_ROLES.SELLER,
  USER_ROLES.ADMIN,
  USER_ROLES.MODERATOR,
]

export const ROLE_DESCRIPTIONS: Record<UserRole, string> = {
  [USER_ROLES.USER]: 'Standard user',
  [USER_ROLES.SELLER]: 'Can create auctions',
  [USER_ROLES.ADMIN]: 'Full access',
  [USER_ROLES.MODERATOR]: 'Can moderate content',
}
