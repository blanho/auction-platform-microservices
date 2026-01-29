import {
  Box,
  List,
  Typography,
  Skeleton,
  ListItem,
  ListItemIcon,
  ListItemText,
} from '@mui/material'
import { NotificationsOff } from '@mui/icons-material'
import type { Notification } from '../types'
import { NotificationItem } from './NotificationItem'

interface NotificationListProps {
  notifications?: Notification[]
  isLoading?: boolean
  emptyMessage?: string
  emptyDescription?: string
  onMarkAsRead?: (id: string) => void
  onDelete?: (id: string) => void
  onArchive?: (id: string) => void
}

export function NotificationList({
  notifications,
  isLoading,
  emptyMessage = 'No notifications yet',
  emptyDescription = "You'll see notifications about your bids and auctions here",
  onMarkAsRead,
  onDelete,
  onArchive,
}: NotificationListProps) {
  if (isLoading) {
    return (
      <List disablePadding>
        {[...Array(5)].map((_, i) => (
          <ListItem key={i} sx={{ py: 2, px: 3 }}>
            <ListItemIcon>
              <Skeleton variant="circular" width={40} height={40} />
            </ListItemIcon>
            <ListItemText primary={<Skeleton width="60%" />} secondary={<Skeleton width="40%" />} />
          </ListItem>
        ))}
      </List>
    )
  }

  if (!notifications || notifications.length === 0) {
    return (
      <Box sx={{ py: 8, textAlign: 'center' }}>
        <NotificationsOff sx={{ fontSize: 48, color: '#D4D4D4', mb: 2 }} />
        <Typography sx={{ color: '#78716C', mb: 1 }}>{emptyMessage}</Typography>
        {emptyDescription && (
          <Typography sx={{ fontSize: '0.875rem', color: '#A1A1AA' }}>
            {emptyDescription}
          </Typography>
        )}
      </Box>
    )
  }

  return (
    <List disablePadding>
      {notifications.map((notification, index) => (
        <Box key={notification.id}>
          <NotificationItem
            notification={notification}
            onMarkAsRead={onMarkAsRead}
            onDelete={onDelete}
            onArchive={onArchive}
          />
          {index < notifications.length - 1 && (
            <Box component="hr" sx={{ border: 'none', borderTop: '1px solid #F5F5F5', m: 0 }} />
          )}
        </Box>
      ))}
    </List>
  )
}
