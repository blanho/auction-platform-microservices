import { useEffect, useRef } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { signalRService } from '@/services/signalr'
import type { BidUpdatePayload, AuctionStatusPayload } from '@/services/signalr'

interface UseAuctionSignalROptions {
  auctionId: string | undefined
  enabled?: boolean
}

export const useAuctionSignalR = ({ auctionId, enabled = true }: UseAuctionSignalROptions) => {
  const queryClient = useQueryClient()
  const hasJoined = useRef(false)

  useEffect(() => {
    if (!auctionId || !enabled || !signalRService.isConnected) {
      return
    }

    const joinRoomAndListen = async () => {
      if (hasJoined.current) return

      try {
        await signalRService.joinAuctionRoom(auctionId)
        hasJoined.current = true
        console.log(`🏠 Joined auction room: ${auctionId}`)

        signalRService.on('BidPlaced', (bidUpdate: BidUpdatePayload) => {
          if (bidUpdate.auctionId === auctionId) {
            console.log('💰 New bid placed:', bidUpdate)

            queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
            queryClient.invalidateQueries({ queryKey: ['bids', auctionId] })
          }
        })

        signalRService.on('AuctionEnded', (status: AuctionStatusPayload) => {
          if (status.auctionId === auctionId) {
            console.log('🏁 Auction ended:', status)

            queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
          }
        })

        signalRService.on('AuctionExtended', (status: AuctionStatusPayload) => {
          if (status.auctionId === auctionId) {
            console.log('⏰ Auction extended:', status)

            queryClient.invalidateQueries({ queryKey: ['auction', auctionId] })
          }
        })
      } catch (error) {
        console.error('Failed to join auction room:', error)
        hasJoined.current = false
      }
    }

    joinRoomAndListen()

    return () => {
      if (hasJoined.current && auctionId) {
        signalRService.leaveAuctionRoom(auctionId)
        hasJoined.current = false
        console.log(`👋 Left auction room: ${auctionId}`)
      }

      signalRService.off('BidPlaced')
      signalRService.off('AuctionEnded')
      signalRService.off('AuctionExtended')
    }
  }, [auctionId, enabled, queryClient])

  return {
    isConnected: signalRService.isConnected,
  }
}
