import type { AdminUser } from '../types'

export function getAdminUserDisplayName(user: AdminUser | null | undefined): string {
  if (!user) return ''
  return user.displayName || user.username || 'Unknown User'
}

export function getAdminUserInitial(user: AdminUser | null | undefined): string {
  if (!user) return '?'
  return user.displayName?.[0] || user.username?.[0] || '?'
}

export function getRoleFilterFromTab(tabIndex: number): string | undefined {
  const roleMap: Record<number, string | undefined> = {
    0: undefined,
    1: 'user',
    2: 'seller',
    3: 'admin',
  }
  return roleMap[tabIndex]
}
