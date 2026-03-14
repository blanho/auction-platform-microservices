import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { viewsApi } from '../api'

export const viewKeys = {
  all: ['views'] as const,
  count: (auctionId: string) => [...viewKeys.all, 'count', auctionId] as const,
}

export const useViewCount = (auctionId: string) => {
  return useQuery({
    queryKey: viewKeys.count(auctionId),
    queryFn: () => viewsApi.getViewCount(auctionId),
    enabled: !!auctionId,
    staleTime: 60 * 1000,
  })
}

export const useRecordView = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (auctionId: string) => viewsApi.recordView(auctionId),
    onSuccess: (_, auctionId) => {
      queryClient.invalidateQueries({ queryKey: viewKeys.count(auctionId) })
    },
  })
}
