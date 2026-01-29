import { useEffect, useRef, useCallback } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { signalRService } from '@/services/signalr'
import type { BidUpdatePayload, AuctionStatusPayload } from '@/services/signalr'
import { signalRLogger } from '@/shared/lib/logger'

interface UseAuctionSignalROptions {
  auctionId: string | undefined
  enabled?: boolean
}

export const useAuctionSignalR = ({ auctionId, enabled = true }: UseAuctionSignalROptions) => {
  const queryClient = useQueryClient()
  const hasJoined = useRef(false)
  const isMountedRef = useRef(true)
  const currentAuctionId = useRef(auctionId)

  const handleBidPlaced = useCallback(
    (bidUpdate: BidUpdatePayload) => {
      if (!isMountedRef.current) {return}
      if (bidUpdate.auctionId === currentAuctionId.current) {
        signalRLogger.info('💰 New bid placed:', bidUpdate.bidId)
        queryClient.invalidateQueries({ queryKey: ['auction', currentAuctionId.current] })
        queryClient.invalidateQueries({ queryKey: ['bids', currentAuctionId.current] })
      }
    },
    [queryClient]
  )

  const handleAuctionEnded = useCallback(
    (status: AuctionStatusPayload) => {
      if (!isMountedRef.current) {return}
      if (status.auctionId === currentAuctionId.current) {
        signalRLogger.info('🏁 Auction ended:', status.auctionId)
        queryClient.invalidateQueries({ queryKey: ['auction', currentAuctionId.current] })
      }
    },
    [queryClient]
  )

  const handleAuctionExtended = useCallback(
    (status: AuctionStatusPayload) => {
      if (!isMountedRef.current) {return}
      if (status.auctionId === currentAuctionId.current) {
        signalRLogger.info('⏰ Auction extended:', status.auctionId)
        queryClient.invalidateQueries({ queryKey: ['auction', currentAuctionId.current] })
      }
    },
    [queryClient]
  )

  useEffect(() => {
    currentAuctionId.current = auctionId
  }, [auctionId])

  useEffect(() => {
    isMountedRef.current = true

    if (!auctionId || !enabled || !signalRService.isConnected) {
      return
    }

    const joinRoomAndListen = async () => {
      if (hasJoined.current) {return}

      try {
        await signalRService.joinAuctionRoom(auctionId)

        if (!isMountedRef.current) {
          signalRService.leaveAuctionRoom(auctionId)
          return
        }

        hasJoined.current = true
        signalRLogger.info(`🏠 Joined auction room: ${auctionId}`)

        signalRService.on('BidPlaced', handleBidPlaced)
        signalRService.on('AuctionEnded', handleAuctionEnded)
        signalRService.on('AuctionExtended', handleAuctionExtended)
      } catch (error) {
        signalRLogger.error('Failed to join auction room:', error)
        hasJoined.current = false
      }
    }

    joinRoomAndListen()

    return () => {
      isMountedRef.current = false

      if (hasJoined.current && auctionId) {
        signalRService.leaveAuctionRoom(auctionId)
        hasJoined.current = false
        signalRLogger.info(`👋 Left auction room: ${auctionId}`)
      }

      signalRService.off('BidPlaced', handleBidPlaced)
      signalRService.off('AuctionEnded', handleAuctionEnded)
      signalRService.off('AuctionExtended', handleAuctionExtended)
    }
  }, [auctionId, enabled, handleBidPlaced, handleAuctionEnded, handleAuctionExtended])

  return {
    isConnected: signalRService.isConnected,
  }
}
