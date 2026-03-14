import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { biddingApi } from '../api'
import type { PlaceBidRequest } from '../types'
import { auctionKeys } from '@/modules/auctions/hooks'

export const bidKeys = {
  all: ['bids'] as const,
  forAuction: (auctionId: string) => [...bidKeys.all, 'auction', auctionId] as const,
  my: () => [...bidKeys.all, 'my'] as const,
  autoBid: (auctionId: string) => [...bidKeys.all, 'auto', auctionId] as const,
  myAutoBids: () => [...bidKeys.all, 'auto', 'my'] as const,
}

export const useBidsForAuction = (auctionId: string) => {
  return useQuery({
    queryKey: ['bids', 'auction', auctionId],
    queryFn: () => biddingApi.getBidsForAuction(auctionId),
    enabled: !!auctionId,
  })
}

export const useMyBids = () => {
  return useQuery({
    queryKey: ['bids', 'my'],
    queryFn: () => biddingApi.getMyBids(),
  })
}

export const usePlaceBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: PlaceBidRequest) => biddingApi.placeBid(data),
    onSuccess: (_, { auctionId }) => {
      queryClient.invalidateQueries({ queryKey: bidKeys.forAuction(auctionId) })
      queryClient.invalidateQueries({ queryKey: auctionKeys.detail(auctionId) })
    },
  })
}

export const useMyAutoBids = (activeOnly?: boolean) => {
  return useQuery({
    queryKey: ['autoBids', 'my', activeOnly],
    queryFn: () => biddingApi.getMyAutoBids(activeOnly),
  })
}

export const useCancelAutoBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (autoBidId: string) => biddingApi.cancelAutoBid(autoBidId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['autoBids'] })
    },
  })
}
