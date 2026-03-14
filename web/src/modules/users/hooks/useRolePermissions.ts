import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
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

export const useRole = (roleId: string) => {
  return useQuery({
    queryKey: rolePermissionKeys.role(roleId),
    queryFn: () => rolePermissionsApi.getRole(roleId),
    enabled: !!roleId,
  })
}

export const useRolePermissions = (roleId: string) => {
  return useQuery({
    queryKey: rolePermissionKeys.permissions(roleId),
    queryFn: () => rolePermissionsApi.getRolePermissions(roleId),
    enabled: !!roleId,
  })
}

export const usePermissionDefinitions = () => {
  return useQuery({
    queryKey: rolePermissionKeys.definitions(),
    queryFn: () => rolePermissionsApi.getPermissionDefinitions(),
    staleTime: 1000 * 60 * 10,
  })
}

export const useGrantPermission = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roleId, permission }: { roleId: string; permission: string }) =>
      rolePermissionsApi.grantPermission(roleId, permission),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.role(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.permissions(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.roles() })
    },
  })
}

export const useRevokePermission = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roleId, permission }: { roleId: string; permission: string }) =>
      rolePermissionsApi.revokePermission(roleId, permission),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.role(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.permissions(roleId) })
      queryClient.invalidateQueries({ queryKey: rolePermissionKeys.roles() })
    },
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
