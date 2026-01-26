import { useMemo } from 'react'
import type { ReactNode } from 'react'
import { useAuth } from '../hooks/useAuth'
import { Permission, hasPermission, hasAnyPermission, hasAllPermissions, getPermissionsForRoles } from '@/shared/permissions'
import { PermissionContext } from '../context/PermissionContext'

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

  return (
    <PermissionContext.Provider value={value}>
      {children}
    </PermissionContext.Provider>
  )
}
