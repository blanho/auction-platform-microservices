import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api'
import type { ShipOrderRequest } from '../types'

export const orderKeys = {
  all: ['orders'] as const,
  lists: () => [...orderKeys.all, 'list'] as const,
  byId: (id: string) => [...orderKeys.all, 'by-id', id] as const,
  byAuctionId: (auctionId: string) => [...orderKeys.all, 'by-auction', auctionId] as const,
  byBuyer: (username: string, page: number, pageSize: number) =>
    [...orderKeys.all, 'by-buyer', username, { page, pageSize }] as const,
  bySeller: (username: string, page: number, pageSize: number) =>
    [...orderKeys.all, 'by-seller', username, { page, pageSize }] as const,
}

export const useOrderById = (id: string) => {
  return useQuery({
    queryKey: orderKeys.byId(id),
    queryFn: () => ordersApi.getOrderById(id),
    enabled: !!id,
  })
}

export const useShipOrder = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ShipOrderRequest }) =>
      ordersApi.shipOrder(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.byId(id) })
      queryClient.invalidateQueries({ queryKey: orderKeys.all })
    },
  })
}

export const useMarkDelivered = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => ordersApi.markDelivered(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.byId(id) })
      queryClient.invalidateQueries({ queryKey: orderKeys.all })
    },
  })
}
