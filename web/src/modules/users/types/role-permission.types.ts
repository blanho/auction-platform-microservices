export interface PermissionDefinition {
  code: string
  name: string
  description: string
  category: string
}

export interface RoleDto {
  id: string
  name: string
  description: string | null
  isSystemRole: boolean
  permissions: string[]
  createdAt: string
  updatedAt: string | null
}

export interface SetPermissionsRequest {
  permissions: string[]
}

export interface TogglePermissionRequest {
  permission: string
  enabled: boolean
}

export interface PermissionCategory {
  name: string
  permissions: PermissionDefinition[]
}
