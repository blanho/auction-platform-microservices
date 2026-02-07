import { useState, useCallback } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Typography,
  Box,
  Alert,
  Snackbar,
  Button,
  Stack,
  Breadcrumbs,
  Link,
  Grid,
} from '@mui/material'
import { Shield, Refresh, NavigateNext } from '@mui/icons-material'
import { Link as RouterLink } from 'react-router-dom'
import {
  useRoles,
  usePermissionDefinitions,
  useTogglePermission,
  useSetPermissions,
} from '../hooks'
import { fadeInUp, staggerContainer, staggerItem } from '@/shared/lib/animations'
import { RoleCard, RoleCardSkeleton, PermissionsPanel, PermissionsPanelSkeleton } from '../components'
import type { RoleDto } from '../types'

export function RolePermissionsPage() {
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null)
  const [loadingPermission, setLoadingPermission] = useState<string | null>(null)
  const [snackbar, setSnackbar] = useState<{
    open: boolean
    message: string
    severity: 'success' | 'error'
  }>({ open: false, message: '', severity: 'success' })

  const { data: roles, isLoading: rolesLoading, refetch: refetchRoles } = useRoles()
  const { data: definitions = [], isLoading: definitionsLoading } = usePermissionDefinitions()
  const toggleMutation = useTogglePermission()
  const setPermissionsMutation = useSetPermissions()

  const showSnackbar = useCallback((message: string, severity: 'success' | 'error') => {
    setSnackbar({ open: true, message, severity })
  }, [])

  const handleRoleSelect = useCallback((role: RoleDto) => {
    setSelectedRole(role)
  }, [])

  const handleTogglePermission = useCallback(
    async (permission: string, enabled: boolean) => {
      if (!selectedRole) {
        return
      }

      setLoadingPermission(permission)
      try {
        await toggleMutation.mutateAsync({
          roleId: selectedRole.id,
          data: { permission, enabled },
        })

        setSelectedRole((prev) => {
          if (!prev) {
            return null
          }
          const newPermissions = enabled
            ? [...prev.permissions, permission]
            : prev.permissions.filter((p) => p !== permission)
          return { ...prev, permissions: newPermissions }
        })

        showSnackbar(
          `Permission "${permission}" ${enabled ? 'granted' : 'revoked'} for ${selectedRole.name}`,
          'success'
        )
      } catch {
        showSnackbar(`Failed to ${enabled ? 'grant' : 'revoke'} permission`, 'error')
      } finally {
        setLoadingPermission(null)
      }
    },
    [selectedRole, toggleMutation, showSnackbar]
  )

  const handleSelectAllCategory = useCallback(
    async (_category: string, permissions: string[], enabled: boolean) => {
      if (!selectedRole) {
        return
      }

      const currentPermissions = new Set(selectedRole.permissions)
      let newPermissions: string[]

      if (enabled) {
        permissions.forEach((p) => currentPermissions.add(p))
        newPermissions = Array.from(currentPermissions)
      } else {
        newPermissions = selectedRole.permissions.filter((p) => !permissions.includes(p))
      }

      try {
        await setPermissionsMutation.mutateAsync({
          roleId: selectedRole.id,
          data: { permissions: newPermissions },
        })

        setSelectedRole((prev) => (prev ? { ...prev, permissions: newPermissions } : null))
        showSnackbar(
          `All permissions in category ${enabled ? 'granted' : 'revoked'}`,
          'success'
        )
        refetchRoles()
      } catch {
        showSnackbar('Failed to update permissions', 'error')
      }
    },
    [selectedRole, setPermissionsMutation, showSnackbar, refetchRoles]
  )

  const handleRefresh = useCallback(() => {
    refetchRoles()
    if (selectedRole) {
      const updated = roles?.find((r) => r.id === selectedRole.id)
      if (updated) {
        setSelectedRole(updated)
      }
    }
  }, [refetchRoles, roles, selectedRole])

  const isLoading = rolesLoading || definitionsLoading

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Breadcrumbs separator={<NavigateNext fontSize="small" />} sx={{ mb: 3 }}>
            <Link component={RouterLink} to="/admin" color="inherit" underline="hover">
              Admin
            </Link>
            <Link component={RouterLink} to="/admin/users" color="inherit" underline="hover">
              Users
            </Link>
            <Typography color="text.primary">Role Permissions</Typography>
          </Breadcrumbs>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            justifyContent="space-between"
            alignItems={{ xs: 'flex-start', sm: 'center' }}
            spacing={2}
            mb={4}
          >
            <Box>
              <Stack direction="row" alignItems="center" spacing={1.5} mb={0.5}>
                <Shield color="primary" />
                <Typography variant="h4" fontWeight={700}>
                  Role Permissions
                </Typography>
              </Stack>
              <Typography color="text.secondary">
                Manage permissions for each role. Toggle permissions on/off to control access.
              </Typography>
            </Box>
            <Button
              variant="outlined"
              startIcon={<Refresh />}
              onClick={handleRefresh}
              disabled={isLoading}
            >
              Refresh
            </Button>
          </Stack>
        </motion.div>

        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 4 }}>
            <motion.div variants={fadeInUp}>
              <Typography variant="h6" fontWeight={600} mb={2}>
                Roles
              </Typography>
              <Stack spacing={2}>
                {isLoading
                  ? [1, 2, 3].map((i) => (
                      <motion.div key={i} variants={staggerItem}>
                        <RoleCardSkeleton />
                      </motion.div>
                    ))
                  : roles?.map((role) => (
                      <motion.div key={role.id} variants={staggerItem}>
                        <RoleCard
                          role={role}
                          selected={selectedRole?.id === role.id}
                          onClick={() => handleRoleSelect(role)}
                        />
                      </motion.div>
                    ))}
              </Stack>
            </motion.div>
          </Grid>

          <Grid size={{ xs: 12, md: 8 }}>
            <motion.div variants={fadeInUp}>
              <Typography variant="h6" fontWeight={600} mb={2}>
                Permissions
              </Typography>
              {isLoading ? (
                <PermissionsPanelSkeleton />
              ) : (
                <PermissionsPanel
                  role={selectedRole}
                  definitions={definitions}
                  loadingPermission={loadingPermission}
                  onTogglePermission={handleTogglePermission}
                  onSelectAllCategory={handleSelectAllCategory}
                />
              )}
            </motion.div>
          </Grid>
        </Grid>
      </motion.div>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Container>
  )
}
