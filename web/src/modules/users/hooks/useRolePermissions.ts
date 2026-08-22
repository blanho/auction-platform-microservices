import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { rolePermissionsApi } from '../api'
import type { SetPermissionsRequest, TogglePermissionRequest } from '../types'

export const rolePermissionKeys = {
  all: ['role-permissions'] as const,
  roles: () => [...rolePermissionKeys.all, 'roles'] as const,
  role: (id: string) => [...rolePermissionKeys.all, 'role', id] as const,
  permissions: (roleId: string) => [...rolePermissionKeys.all, 'permissions', roleId] as const,
  definitions: () => [...rolePermissionKeys.all, 'definitions'] as const,
}

export const useRoles = () => {
  return useQuery({
    queryKey: rolePermissionKeys.roles(),
    queryFn: () => rolePermissionsApi.getRoles(),
  })
}

export const usePermissionDefinitions = () => {
  return useQuery({
    queryKey: rolePermissionKeys.definitions(),
    queryFn: () => rolePermissionsApi.getPermissionDefinitions(),
    staleTime: 1000 * 60 * 10,
  })
}

export const useSetPermissions = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roleId, data }: { roleId: string; data: SetPermissionsRequest }) =>
      rolePermissionsApi.setPermissions(roleId, data),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.role(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.permissions(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.roles() })
    },
  })
}

export const useTogglePermission = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roleId, data }: { roleId: string; data: TogglePermissionRequest }) =>
      rolePermissionsApi.togglePermission(roleId, data),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.role(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.permissions(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.roles() })
    },
  })
}
