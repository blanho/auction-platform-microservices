import * as signalR from '@microsoft/signalr'
import { getAccessToken } from '@/modules/auth/utils/token.utils'

const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || 'http://localhost:5000/notificationHub'

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private isIntentionalDisconnect = false

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    const token = getAccessToken()
    if (!token) {
      console.warn('No access token available for SignalR connection')
      return
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_URL, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000)
          }
          return null
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build()

    this.setupEventHandlers()

    try {
      this.isIntentionalDisconnect = false
      await this.connection.start()
      this.reconnectAttempts = 0
      console.log('✅ SignalR connected')
    } catch (error) {
      console.error('❌ SignalR connection failed:', error)
      if (this.reconnectAttempts < this.maxReconnectAttempts) {
        this.reconnectAttempts++
        setTimeout(() => this.connect(), 5000)
      }
    }
  }

  private setupEventHandlers(): void {
    if (!this.connection) return

    this.connection.onreconnecting((error) => {
      console.warn('🔄 SignalR reconnecting...', error)
    })

    this.connection.onreconnected((connectionId) => {
      console.log('✅ SignalR reconnected:', connectionId)
      this.reconnectAttempts = 0
    })

    this.connection.onclose((error) => {
      console.log('🔌 SignalR disconnected', error)
      if (!this.isIntentionalDisconnect && this.reconnectAttempts < this.maxReconnectAttempts) {
        this.reconnectAttempts++
        setTimeout(() => this.connect(), 5000)
      }
    })
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      this.isIntentionalDisconnect = true
      try {
        await this.connection.stop()
        console.log('SignalR disconnected')
      } catch (error) {
        console.error('Error disconnecting SignalR:', error)
      }
      this.connection = null
    }
  }

  on<T = unknown>(eventName: string, callback: (data: T) => void): void {
    if (!this.connection) {
      console.warn(`Cannot register event "${eventName}" - connection not initialized`)
      return
    }
    this.connection.on(eventName, callback)
  }

  off<T = unknown>(eventName: string, callback?: (data: T) => void): void {
    if (!this.connection) return
    if (callback) {
      this.connection.off(eventName, callback)
    } else {
      this.connection.off(eventName)
    }
  }

  async invoke(methodName: string, ...args: unknown[]): Promise<unknown> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR connection is not established')
    }
    return await this.connection.invoke(methodName, ...args)
  }

  async joinAuctionRoom(auctionId: string): Promise<void> {
    try {
      await this.invoke('JoinAuctionRoom', auctionId)
      console.log(`📍 Joined auction room: ${auctionId}`)
    } catch (error) {
      console.error('Failed to join auction room:', error)
    }
  }

  async leaveAuctionRoom(auctionId: string): Promise<void> {
    try {
      await this.invoke('LeaveAuctionRoom', auctionId)
      console.log(`📍 Left auction room: ${auctionId}`)
    } catch (error) {
      console.error('Failed to leave auction room:', error)
    }
  }

  get isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected
  }

  get state(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null
  }
}

export const signalRService = new SignalRService()

export interface NotificationPayload {
  id: string
  userId: string
  type: string
  title: string
  message: string
  data?: string
  status: string
  readAt?: string
  auctionId?: string
  bidId?: string
  createdAt: string
}

export interface BidUpdatePayload {
  auctionId: string
  bidId: string
  amount: number
  bidderId: string
  bidderName: string
  timestamp: string
}

export interface AuctionStatusPayload {
  auctionId: string
  status: string
  winnerId?: string
  winningBid?: number
}
