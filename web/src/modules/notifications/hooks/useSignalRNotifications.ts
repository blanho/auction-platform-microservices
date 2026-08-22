import { useAuth } from '@/app/providers'
import type { NotificationPayload } from '@/services/signalr'
import { signalRService } from '@/services/signalr'
import { useSignalRState } from '@/shared/hooks'
import { signalRLogger } from '@/shared/lib/logger'
import { HubConnectionState } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { notificationKeys } from './useNotifications'

export function useSignalRNotifications(): { isConnected: boolean } {
  const queryClient = useQueryClient()
  const { isAuthenticated } = useAuth()
  const connectionState = useSignalRState()

  useEffect(() => {
    if (!isAuthenticated) {
      return
    }

    let isActive = true

    const handleNotification = (notification: NotificationPayload) => {
      signalRLogger.info('New notification received:', notification.id)
      void queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
      void queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    }

    const handleNotificationRead = (notificationId: string) => {
      signalRLogger.info('Notification marked as read:', notificationId)
      void queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
      void queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    }

    const handleAllNotificationsRead = () => {
      signalRLogger.info('All notifications marked as read')
      void queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
      void queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
    }

    const connectAndSubscribe = async (): Promise<void> => {
      try {
        await signalRService.connect()
        if (!isActive) {
          return
        }

        signalRService.on('ReceiveNotification', handleNotification)
        signalRService.on('NotificationRead', handleNotificationRead)
        signalRService.on('AllNotificationsRead', handleAllNotificationsRead)
      } catch (error) {
        if (isActive) {
          signalRLogger.error('Failed to initialize SignalR notifications:', error)
        }
      }
    }

    void connectAndSubscribe()

    return () => {
      isActive = false
      signalRService.off('ReceiveNotification', handleNotification)
      signalRService.off('NotificationRead', handleNotificationRead)
      signalRService.off('AllNotificationsRead', handleAllNotificationsRead)
    }
  }, [isAuthenticated, queryClient])

  return { isConnected: connectionState === HubConnectionState.Connected }
}
