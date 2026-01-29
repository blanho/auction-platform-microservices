import { useEffect, useRef, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { signalRService } from '@/services/signalr'
import type { NotificationPayload } from '@/services/signalr'
import { notificationKeys } from '../hooks/useNotifications'
import { useAuth } from '@/app/providers'
import { signalRLogger } from '@/shared/lib/logger'

export const useSignalRNotifications = () => {
  const queryClient = useQueryClient()
  const { isAuthenticated } = useAuth()
  const hasConnected = useRef(false)
  const isMountedRef = useRef(true)

  const handleNotification = useCallback(
    (notification: NotificationPayload) => {
      if (!isMountedRef.current) {return}
      signalRLogger.info('📬 New notification received:', notification.id)
      queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    },
    [queryClient]
  )

  const handleNotificationRead = useCallback(
    (notificationId: string) => {
      if (!isMountedRef.current) {return}
      signalRLogger.info('✅ Notification marked as read:', notificationId)
      queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
      queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    },
    [queryClient]
  )

  const handleAllNotificationsRead = useCallback(() => {
    if (!isMountedRef.current) {return}
    signalRLogger.info('✅ All notifications marked as read')
    queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
    queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
  }, [queryClient])

  useEffect(() => {
    isMountedRef.current = true

    if (!isAuthenticated) {
      if (hasConnected.current) {
        signalRService.disconnect()
        hasConnected.current = false
      }
      return
    }

    if (hasConnected.current) {return}

    const connectAndListen = async () => {
      try {
        await signalRService.connect()

        if (!isMountedRef.current) {
          signalRService.disconnect()
          return
        }

        hasConnected.current = true

        signalRService.on('ReceiveNotification', handleNotification)
        signalRService.on('NotificationRead', handleNotificationRead)
        signalRService.on('AllNotificationsRead', handleAllNotificationsRead)
      } catch (error) {
        signalRLogger.error('Failed to connect to SignalR:', error)
        hasConnected.current = false
      }
    }

    connectAndListen()

    return () => {
      isMountedRef.current = false
      signalRService.off('ReceiveNotification', handleNotification)
      signalRService.off('NotificationRead', handleNotificationRead)
      signalRService.off('AllNotificationsRead', handleAllNotificationsRead)
    }
  }, [isAuthenticated, handleNotification, handleNotificationRead, handleAllNotificationsRead])

  return {
    isConnected: signalRService.isConnected,
  }
}
