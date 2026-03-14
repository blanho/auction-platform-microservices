import {
  TableRow,
  TableCell,
  Box,
  Avatar,
  Typography,
  Chip,
  IconButton,
  Tooltip,
} from '@mui/material'
import { MoreVert, Security, CheckCircle, Block } from '@mui/icons-material'
import { formatDate } from '@/shared/utils/formatters'
import { ROLE_COLORS } from '../constants'
import { getUserStatus, getAdminUserInitial } from '../utils'
import { StatusBadge } from '@/shared/ui'
import type { AdminUser } from '../types'

interface UserTableRowProps {
  user: AdminUser
  onMenuOpen: (event: React.MouseEvent<HTMLElement>, user: AdminUser) => void
}

export function UserTableRow({ user, onMenuOpen }: UserTableRowProps) {
  const status = getUserStatus(user)

  return (
    <TableRow hover sx={{ cursor: 'pointer', transition: 'background 150ms' }}>
      <TableCell>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Avatar sx={{ bgcolor: 'primary.main' }}>{getAdminUserInitial(user)}</Avatar>
          <Box>
            <Typography variant="body2" fontWeight={500}>
              {user.displayName || user.username}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {user.email}
            </Typography>
          </Box>
        </Box>
      </TableCell>
      <TableCell>
        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
          {user.roles.map((role) => (
            <Chip
              key={role}
              label={role}
              size="small"
              color={ROLE_COLORS[role as keyof typeof ROLE_COLORS] || 'default'}
              sx={{ textTransform: 'capitalize' }}
            />
          ))}
        </Box>
      </TableCell>
      <TableCell>
        <StatusBadge status={status.label} />
      </TableCell>
      <TableCell>
        {user.twoFactorEnabled ? (
          <Tooltip title="2FA Enabled">
            <Security sx={{ color: 'success.main', fontSize: 20 }} />
          </Tooltip>
        ) : (
          <Tooltip title="2FA Disabled">
            <Security sx={{ color: 'text.disabled', fontSize: 20 }} />
          </Tooltip>
        )}
      </TableCell>
      <TableCell>
        {user.emailConfirmed ? (
          <CheckCircle sx={{ color: 'success.main', fontSize: 20 }} />
        ) : (
          <Block sx={{ color: 'text.disabled', fontSize: 20 }} />
        )}
      </TableCell>
      <TableCell>{formatDate(user.createdAt)}</TableCell>
      <TableCell>{user.lastLoginAt ? formatDate(user.lastLoginAt) : '-'}</TableCell>
      <TableCell align="right">
        <IconButton onClick={(e) => onMenuOpen(e, user)}>
          <MoreVert />
        </IconButton>
      </TableCell>
    </TableRow>
  )
}
