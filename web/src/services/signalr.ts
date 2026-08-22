import { getAccessToken } from '@/modules/auth/utils/token.utils'
import { signalRLogger } from '@/shared/lib/logger'
import * as signalR from '@microsoft/signalr'

const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || 'http://localhost:6001/hubs/notifications'
const MAX_RECONNECT_ATTEMPTS = 5
const AUTOMATIC_RECONNECT_BASE_DELAY_MS = 1000
const MAX_AUTOMATIC_RECONNECT_DELAY_MS = 30000
const MANUAL_RECONNECT_DELAY_MS = 5000

type ConnectionStateListener = () => void

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private connectionStartPromise: Promise<void> | null = null
  private reconnectAttempts = 0
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null
  private isIntentionalDisconnect = false
  private readonly connectionStateListeners = new Set<ConnectionStateListener>()

  async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    if (this.connectionStartPromise) {
      await this.connectionStartPromise
      return
    }

    if (this.connection && this.connection.state !== signalR.HubConnectionState.Disconnected) {
      return
    }

    if (!getAccessToken()) {
      signalRLogger.warn('No access token available for SignalR connection')
      return
    }

    this.clearReconnectTimer()
    this.isIntentionalDisconnect = false

    const connection = this.connection ?? this.createConnection()
    this.connection = connection

    const connectionStartPromise = this.startConnection(connection)
    this.connectionStartPromise = connectionStartPromise
    this.notifyConnectionStateListeners()

    try {
      await connectionStartPromise
    } finally {
      if (this.connectionStartPromise === connectionStartPromise) {
        this.connectionStartPromise = null
      }
    }
  }

  private createConnection(): signalR.HubConnection {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(SIGNALR_URL, {
        accessTokenFactory: () => getAccessToken() ?? '',
        transport:
          signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < MAX_RECONNECT_ATTEMPTS) {
            return Math.min(
              AUTOMATIC_RECONNECT_BASE_DELAY_MS * 2 ** retryContext.previousRetryCount,
              MAX_AUTOMATIC_RECONNECT_DELAY_MS
            )
          }
          return null
        },
      })
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    this.setupConnectionHandlers(connection)

    return connection
  }

  private async startConnection(connection: signalR.HubConnection): Promise<void> {
    try {
      await connection.start()

      if (this.connection !== connection || this.isIntentionalDisconnect) {
        await connection.stop()
        return
      }

      this.reconnectAttempts = 0
      this.notifyConnectionStateListeners()
      signalRLogger.info('Connected')
    } catch (error) {
      if (this.connection !== connection || this.isIntentionalDisconnect) {
        return
      }

      signalRLogger.error('Connection failed:', error)
      this.notifyConnectionStateListeners()
      this.scheduleReconnect()
    }
  }

  private setupConnectionHandlers(connection: signalR.HubConnection): void {
    connection.onreconnecting((error) => {
      this.notifyConnectionStateListeners()
      signalRLogger.warn('Reconnecting...', error)
    })

    connection.onreconnected((connectionId) => {
      if (this.connection !== connection) {
        return
      }

      signalRLogger.info('Reconnected:', connectionId)
      this.reconnectAttempts = 0
      this.notifyConnectionStateListeners()
    })

    connection.onclose((error) => {
      if (this.connection !== connection) {
        return
      }

      signalRLogger.info('Disconnected', error)
      this.notifyConnectionStateListeners()
      this.scheduleReconnect()
    })
  }

  private scheduleReconnect(): void {
    if (
      this.isIntentionalDisconnect ||
      this.reconnectTimer ||
      this.reconnectAttempts >= MAX_RECONNECT_ATTEMPTS
    ) {
      return
    }

    this.reconnectAttempts++
    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null
      void this.connect()
    }, MANUAL_RECONNECT_DELAY_MS)
  }

  private clearReconnectTimer(): void {
    if (!this.reconnectTimer) {
      return
    }

    clearTimeout(this.reconnectTimer)
    this.reconnectTimer = null
  }

  async disconnect(): Promise<void> {
    this.isIntentionalDisconnect = true
    this.clearReconnectTimer()
    this.reconnectAttempts = 0

    const connection = this.connection
    this.connection = null
    this.connectionStartPromise = null
    this.notifyConnectionStateListeners()

    if (!connection) {
      return
    }

    try {
      await connection.stop()
      signalRLogger.info('Disconnected')
    } catch (error) {
      signalRLogger.error('Error disconnecting:', error)
    }
  }

  subscribeToState(listener: ConnectionStateListener): () => void {
    this.connectionStateListeners.add(listener)
    return () => this.connectionStateListeners.delete(listener)
  }

  private notifyConnectionStateListeners(): void {
    this.connectionStateListeners.forEach((listener) => listener())
  }

  on<T = unknown>(eventName: string, callback: (data: T) => void): void {
    if (!this.connection) {
      signalRLogger.warn(`Cannot register event "${eventName}" - connection not initialized`)
      return
    }
    this.connection.on(eventName, callback)
  }

  off<T = unknown>(eventName: string, callback?: (data: T) => void): void {
    if (!this.connection) {
      return
    }
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
    return this.connection.invoke(methodName, ...args)
  }

  async joinAuctionRoom(auctionId: string): Promise<void> {
    try {
      await this.invoke('JoinAuctionRoom', auctionId)
      signalRLogger.info(`Joined auction room: ${auctionId}`)
    } catch (error) {
      signalRLogger.error('Failed to join auction room:', error)
      throw error
    }
  }

  async leaveAuctionRoom(auctionId: string): Promise<void> {
    try {
      await this.invoke('LeaveAuctionRoom', auctionId)
      signalRLogger.info(`Left auction room: ${auctionId}`)
    } catch (error) {
      signalRLogger.error('Failed to leave auction room:', error)
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
