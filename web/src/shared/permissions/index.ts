export const Permission = {
  AuctionsView: 'auctions:view',
  AuctionsCreate: 'auctions:create',
  AuctionsEdit: 'auctions:edit',
  AuctionsDelete: 'auctions:delete',
  AuctionsManage: 'auctions:manage',

  BidsPlace: 'bids:place',
  BidsView: 'bids:view',
  BidsCancel: 'bids:cancel',
  BidsManage: 'bids:manage',

  UsersView: 'users:view',
  UsersEdit: 'users:edit',
  UsersDelete: 'users:delete',
  UsersManage: 'users:manage',

  PaymentsView: 'payments:view',
  PaymentsCreate: 'payments:create',
  PaymentsManage: 'payments:manage',

  NotificationsView: 'notifications:view',
  NotificationsManage: 'notifications:manage',

  ReportsView: 'reports:view',
  ReportsGenerate: 'reports:generate',

  SettingsView: 'settings:view',
  SettingsManage: 'settings:manage',

  AnalyticsView: 'analytics:view',
  AnalyticsExport: 'analytics:export',

  StorageUpload: 'storage:upload',
  StorageDelete: 'storage:delete',

  AdminAccess: 'admin:access',
  AdminManageUsers: 'admin:manage-users',
  AdminManageAuctions: 'admin:manage-auctions',
  AdminViewAnalytics: 'admin:view-analytics',
} as const

export type Permission = (typeof Permission)[keyof typeof Permission]

export const ROLE_PERMISSIONS: Record<string, Permission[]> = {
  guest: [Permission.AuctionsView],
  buyer: [
    Permission.AuctionsView,
    Permission.BidsPlace,
    Permission.BidsView,
    Permission.BidsCancel,
    Permission.UsersView,
    Permission.UsersEdit,
    Permission.PaymentsView,
    Permission.PaymentsCreate,
    Permission.NotificationsView,
    Permission.StorageUpload,
  ],
  seller: [
    Permission.AuctionsView,
    Permission.AuctionsCreate,
    Permission.AuctionsEdit,
    Permission.BidsView,
    Permission.UsersView,
    Permission.UsersEdit,
    Permission.PaymentsView,
    Permission.PaymentsCreate,
    Permission.NotificationsView,
    Permission.StorageUpload,
    Permission.AnalyticsView,
    Permission.ReportsView,
  ],
  admin: Object.values(Permission) as Permission[],
}

export const hasPermission = (
  userPermissions: Permission[],
  requiredPermission: Permission
): boolean => {
  return userPermissions.includes(requiredPermission)
}

export const hasAnyPermission = (
  userPermissions: Permission[],
  requiredPermissions: Permission[]
): boolean => {
  return requiredPermissions.some((p) => userPermissions.includes(p))
}

export const hasAllPermissions = (
  userPermissions: Permission[],
  requiredPermissions: Permission[]
): boolean => {
  return requiredPermissions.every((p) => userPermissions.includes(p))
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
