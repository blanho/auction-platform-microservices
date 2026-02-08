import { useState, useMemo, useCallback } from 'react'
import { Link } from 'react-router-dom'
import {
  Box,
  Container,
  Typography,
  Card,
  Tabs,
  Tab,
  Chip,
  Button,
  Pagination,
  Tooltip,
  IconButton,
} from '@mui/material'
import { Settings, DoneAll } from '@mui/icons-material'
import {
  useNotifications,
  useNotificationSummary,
  useMarkAsRead,
  useMarkAllAsRead,
  useDeleteNotification,
  useArchiveNotification,
} from '../hooks'
import type { NotificationFilters } from '../types'
import { NotificationList } from '../components'
import { NOTIFICATION_CONFIG } from '../constants'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'

export function NotificationsPage() {
  const [activeTab, setActiveTab] = useState(0)
  const [filters, setFilters] = useState<NotificationFilters>({
    page: 1,
    pageSize: NOTIFICATION_CONFIG.DEFAULT_PAGE_SIZE,
  })

  const statusFilter = useMemo(() => {
    if (activeTab === 1) {return 'unread'}
    if (activeTab === 2) {return 'archived'}
    return undefined
  }, [activeTab])

  const {
    data: notifications,
    isLoading,
    error,
  } = useNotifications({
    ...filters,
    status: statusFilter,
  })
  const { data: summary } = useNotificationSummary()
  const markAsRead = useMarkAsRead()
  const markAllAsRead = useMarkAllAsRead()
  const deleteNotification = useDeleteNotification()
  const archiveNotification = useArchiveNotification()

  const handleMarkAsRead = useCallback(async (id: string) => {
    try {
      await markAsRead.mutateAsync(id)
    } catch {
      // Error handled by mutation
    }
  }, [markAsRead])

  const handleMarkAllAsRead = useCallback(async () => {
    try {
      await markAllAsRead.mutateAsync()
    } catch {
      // Error handled by mutation
    }
  }, [markAllAsRead])

  const handleDelete = useCallback(async (id: string) => {
    try {
      await deleteNotification.mutateAsync(id)
    } catch {
      // Error handled by mutation
    }
  }, [deleteNotification])

  const handleArchive = useCallback(async (id: string) => {
    try {
      await archiveNotification.mutateAsync(id)
    } catch {
      // Error handled by mutation
    }
  }, [archiveNotification])

  const getEmptyMessage = () => {
    switch (activeTab) {
      case 1:
        return 'All caught up!'
      case 2:
        return 'No archived notifications'
      default:
        return 'No notifications yet'
    }
  }

  const getEmptyDescription = () => {
    return activeTab === 0
      ? "You'll see notifications about your bids and auctions here"
      : undefined
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
        <InlineAlert severity="error">Failed to load notifications. Please try again.</InlineAlert>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: { xs: 4, md: 6 }, minHeight: '60vh' }}>
      <Box
        sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}
      >
        <Box>
          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 600,
              color: palette.neutral[900],
            }}
          >
            Notifications
          </Typography>
          <Typography sx={{ color: palette.neutral[500] }}>
            Stay updated with your auction activity
          </Typography>
        </Box>

        <Box sx={{ display: 'flex', gap: 2 }}>
          {summary && summary.unreadCount > 0 && (
            <Button
              variant="outlined"
              startIcon={<DoneAll />}
              onClick={handleMarkAllAsRead}
              disabled={markAllAsRead.isPending}
              sx={{
                borderColor: '#E5E5E5',
                color: palette.neutral[700],
                textTransform: 'none',
                '&:hover': { borderColor: palette.neutral[900] },
              }}
            >
              Mark all as read
            </Button>
          )}
          <Tooltip title="Notification Settings">
            <IconButton
              component={Link}
              to="/settings"
              sx={{
                bgcolor: palette.neutral[100],
                '&:hover': { bgcolor: '#E5E5E5' },
              }}
            >
              <Settings />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      <Card
        sx={{
          borderRadius: 2,
          boxShadow: '0 4px 20px rgba(0,0,0,0.08)',
        }}
      >
        <Box sx={{ borderBottom: '1px solid #F5F5F5' }}>
          <Tabs
            value={activeTab}
            onChange={(_, v) => {
              setActiveTab(v)
              setFilters({ ...filters, page: 1 })
            }}
            sx={{
              px: 2,
              '& .MuiTab-root': {
                textTransform: 'none',
                fontWeight: 500,
                minHeight: 56,
              },
              '& .Mui-selected': {
                color: palette.brand.primary,
              },
              '& .MuiTabs-indicator': {
                bgcolor: palette.brand.primary,
              },
            }}
          >
            <Tab
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  All
                  <Chip
                    label={summary?.totalCount || 0}
                    size="small"
                    sx={{ height: 20, fontSize: '0.75rem' }}
                  />
                </Box>
              }
            />
            <Tab
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  Unread
                  {summary && summary.unreadCount > 0 && (
                    <Chip
                      label={summary.unreadCount}
                      size="small"
                      color="error"
                      sx={{ height: 20, fontSize: '0.75rem' }}
                    />
                  )}
                </Box>
              }
            />
            <Tab label="Archived" />
          </Tabs>
        </Box>

        <NotificationList
          notifications={notifications?.items}
          isLoading={isLoading}
          emptyMessage={getEmptyMessage()}
          emptyDescription={getEmptyDescription()}
          onMarkAsRead={handleMarkAsRead}
          onDelete={handleDelete}
          onArchive={handleArchive}
        />

        {notifications && notifications.totalPages > 1 && (
          <Box
            sx={{ display: 'flex', justifyContent: 'center', p: 3, borderTop: '1px solid #F5F5F5' }}
          >
            <Pagination
              count={notifications.totalPages}
              page={filters.page || 1}
              onChange={(_, page) => setFilters({ ...filters, page })}
              color="primary"
            />
          </Box>
        )}
      </Card>
    </Container>
  )
}
