import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ordersApi } from '../api'
import type {
  CreateOrderRequest,
  UpdateOrderRequest,
  ProcessPaymentRequest,
  ShipOrderRequest,
  CancelOrderRequest,
} from '../types'

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

export const useOrderByAuctionId = (auctionId: string) => {
  return useQuery({
    queryKey: orderKeys.byAuctionId(auctionId),
    queryFn: () => ordersApi.getOrderByAuctionId(auctionId),
    enabled: !!auctionId,
  })
}

export const useOrdersByBuyer = (username: string, page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: orderKeys.byBuyer(username, page, pageSize),
    queryFn: () => ordersApi.getOrdersByBuyer(username, page, pageSize),
    enabled: !!username,
  })
}

export const useOrdersBySeller = (username: string, page = 1, pageSize = 20) => {
  return useQuery({
    queryKey: orderKeys.bySeller(username, page, pageSize),
    queryFn: () => ordersApi.getOrdersBySeller(username, page, pageSize),
    enabled: !!username,
  })
}

export const useCreateOrder = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateOrderRequest) => ordersApi.createOrder(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: orderKeys.all })
    },
  })
}

export const useUpdateOrder = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateOrderRequest }) =>
      ordersApi.updateOrder(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.byId(id) })
      queryClient.invalidateQueries({ queryKey: orderKeys.lists() })
    },
  })
}

export const useProcessPayment = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ProcessPaymentRequest }) =>
      ordersApi.processPayment(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: orderKeys.byId(id) })
      queryClient.invalidateQueries({ queryKey: orderKeys.all })
    },
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

export const useCancelOrder = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data?: CancelOrderRequest }) =>
      ordersApi.cancelOrder(id, data),
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
