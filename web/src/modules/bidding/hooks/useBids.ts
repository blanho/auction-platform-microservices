import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { biddingApi } from '../api'
import { BID_CONSTANTS } from '../constants'
import { placeBidSchema, retractBidSchema } from '../schemas'
import type {
  PlaceBidRequest,
  BidHistoryFilters,
} from '../types'

const QUERY_KEYS = BID_CONSTANTS.QUERY_KEYS

export const usePlaceBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: PlaceBidRequest) => {
      const result = placeBidSchema.safeParse(data)
      if (!result.success) {
        throw new Error(result.error.issues.map((e) => e.message).join(', '))
      }
      return biddingApi.placeBid(data)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.bidsForAuction(variables.auctionId) })
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.myBids })
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.winningBids({}) })
    },
  })
}

export const useBidById = (bidId: string) => {
  return useQuery({
    queryKey: QUERY_KEYS.bidById(bidId),
    queryFn: () => biddingApi.getBidById(bidId),
    enabled: !!bidId,
  })
}

export const useBidsForAuction = (auctionId: string) => {
  return useQuery({
    queryKey: QUERY_KEYS.bidsForAuction(auctionId),
    queryFn: () => biddingApi.getBidsForAuction(auctionId),
    enabled: !!auctionId,
  })
}

export const useMyBids = () => {
  return useQuery({
    queryKey: QUERY_KEYS.myBids,
    queryFn: () => biddingApi.getMyBids(),
  })
}

export const useWinningBids = (page: number = 1, pageSize: number = 20) => {
  return useQuery({
    queryKey: QUERY_KEYS.winningBids({ page, pageSize }),
    queryFn: () => biddingApi.getWinningBids(page, pageSize),
  })
}

export const useBidHistory = (filters: BidHistoryFilters) => {
  return useQuery({
    queryKey: QUERY_KEYS.bidHistory(filters),
    queryFn: () => biddingApi.getBidHistory(filters),
  })
}

export const useRetractBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ bidId, reason }: { bidId: string; reason: string }) => {
      const result = retractBidSchema.safeParse({ reason })
      if (!result.success) {
        throw new Error(result.error.issues.map((e) => e.message).join(', '))
      }
      return biddingApi.retractBid(bidId, reason)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.myBids })
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.bids })
    },
  })
}

export const useBidIncrement = (currentBid: number) => {
  return useQuery({
    queryKey: QUERY_KEYS.bidIncrement(currentBid),
    queryFn: () => biddingApi.getBidIncrement(currentBid),
    enabled: currentBid > 0,
  })
}
