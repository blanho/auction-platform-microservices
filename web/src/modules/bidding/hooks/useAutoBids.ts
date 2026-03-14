import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { biddingApi } from '../api'
import { BID_CONSTANTS } from '../constants'
import { createAutoBidSchema, updateAutoBidSchema } from '../schemas'
import type { CreateAutoBidRequest, UpdateAutoBidRequest } from '../types'

const QUERY_KEYS = BID_CONSTANTS.QUERY_KEYS

export const useCreateAutoBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateAutoBidRequest) => {
      const result = createAutoBidSchema.safeParse(data)
      if (!result.success) {
        throw new Error(result.error.issues.map((e) => e.message).join(', '))
      }
      return biddingApi.createAutoBid(data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.autoBids })
    },
  })
}

export const useAutoBidById = (autoBidId: string) => {
  return useQuery({
    queryKey: QUERY_KEYS.autoBidById(autoBidId),
    queryFn: () => biddingApi.getAutoBidById(autoBidId),
    enabled: !!autoBidId,
  })
}

export const useMyAutoBids = (activeOnly?: boolean, page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: QUERY_KEYS.myAutoBids(activeOnly, page, pageSize),
    queryFn: () => biddingApi.getMyAutoBids(activeOnly, page, pageSize),
  })
}

export const useAutoBidForAuction = (auctionId: string | undefined, enabled = true) => {
  return useQuery({
    queryKey: ['autobid', 'auction', auctionId],
    queryFn: () => {
      if (!auctionId) {
        throw new Error('Auction ID is required')
      }
      return biddingApi.getAutoBidForAuction(auctionId)
    },
    enabled: enabled && !!auctionId,
  })
}

export const useUpdateAutoBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ autoBidId, data }: { autoBidId: string; data: UpdateAutoBidRequest }) => {
      const result = updateAutoBidSchema.safeParse(data)
      if (!result.success) {
        throw new Error(result.error.issues.map((e) => e.message).join(', '))
      }
      return biddingApi.updateAutoBid(autoBidId, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.autoBids })
    },
  })
}

export const useToggleAutoBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ autoBidId, activate }: { autoBidId: string; activate: boolean }) =>
      biddingApi.toggleAutoBid(autoBidId, activate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.autoBids })
    },
  })
}

export const useCancelAutoBid = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (autoBidId: string) => biddingApi.cancelAutoBid(autoBidId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.autoBids })
    },
  })
}
