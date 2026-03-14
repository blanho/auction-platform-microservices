import { http } from '@/services/http'
import type {
  RoleDto,
  PermissionDefinition,
  SetPermissionsRequest,
  TogglePermissionRequest,
} from '../types'

export const rolePermissionsApi = {
  async getRoles(): Promise<RoleDto[]> {
    const response = await http.get<RoleDto[]>('/roles')
    return response.data
  },

  async getRole(roleId: string): Promise<RoleDto> {
    const response = await http.get<RoleDto>(`/roles/${roleId}`)
    return response.data
  },

  async getRolePermissions(roleId: string): Promise<string[]> {
    const response = await http.get<string[]>(`/roles/${roleId}/permissions`)
    return response.data
  },

  async getPermissionDefinitions(): Promise<PermissionDefinition[]> {
    const response = await http.get<PermissionDefinition[]>('/roles/permissions/definitions')
    return response.data
  },

  async grantPermission(roleId: string, permission: string): Promise<void> {
    await http.post(`/roles/${roleId}/permissions/${encodeURIComponent(permission)}`)
  },

  async revokePermission(roleId: string, permission: string): Promise<void> {
    await http.delete(`/roles/${roleId}/permissions/${encodeURIComponent(permission)}`)
  },

  async setPermissions(roleId: string, data: SetPermissionsRequest): Promise<void> {
    await http.put(`/roles/${roleId}/permissions`, data)
  },

  async togglePermission(roleId: string, data: TogglePermissionRequest): Promise<void> {
    await http.post(`/roles/${roleId}/permissions/toggle`, data)
  },
}
