import type { Permission } from '@/shared/permissions'

export const USER_PERMISSIONS = {
  VIEW: 'users:view' as Permission,
  EDIT: 'users:edit' as Permission,
  DELETE: 'users:delete' as Permission,
  MANAGE: 'users:manage' as Permission,
}
