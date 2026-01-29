import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { auctionsApi } from '../api'
import type { AuctionFilters, CreateAuctionRequest, UpdateAuctionRequest } from '../types'

export const auctionKeys = {
  all: ['auctions'] as const,
  lists: () => [...auctionKeys.all, 'list'] as const,
  list: (filters: AuctionFilters) => [...auctionKeys.lists(), filters] as const,
  featured: (pageSize: number) => [...auctionKeys.all, 'featured', pageSize] as const,
  details: () => [...auctionKeys.all, 'detail'] as const,
  detail: (id: string) => [...auctionKeys.details(), id] as const,
  batch: (ids: string[]) => [...auctionKeys.all, 'batch', ids] as const,
  my: (filters: AuctionFilters) => [...auctionKeys.all, 'my', filters] as const,
}

export const useAuctions = (filters: AuctionFilters) => {
  return useQuery({
    queryKey: auctionKeys.list(filters),
    queryFn: () => auctionsApi.getAuctions(filters),
  })
}

export const useFeaturedAuctions = (pageSize = 8) => {
  return useQuery({
    queryKey: auctionKeys.featured(pageSize),
    queryFn: () => auctionsApi.getFeaturedAuctions(pageSize),
  })
}

export const useAuction = (id: string) => {
  return useQuery({
    queryKey: auctionKeys.detail(id),
    queryFn: () => auctionsApi.getAuctionById(id),
    enabled: !!id,
  })
}

export const useAuctionsByIds = (ids: string[]) => {
  return useQuery({
    queryKey: auctionKeys.batch(ids),
    queryFn: () => auctionsApi.getAuctionsByIds(ids),
    enabled: ids.length > 0,
  })
}

export const useMyAuctions = (filters: AuctionFilters) => {
  return useQuery({
    queryKey: auctionKeys.my(filters),
    queryFn: () => auctionsApi.getMyAuctions(filters),
  })
}

export const useCreateAuction = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateAuctionRequest) => auctionsApi.createAuction(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: auctionKeys.lists() })
    },
  })
}

export const useUpdateAuction = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAuctionRequest }) =>
      auctionsApi.updateAuction(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: auctionKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: auctionKeys.lists() })
    },
  })
}

export const useDeleteAuction = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => auctionsApi.deleteAuction(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: auctionKeys.lists() })
    },
  })
}

export const useActivateAuction = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => auctionsApi.activateAuction(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: auctionKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: auctionKeys.lists() })
      queryClient.invalidateQueries({ queryKey: auctionKeys.my({}) })
    },
  })
}

export const useDeactivateAuction = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => auctionsApi.deactivateAuction(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: auctionKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: auctionKeys.lists() })
      queryClient.invalidateQueries({ queryKey: auctionKeys.my({}) })
    },
  })
}
