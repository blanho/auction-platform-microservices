import { Link } from 'react-router-dom'
import {
  Box,
  ListItem,
  ListItemIcon,
  ListItemText,
  Typography,
  IconButton,
  Menu,
  MenuItem,
} from '@mui/material'
import { MoreVert, Delete, Archive, Circle } from '@mui/icons-material'
import { useState } from 'react'
import type { Notification } from '../types'
import { formatTimeAgo, getNotificationLink } from '../utils'
import { NotificationIcon } from './NotificationIcon'
import { palette } from '@/shared/theme/tokens'

interface NotificationItemProps {
  notification: Notification
  onMarkAsRead?: (id: string) => void
  onDelete?: (id: string) => void
  onArchive?: (id: string) => void
}

export function NotificationItem({
  notification,
  onMarkAsRead,
  onDelete,
  onArchive,
}: NotificationItemProps) {
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)
  const link = getNotificationLink(notification)
  const isUnread = notification.status === 'unread'

  const handleClick = () => {
    if (isUnread && onMarkAsRead) {
      onMarkAsRead(notification.id)
    }
  }

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    event.stopPropagation()
    setAnchorEl(event.currentTarget)
  }

  const handleMenuClose = () => {
    setAnchorEl(null)
  }

  const handleDelete = () => {
    if (onDelete) {onDelete(notification.id)}
    handleMenuClose()
  }

  const handleArchive = () => {
    if (onArchive) {onArchive(notification.id)}
    handleMenuClose()
  }

  return (
    <>
      <ListItem
        sx={{
          py: 2,
          px: 3,
          bgcolor: isUnread ? '#FFFBEB' : 'transparent',
          '&:hover': { bgcolor: isUnread ? '#FEF3C7' : palette.neutral[50] },
          cursor: link ? 'pointer' : 'default',
        }}
        onClick={handleClick}
        secondaryAction={
          <IconButton
            onClick={handleMenuOpen}
            sx={{
              opacity: 0,
              transition: 'opacity 0.2s',
              '.MuiListItem-root:hover &': { opacity: 1 },
            }}
          >
            <MoreVert />
          </IconButton>
        }
      >
        <ListItemIcon sx={{ minWidth: 52 }}>
          <NotificationIcon type={notification.type} size={24} showBackground />
        </ListItemIcon>
        <ListItemText
          primary={
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              {link ? (
                <Typography
                  component={Link}
                  to={link}
                  sx={{
                    fontWeight: isUnread ? 600 : 500,
                    color: palette.neutral[900],
                    textDecoration: 'none',
                    '&:hover': { color: palette.brand.primary },
                  }}
                >
                  {notification.title}
                </Typography>
              ) : (
                <Typography
                  sx={{
                    fontWeight: isUnread ? 600 : 500,
                    color: palette.neutral[900],
                  }}
                >
                  {notification.title}
                </Typography>
              )}
              {isUnread && <Circle sx={{ fontSize: 8, color: palette.brand.primary }} />}
            </Box>
          }
          secondary={
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
              <Typography
                component="span"
                sx={{ fontSize: '0.875rem', color: palette.neutral[500] }}
              >
                {notification.message}
              </Typography>
              <Typography component="span" sx={{ fontSize: '0.75rem', color: '#A1A1AA' }}>
                {formatTimeAgo(notification.createdAt)}
              </Typography>
            </Box>
          }
        />
      </ListItem>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <MenuItem onClick={handleArchive}>
          <Archive sx={{ mr: 1, fontSize: 20 }} />
          Archive
        </MenuItem>
        <MenuItem onClick={handleDelete} sx={{ color: palette.semantic.error }}>
          <Delete sx={{ mr: 1, fontSize: 20 }} />
          Delete
        </MenuItem>
      </Menu>
    </>
  )
}
