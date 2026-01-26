import { useEffect, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { signalRService } from '@/services/signalr'
import type { NotificationPayload } from '@/services/signalr'
import { notificationKeys } from '../hooks/useNotifications'
import { useAuth } from '@/app/providers'

export const useSignalRNotifications = () => {
  const queryClient = useQueryClient()
  const { isAuthenticated } = useAuth()
  const hasConnected = useRef(false)

  useEffect(() => {
    if (!isAuthenticated) {
      if (hasConnected.current) {
        signalRService.disconnect()
        hasConnected.current = false
      }
      return
    }

    if (hasConnected.current) return

    const connectAndListen = async () => {
      try {
        await signalRService.connect()
        hasConnected.current = true

        signalRService.on('ReceiveNotification', (notification: NotificationPayload) => {
          console.log('📬 New notification received:', notification)

          queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
          queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
        })

        signalRService.on('NotificationRead', (notificationId: string) => {
          console.log('✅ Notification marked as read:', notificationId)

          queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
          queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
        })

        signalRService.on('AllNotificationsRead', () => {
          console.log('✅ All notifications marked as read')

          queryClient.invalidateQueries({ queryKey: notificationKeys.summary() })
          queryClient.invalidateQueries({ queryKey: notificationKeys.lists() })
        })
      } catch (error) {
        console.error('Failed to connect to SignalR:', error)
        hasConnected.current = false
      }
    }

    connectAndListen()

    return () => {
      signalRService.off('ReceiveNotification')
      signalRService.off('NotificationRead')
      signalRService.off('AllNotificationsRead')
    }
  }, [isAuthenticated, queryClient])

  return {
    isConnected: signalRService.isConnected,
  }
}
