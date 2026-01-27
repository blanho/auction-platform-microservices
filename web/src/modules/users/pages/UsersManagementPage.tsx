import { useState, useMemo, useCallback } from 'react'
import { motion } from 'framer-motion'
import {
  Container,
  Card,
  Typography,
  Box,
  TextField,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Button,
  Tabs,
  Tab,
} from '@mui/material'
import { Search, PersonAdd, Refresh } from '@mui/icons-material'
import { useQueryClient } from '@tanstack/react-query'
import {
  useUsers,
  useSuspendUser,
  useUnsuspendUser,
  useActivateUser,
  useDeactivateUser,
  useUpdateUserRoles,
  useDeleteUser,
  useUserStats,
  useUser2FAStatus,
  useResetUser2FA,
  useDisableUser2FA,
} from '@/modules/users/hooks'
import { fadeInUp, staggerContainer } from '@/shared/lib/animations'
import { getRoleFilterFromTab } from '../utils'
import {
  UserStatsGrid,
  UserTableRow,
  UserTableSkeleton,
  UserTableEmptyState,
  UserActionsMenu,
  SuspendUserDialog,
  ActivateUserDialog,
  DeactivateUserDialog,
  DeleteUserDialog,
  ManageRolesDialog,
  TwoFactorDialog,
} from '../components'
import type { AdminUser, UserFilters, UserActionDialog } from '../types'

