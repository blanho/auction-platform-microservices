import { createContext } from 'react'
import type { Permission } from '@/shared/permissions'

export interface PermissionContextType {
  permissions: Permission[]
  can: (permission: Permission) => boolean
  canAny: (permissions: Permission[]) => boolean
  canAll: (permissions: Permission[]) => boolean
}

export const PermissionContext = createContext<PermissionContextType | undefined>(undefined)
