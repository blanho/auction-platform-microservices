import type { AuctionStatusPayload, BidUpdatePayload } from '@/services/signalr'
import { signalRService } from '@/services/signalr'
import { useSignalRState } from '@/shared/hooks'
import { signalRLogger } from '@/shared/lib/logger'
import { HubConnectionState } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useCallback, useEffect, useRef } from 'react'

interface UseAuctionSignalROptions {
  auctionId: string | undefined
  enabled?: boolean
}

interface AuctionRoomJoinRequest {
  auctionId: string
  promise: Promise<void>
}

export const useAuctionSignalR = ({ auctionId, enabled = true }: UseAuctionSignalROptions) => {
  const queryClient = useQueryClient()
  const connectionState = useSignalRState()
  const desiredAuctionIdRef = useRef<string | null>(null)
  const joinedAuctionIdRef = useRef<string | null>(null)
  const joinRequestRef = useRef<AuctionRoomJoinRequest | null>(null)
  const isConnected = connectionState === HubConnectionState.Connected

  const handleBidPlaced = useCallback(
    (bidUpdate: BidUpdatePayload) => {
      if (bidUpdate.auctionId !== auctionId) {
        return
      }

      signalRLogger.info('New bid placed:', bidUpdate.bidId)
      void queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
      void queryClient.invalidateQueries({ queryKey: ['bids', auctionId] })
    },
    [auctionId, queryClient]
  )

  const handleAuctionEnded = useCallback(
    (status: AuctionStatusPayload) => {
      if (status.auctionId !== auctionId) {
        return
      }

      signalRLogger.info('Auction ended:', status.auctionId)
      void queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
    },
    [auctionId, queryClient]
  )

  const handleAuctionExtended = useCallback(
    (status: AuctionStatusPayload) => {
      if (status.auctionId !== auctionId) {
        return
      }

      signalRLogger.info('Auction extended:', status.auctionId)
      void queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
    },
    [auctionId, queryClient]
  )

  useEffect(() => {
    if (!auctionId || !enabled || !isConnected) {
      desiredAuctionIdRef.current = null
      return
    }

    desiredAuctionIdRef.current = auctionId
    let isActive = true

    const joinRoomAndSubscribe = async (): Promise<void> => {
      let joinRequest = joinRequestRef.current
      if (!joinRequest || joinRequest.auctionId !== auctionId) {
        joinRequest = {
          auctionId,
          promise: signalRService.joinAuctionRoom(auctionId),
        }
        joinRequestRef.current = joinRequest
      }

      try {
        await joinRequest.promise
      } catch (error) {
        if (isActive) {
          signalRLogger.error('Failed to subscribe to auction updates:', error)
        }
        return
      } finally {
        if (joinRequestRef.current === joinRequest) {
          joinRequestRef.current = null
        }
      }

      if (!isActive) {
        if (desiredAuctionIdRef.current !== auctionId) {
          void signalRService.leaveAuctionRoom(auctionId)
        }
        return
      }

      if (desiredAuctionIdRef.current !== auctionId) {
        void signalRService.leaveAuctionRoom(auctionId)
        return
      }

      joinedAuctionIdRef.current = auctionId
      signalRService.off('BidPlaced', handleBidPlaced)
      signalRService.off('AuctionEnded', handleAuctionEnded)
      signalRService.off('AuctionExtended', handleAuctionExtended)
      signalRService.on('BidPlaced', handleBidPlaced)
      signalRService.on('AuctionEnded', handleAuctionEnded)
      signalRService.on('AuctionExtended', handleAuctionExtended)
    }

    void joinRoomAndSubscribe()

    return () => {
      isActive = false

      signalRService.off('BidPlaced', handleBidPlaced)
      signalRService.off('AuctionEnded', handleAuctionEnded)
      signalRService.off('AuctionExtended', handleAuctionExtended)

      if (joinedAuctionIdRef.current === auctionId) {
        joinedAuctionIdRef.current = null
        void signalRService.leaveAuctionRoom(auctionId)
      }

      if (desiredAuctionIdRef.current === auctionId) {
        desiredAuctionIdRef.current = null
      }
    }
  }, [auctionId, enabled, isConnected, handleBidPlaced, handleAuctionEnded, handleAuctionExtended])

  return { isConnected }
}
