import { useMemo } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Stack,
  Chip,
  Skeleton,
  IconButton,
  Tooltip,
} from '@mui/material'
import { ExpandMore, CheckCircle, RadioButtonUnchecked, SelectAll, DeselectOutlined } from '@mui/icons-material'
import { PermissionToggle } from './PermissionToggle'
import type { PermissionDefinition, RoleDto, PermissionCategory } from '../../types'

interface PermissionsPanelProps {
  readonly role: RoleDto | null
  readonly definitions: readonly PermissionDefinition[]
  readonly loadingPermission: string | null
  readonly onTogglePermission: (permission: string, enabled: boolean) => void
  readonly onSelectAllCategory: (category: string, permissions: string[], enabled: boolean) => void
}

function groupByCategory(definitions: readonly PermissionDefinition[]): PermissionCategory[] {
  const categoryMap = new Map<string, PermissionDefinition[]>()

  definitions.forEach((def) => {
    const existing = categoryMap.get(def.category) || []
    existing.push(def)
    categoryMap.set(def.category, existing)
  })

  return Array.from(categoryMap.entries())
    .map(([name, permissions]) => ({ name, permissions }))
    .sort((a, b) => a.name.localeCompare(b.name))
}

const CATEGORY_ICONS: Record<string, string> = {
  Auctions: '🏷️',
  Bids: '💰',
  Users: '👥',
  Orders: '📦',
  Payments: '💳',
  Wallet: '👛',
  Analytics: '📊',
  Storage: '📁',
  Notifications: '🔔',
  Reviews: '⭐',
  Reports: '📋',
  Categories: '🗂️',
  Brands: '🏢',
  Audit: '📜',
}

export function PermissionsPanel({
  role,
  definitions,
  loadingPermission,
  onTogglePermission,
  onSelectAllCategory,
}: PermissionsPanelProps) {
  const categories = useMemo(() => groupByCategory(definitions), [definitions])
  const enabledPermissions = useMemo(() => new Set(role?.permissions || []), [role?.permissions])

  if (!role) {
    return (
      <Card sx={{ height: '100%' }}>
        <CardContent
          sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            height: 400,
          }}
        >
          <Typography color="text.secondary">Select a role to manage permissions</Typography>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Stack direction="row" alignItems="center" justifyContent="space-between" mb={2}>
          <Box>
            <Typography variant="h6" fontWeight={600}>
              Permissions for {role.name}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {enabledPermissions.size} of {definitions.length} permissions enabled
            </Typography>
          </Box>
          <Chip
            label={`${Math.round((enabledPermissions.size / definitions.length) * 100)}%`}
            color={enabledPermissions.size === definitions.length ? 'success' : 'default'}
            size="small"
          />
        </Stack>

        <Box sx={{ maxHeight: 600, overflow: 'auto' }}>
          {categories.map((category) => {
            const categoryPermissions = category.permissions.map((p) => p.code)
            const enabledCount = categoryPermissions.filter((code) =>
              enabledPermissions.has(code)
            ).length
            const allEnabled = enabledCount === categoryPermissions.length

            return (
              <Accordion key={category.name} defaultExpanded={false}>
                <AccordionSummary expandIcon={<ExpandMore />}>
                  <Stack
                    direction="row"
                    alignItems="center"
                    justifyContent="space-between"
                    sx={{ width: '100%', pr: 2 }}
                  >
                    <Stack direction="row" alignItems="center" spacing={1.5}>
                      <Typography fontSize={20}>
                        {CATEGORY_ICONS[category.name] || '⚙️'}
                      </Typography>
                      <Typography fontWeight={500}>{category.name}</Typography>
                    </Stack>
                    <Stack direction="row" alignItems="center" spacing={1}>
                      <Chip
                        icon={
                          allEnabled ? (
                            <CheckCircle sx={{ fontSize: 16 }} />
                          ) : (
                            <RadioButtonUnchecked sx={{ fontSize: 16 }} />
                          )
                        }
                        label={`${enabledCount}/${categoryPermissions.length}`}
                        size="small"
                        color={allEnabled ? 'success' : 'default'}
                        variant={allEnabled ? 'filled' : 'outlined'}
                      />
                      <Tooltip title={allEnabled ? 'Revoke all' : 'Grant all'}>
                        <IconButton
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation()
                            onSelectAllCategory(category.name, categoryPermissions, !allEnabled)
                          }}
                          sx={{ ml: 1 }}
                        >
                          {allEnabled ? (
                            <DeselectOutlined fontSize="small" />
                          ) : (
                            <SelectAll fontSize="small" />
                          )}
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </Stack>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <Stack spacing={0.5} sx={{ p: 1 }}>
                    {category.permissions.map((permission) => (
                      <PermissionToggle
                        key={permission.code}
                        permission={permission}
                        enabled={enabledPermissions.has(permission.code)}
                        loading={loadingPermission === permission.code}
                        onToggle={onTogglePermission}
                      />
                    ))}
                  </Stack>
                </AccordionDetails>
              </Accordion>
            )
          })}
        </Box>
      </CardContent>
    </Card>
  )
}

export function PermissionsPanelSkeleton() {
  return (
    <Card sx={{ height: '100%' }}>
      <CardContent>
        <Skeleton variant="text" width="50%" height={32} sx={{ mb: 1 }} />
        <Skeleton variant="text" width="30%" height={20} sx={{ mb: 3 }} />
        <Stack spacing={1}>
          {[1, 2, 3, 4, 5].map((i) => (
            <Skeleton key={i} variant="rounded" height={56} />
          ))}
        </Stack>
      </CardContent>
    </Card>
  )
}
