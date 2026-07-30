import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { biddingApi } from '../api'
import type { PlaceBidRequest } from '../types'
import { auctionKeys } from '@/modules/auctions/hooks'

export const bidKeys = {
  all: ['bids'] as const,
  forAuction: (auctionId: string) => [...bidKeys.all, 'auction', auctionId] as const,
  my: () => [...bidKeys.all, 'my'] as const,
}

export const useBidsForAuction = (auctionId: string) => {
  return useQuery({
    queryKey: bidKeys.forAuction(auctionId),
    queryFn: () => biddingApi.getBidsForAuction(auctionId),
    enabled: !!auctionId,
  })
}

export const useMyBids = () => {
  return useQuery({
    queryKey: bidKeys.my(),
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
