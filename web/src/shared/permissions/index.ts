export type Permission =
  | 'auctions:view'
  | 'auctions:create'
  | 'auctions:edit'
  | 'auctions:delete'
  | 'auctions:manage'
  | 'bids:place'
  | 'bids:view'
  | 'bids:cancel'
  | 'users:view'
  | 'users:edit'
  | 'users:delete'
  | 'users:manage'
  | 'payments:view'
  | 'payments:manage'
  | 'reports:view'
  | 'reports:generate'
  | 'settings:view'
  | 'settings:manage'
  | 'admin:access'

export const hasPermission = (
  userPermissions: Permission[],
  permission: Permission
): boolean => {
  return userPermissions.includes(permission)
}

export const hasAnyPermission = (
  userPermissions: Permission[],
  permissions: Permission[]
): boolean => {
  return permissions.some((permission) => userPermissions.includes(permission))
}

export const hasAllPermissions = (
  userPermissions: Permission[],
  permissions: Permission[]
): boolean => {
  return permissions.every((permission) => userPermissions.includes(permission))
}

export const ROLE_PERMISSIONS: Record<string, Permission[]> = {
  admin: [
    'auctions:view',
    'auctions:create',
    'auctions:edit',
    'auctions:delete',
    'auctions:manage',
    'bids:place',
    'bids:view',
    'bids:cancel',
    'users:view',
    'users:edit',
    'users:delete',
    'users:manage',
    'payments:view',
    'payments:manage',
    'reports:view',
    'reports:generate',
    'settings:view',
    'settings:manage',
    'admin:access',
  ],
  seller: [
    'auctions:view',
    'auctions:create',
    'auctions:edit',
    'bids:view',
    'users:view',
    'users:edit',
    'payments:view',
    'reports:view',
  ],
  buyer: [
    'auctions:view',
    'bids:place',
    'bids:view',
    'bids:cancel',
    'users:view',
    'users:edit',
    'payments:view',
  ],
  guest: ['auctions:view'],
}

export const getPermissionsForRoles = (roles: string[]): Permission[] => {
  const permissions = new Set<Permission>()
  roles.forEach((role) => {
    const rolePermissions = ROLE_PERMISSIONS[role.toLowerCase()]
    if (rolePermissions) {
      rolePermissions.forEach((permission) => permissions.add(permission))
    }
  })
  return Array.from(permissions)
}
