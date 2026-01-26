import { Menu, MenuItem, Divider } from '@mui/material'
import {
  AdminPanelSettings,
  Security,
  Edit,
  Email,
  CheckCircle,
  Block,
  PersonOff,
  Person,
  Delete,
} from '@mui/icons-material'
import type { AdminUser } from '../types'

interface UserActionsMenuProps {
  anchorEl: HTMLElement | null
  user: AdminUser | null
  onClose: () => void
  onManageRoles: () => void
  onManage2FA: () => void
  onEdit: () => void
  onSendEmail: () => void
  onSuspend: () => void
  onUnsuspend: () => void
  onActivate: () => void
  onDeactivate: () => void
  onDelete: () => void
}

export function UserActionsMenu({
  anchorEl,
  user,
  onClose,
  onManageRoles,
  onManage2FA,
  onEdit,
  onSendEmail,
  onSuspend,
  onUnsuspend,
  onActivate,
  onDeactivate,
  onDelete,
}: UserActionsMenuProps) {
  return (
    <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={onClose}>
      <MenuItem onClick={onManageRoles}>
        <AdminPanelSettings sx={{ mr: 1, fontSize: 18 }} />
        Manage Roles
      </MenuItem>
      <MenuItem onClick={onManage2FA}>
        <Security sx={{ mr: 1, fontSize: 18 }} />
        2FA Settings
      </MenuItem>
      <Divider />
      <MenuItem onClick={onEdit}>
        <Edit sx={{ mr: 1, fontSize: 18 }} />
        Edit User
      </MenuItem>
      <MenuItem onClick={onSendEmail}>
        <Email sx={{ mr: 1, fontSize: 18 }} />
        Send Email
      </MenuItem>
      <Divider />
      {user?.isSuspended ? (
        <MenuItem onClick={onUnsuspend}>
          <CheckCircle sx={{ mr: 1, fontSize: 18, color: 'success.main' }} />
          Remove Suspension
        </MenuItem>
      ) : (
        <MenuItem onClick={onSuspend}>
          <Block sx={{ mr: 1, fontSize: 18, color: 'warning.main' }} />
          Suspend User
        </MenuItem>
      )}
      {user?.isActive ? (
        <MenuItem onClick={onDeactivate}>
          <PersonOff sx={{ mr: 1, fontSize: 18, color: 'warning.main' }} />
          Deactivate User
        </MenuItem>
      ) : (
        <MenuItem onClick={onActivate}>
          <Person sx={{ mr: 1, fontSize: 18, color: 'success.main' }} />
          Activate User
        </MenuItem>
      )}
      <Divider />
      <MenuItem onClick={onDelete} sx={{ color: 'error.main' }}>
        <Delete sx={{ mr: 1, fontSize: 18 }} />
        Delete User
      </MenuItem>
    </Menu>
  )
}
