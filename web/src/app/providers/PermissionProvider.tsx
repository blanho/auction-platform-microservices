import {
  type Permission,
  getPermissionsForRoles,
  hasAllPermissions,
  hasAnyPermission,
  hasPermission,
} from '@/shared/permissions'
import type { ReactNode } from 'react'
import { useMemo } from 'react'
import { PermissionContext } from '../context/PermissionContext'
import { useAuth } from '../hooks/useAuth'

interface PermissionProviderProps {
  children: ReactNode
}

export function PermissionProvider({ children }: PermissionProviderProps) {
  const { user } = useAuth()
  const userRoles = user?.roles

  const value = useMemo(() => {
    const permissions = userRoles ? getPermissionsForRoles(userRoles) : []

    return {
      permissions,
      can: (permission: Permission) => hasPermission(permissions, permission),
      canAny: (perms: Permission[]) => hasAnyPermission(permissions, perms),
      canAll: (perms: Permission[]) => hasAllPermissions(permissions, perms),
    }
  }, [userRoles])

  return <PermissionContext.Provider value={value}>{children}</PermissionContext.Provider>
}
