import { useTranslation } from 'react-i18next'
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
  const { t } = useTranslation('common')
  return (
    <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={onClose}>
      <MenuItem onClick={onManageRoles}>
        <AdminPanelSettings sx={{ mr: 1, fontSize: 18 }} />
        {t('userManagement.manageRolesTitle')}
      </MenuItem>
      <MenuItem onClick={onManage2FA}>
        <Security sx={{ mr: 1, fontSize: 18 }} />
        {t('twoFactor.title')}
      </MenuItem>
      <Divider />
      <MenuItem onClick={onEdit}>
        <Edit sx={{ mr: 1, fontSize: 18 }} />
        {t('userManagement.editUser')}
      </MenuItem>
      <MenuItem onClick={onSendEmail}>
        <Email sx={{ mr: 1, fontSize: 18 }} />
        {t('userManagement.sendEmail')}
      </MenuItem>
      <Divider />
      {user?.isSuspended ? (
        <MenuItem onClick={onUnsuspend}>
          <CheckCircle sx={{ mr: 1, fontSize: 18, color: 'success.main' }} />
          {t('userManagement.removeSuspension')}
        </MenuItem>
      ) : (
        <MenuItem onClick={onSuspend}>
          <Block sx={{ mr: 1, fontSize: 18, color: 'warning.main' }} />
          {t('userManagement.suspendUser')}
        </MenuItem>
      )}
      {user?.isActive ? (
        <MenuItem onClick={onDeactivate}>
          <PersonOff sx={{ mr: 1, fontSize: 18, color: 'warning.main' }} />
          {t('userManagement.deactivateUser')}
        </MenuItem>
      ) : (
        <MenuItem onClick={onActivate}>
          <Person sx={{ mr: 1, fontSize: 18, color: 'success.main' }} />
          {t('userManagement.activateUser')}
        </MenuItem>
      )}
      <Divider />
      <MenuItem onClick={onDelete} sx={{ color: 'error.main' }}>
        <Delete sx={{ mr: 1, fontSize: 18 }} />
        {t('userManagement.deleteUser')}
      </MenuItem>
    </Menu>
  )
}
