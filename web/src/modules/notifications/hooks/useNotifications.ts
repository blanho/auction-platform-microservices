import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationsApi } from '../api'
import type {
  NotificationFilters,
  NotificationPreferences,
  CreateNotificationDto,
  BroadcastNotificationDto,
  AdminNotificationFilters,
} from '../types/notification.types'

export const notificationKeys = {
  all: ['notifications'] as const,
  lists: () => [...notificationKeys.all, 'list'] as const,
  list: (filters: NotificationFilters) => [...notificationKeys.lists(), filters] as const,
  summary: () => [...notificationKeys.all, 'summary'] as const,
  preferences: () => [...notificationKeys.all, 'preferences'] as const,
  unread: () => [...notificationKeys.all, 'unread'] as const,
  admin: () => [...notificationKeys.all, 'admin'] as const,
  adminList: (page: number, pageSize: number, filters?: AdminNotificationFilters) =>
    [...notificationKeys.admin(), 'list', page, pageSize, filters] as const,
  adminStats: () => [...notificationKeys.admin(), 'stats'] as const,
}

export const useNotifications = (filters: NotificationFilters) => {
  return useQuery({
    queryKey: notificationKeys.list(filters),
    queryFn: async () => {
      const allNotifications = await notificationsApi.getNotifications(filters)

      const page = filters.page || 1
      const pageSize = filters.pageSize || 20
      const startIndex = (page - 1) * pageSize
      const endIndex = startIndex + pageSize

      const items = allNotifications.slice(startIndex, endIndex)
      const totalCount = allNotifications.length
      const totalPages = Math.ceil(totalCount / pageSize)

      return {
        items,
        page,
        pageSize,
        totalCount,
        totalPages,
        hasNextPage: page < totalPages,
        hasPreviousPage: page > 1,
      }
    },
  })
}

export const useNotificationSummary = () => {
  return useQuery({
    queryKey: notificationKeys.summary(),
    queryFn: () => notificationsApi.getSummary(),
    refetchInterval: 30000,
  })
}

export const useMarkAsRead = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}

export const useMarkAllAsRead = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}

export const useArchiveNotification = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => notificationsApi.archive(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    },
  })
}

export const useDeleteNotification = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => notificationsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    },
  })
}

export const useNotificationPreferences = () => {
  return useQuery({
    queryKey: notificationKeys.preferences(),
    queryFn: () => notificationsApi.getPreferences(),
  })
}

export const useUpdateNotificationPreferences = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (preferences: Partial<NotificationPreferences>) =>
      notificationsApi.updatePreferences(preferences),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.preferences() })
    },
  })
}

export const useUnreadNotifications = () => {
  return useQuery({
    queryKey: notificationKeys.unread(),
    queryFn: () => notificationsApi.getUnreadNotifications(),
    refetchInterval: 30000,
  })
}

export const useCreateNotification = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateNotificationDto) => notificationsApi.createNotification(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}

export const useAllNotifications = (
  page: number,
  pageSize: number,
  filters?: AdminNotificationFilters
) => {
  return useQuery({
    queryKey: notificationKeys.adminList(page, pageSize, filters),
    queryFn: () => notificationsApi.getAllNotifications(page, pageSize, filters),
  })
}

export const useBroadcastNotification = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: BroadcastNotificationDto) => notificationsApi.broadcastNotification(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: notificationKeys.all })
    },
  })
}

export const useNotificationStats = () => {
  return useQuery({
    queryKey: notificationKeys.adminStats(),
    queryFn: () => notificationsApi.getNotificationStats(),
    refetchInterval: 60000,
  })
}