export function UsersManagementPage() {
  const queryClient = useQueryClient()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [tabValue, setTabValue] = useState(0)
  const [menuAnchor, setMenuAnchor] = useState<null | HTMLElement>(null)
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null)
  const [actionDialog, setActionDialog] = useState<UserActionDialog>(null)
  const [suspendReason, setSuspendReason] = useState('')
  const [selectedRoles, setSelectedRoles] = useState<string[]>([])

  const filters: UserFilters = useMemo(
    () => ({
      page: page + 1,
      pageSize: rowsPerPage,
      search: search || undefined,
      role: getRoleFilterFromTab(tabValue),
    }),
    [page, rowsPerPage, search, tabValue]
  )

  const { data: usersData, isLoading: usersLoading, refetch } = useUsers(filters)
  const { data: stats, isLoading: statsLoading } = useUserStats()
  const { data: user2FAStatus, isLoading: twoFALoading } = useUser2FAStatus(
    actionDialog === '2fa' && selectedUser ? selectedUser.id : ''
  )

  const suspendMutation = useSuspendUser()
  const unsuspendMutation = useUnsuspendUser()
  const activateMutation = useActivateUser()
  const deactivateMutation = useDeactivateUser()
  const updateRolesMutation = useUpdateUserRoles()
  const deleteMutation = useDeleteUser()
  const resetUser2FAMutation = useResetUser2FA()
  const disableUser2FAMutation = useDisableUser2FA()

  const invalidateUsers = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['users'] })
  }, [queryClient])

  const closeDialogAndReset = useCallback(() => {
    setActionDialog(null)
    setSelectedUser(null)
    setSuspendReason('')
  }, [])

  const handleMenuOpen = useCallback((event: React.MouseEvent<HTMLElement>, user: AdminUser) => {
    setMenuAnchor(event.currentTarget)
    setSelectedUser(user)
  }, [])

  const handleMenuClose = useCallback(() => {
    setMenuAnchor(null)
  }, [])

  const handleOpenRolesDialog = useCallback(() => {
    if (selectedUser) {
      setSelectedRoles(selectedUser.roles)
      setActionDialog('roles')
    }
    handleMenuClose()
  }, [selectedUser, handleMenuClose])

  const handleOpen2FADialog = useCallback(() => {
    setActionDialog('2fa')
    handleMenuClose()
  }, [handleMenuClose])

  const handleSuspend = useCallback(() => {
    if (!selectedUser) return
    suspendMutation.mutate(
      { id: selectedUser.id, reason: suspendReason || 'Suspended by admin' },
      {
        onSuccess: () => {
          invalidateUsers()
          closeDialogAndReset()
        },
      }
    )
  }, [selectedUser, suspendReason, suspendMutation, invalidateUsers, closeDialogAndReset])

  const handleUnsuspend = useCallback(() => {
    if (!selectedUser) return
    unsuspendMutation.mutate(selectedUser.id, {
      onSuccess: () => {
        invalidateUsers()
        handleMenuClose()
      },
    })
  }, [selectedUser, unsuspendMutation, invalidateUsers, handleMenuClose])

  const handleActivate = useCallback(() => {
    if (!selectedUser) return
    activateMutation.mutate(selectedUser.id, {
      onSuccess: () => {
        invalidateUsers()
        closeDialogAndReset()
      },
    })
  }, [selectedUser, activateMutation, invalidateUsers, closeDialogAndReset])

  const handleDeactivate = useCallback(() => {
    if (!selectedUser) return
    deactivateMutation.mutate(selectedUser.id, {
      onSuccess: () => {
        invalidateUsers()
        closeDialogAndReset()
      },
    })
  }, [selectedUser, deactivateMutation, invalidateUsers, closeDialogAndReset])

  const handleUpdateRoles = useCallback(() => {
    if (!selectedUser) return
    updateRolesMutation.mutate(
      { id: selectedUser.id, data: { roles: selectedRoles } },
      {
        onSuccess: () => {
          invalidateUsers()
          closeDialogAndReset()
        },
      }
    )
  }, [selectedUser, selectedRoles, updateRolesMutation, invalidateUsers, closeDialogAndReset])

  const handleDelete = useCallback(() => {
    if (!selectedUser) return
    deleteMutation.mutate(selectedUser.id, {
      onSuccess: () => {
        invalidateUsers()
        closeDialogAndReset()
      },
    })
  }, [selectedUser, deleteMutation, invalidateUsers, closeDialogAndReset])

  const handleReset2FA = useCallback(() => {
    if (!selectedUser) return
    resetUser2FAMutation.mutate(selectedUser.id, {
      onSuccess: invalidateUsers,
    })
  }, [selectedUser, resetUser2FAMutation, invalidateUsers])

  const handleDisable2FA = useCallback(() => {
    if (!selectedUser) return
    disableUser2FAMutation.mutate(selectedUser.id, {
      onSuccess: () => {
        invalidateUsers()
        closeDialogAndReset()
      },
    })
  }, [selectedUser, disableUser2FAMutation, invalidateUsers, closeDialogAndReset])

  const handleRoleToggle = useCallback((role: string) => {
    setSelectedRoles((prev) =>
      prev.includes(role) ? prev.filter((r) => r !== role) : [...prev, role]
    )
  }, [])

  const handleTabChange = useCallback((_: React.SyntheticEvent, value: number) => {
    setTabValue(value)
    setPage(0)
  }, [])

  const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(0)
  }, [])

  const handlePageChange = useCallback((_: unknown, newPage: number) => {
    setPage(newPage)
  }, [])

  const handleRowsPerPageChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(e.target.value))
    setPage(0)
  }, [])

  return (
    <Container maxWidth="xl" sx={{ py: 4 }}>
      <motion.div variants={staggerContainer} initial="initial" animate="animate">
        <motion.div variants={fadeInUp}>
          <Box
            sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}
          >
            <Box>
              <Typography
                variant="h4"
                sx={{
                  fontFamily: '"Fira Sans", sans-serif',
                  fontWeight: 700,
                  color: 'text.primary',
                }}
              >
                User Management
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                Manage user accounts, roles, and security settings
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Button variant="outlined" startIcon={<Refresh />} onClick={() => refetch()}>
                Refresh
              </Button>
              <Button
                variant="contained"
                startIcon={<PersonAdd />}
                sx={{
                  bgcolor: 'primary.main',
                  '&:hover': { bgcolor: 'primary.dark' },
                }}
              >
                Add User
              </Button>
            </Box>
          </Box>
        </motion.div>

        <motion.div variants={fadeInUp}>
          <UserStatsGrid stats={stats} loading={statsLoading} />
        </motion.div>

        <motion.div variants={fadeInUp}>
          <Card sx={{ mb: 3 }}>
            <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
              <Tabs value={tabValue} onChange={handleTabChange}>
                <Tab label="All Users" />
                <Tab label="Buyers" />
                <Tab label="Sellers" />
                <Tab label="Admins" />
              </Tabs>
            </Box>

            <Box sx={{ p: 2 }}>
              <TextField
                placeholder="Search users by name or email..."
                size="small"
                value={search}
                onChange={handleSearchChange}
                slotProps={{
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <Search />
                      </InputAdornment>
                    ),
                  },
                }}
                sx={{ width: 350 }}
              />
            </Box>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>User</TableCell>
                    <TableCell>Roles</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>2FA</TableCell>
                    <TableCell>Email Verified</TableCell>
                    <TableCell>Joined</TableCell>
                    <TableCell>Last Login</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {usersLoading && <UserTableSkeleton />}
                  {!usersLoading &&
                    usersData?.items.map((user) => (
                      <UserTableRow key={user.id} user={user} onMenuOpen={handleMenuOpen} />
                    ))}
                  {!usersLoading && usersData?.items.length === 0 && <UserTableEmptyState />}
                </TableBody>
              </Table>
            </TableContainer>

            <TablePagination
              component="div"
              count={usersData?.totalCount || 0}
              page={page}
              onPageChange={handlePageChange}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={handleRowsPerPageChange}
            />
          </Card>
        </motion.div>
      </motion.div>

      <UserActionsMenu
        anchorEl={menuAnchor}
        user={selectedUser}
        onClose={handleMenuClose}
        onManageRoles={handleOpenRolesDialog}
        onManage2FA={handleOpen2FADialog}
        onEdit={handleMenuClose}
        onSendEmail={handleMenuClose}
        onSuspend={() => {
          handleMenuClose()
          setActionDialog('suspend')
        }}
        onUnsuspend={handleUnsuspend}
        onActivate={() => {
          handleMenuClose()
          setActionDialog('activate')
        }}
        onDeactivate={() => {
          handleMenuClose()
          setActionDialog('deactivate')
        }}
        onDelete={() => {
          handleMenuClose()
          setActionDialog('delete')
        }}
      />

      <SuspendUserDialog
        open={actionDialog === 'suspend'}
        user={selectedUser}
        reason={suspendReason}
        loading={suspendMutation.isPending}
        onClose={closeDialogAndReset}
        onReasonChange={setSuspendReason}
        onConfirm={handleSuspend}
      />

      <ActivateUserDialog
        open={actionDialog === 'activate'}
        user={selectedUser}
        loading={activateMutation.isPending}
        onClose={closeDialogAndReset}
        onConfirm={handleActivate}
      />

      <DeactivateUserDialog
        open={actionDialog === 'deactivate'}
        user={selectedUser}
        loading={deactivateMutation.isPending}
        onClose={closeDialogAndReset}
        onConfirm={handleDeactivate}
      />

      <DeleteUserDialog
        open={actionDialog === 'delete'}
        user={selectedUser}
        loading={deleteMutation.isPending}
        onClose={closeDialogAndReset}
        onConfirm={handleDelete}
      />

      <ManageRolesDialog
        open={actionDialog === 'roles'}
        user={selectedUser}
        selectedRoles={selectedRoles}
        loading={updateRolesMutation.isPending}
        onClose={closeDialogAndReset}
        onRoleToggle={handleRoleToggle}
        onConfirm={handleUpdateRoles}
      />

      <TwoFactorDialog
        open={actionDialog === '2fa'}
        user={selectedUser}
        status={user2FAStatus}
        loading={twoFALoading}
        resetLoading={resetUser2FAMutation.isPending}
        disableLoading={disableUser2FAMutation.isPending}
        onClose={closeDialogAndReset}
        onReset={handleReset2FA}
        onDisable={handleDisable2FA}
      />
    </Container>
  )
}
