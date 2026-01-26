import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/app/hooks/useAuth'
import { walletsApi } from '../api'
import type { DepositRequest, WithdrawRequest, TransactionFilters, PaymentMethod } from '../types'

export const currentWalletKeys = {
  all: ['current-wallet'] as const,
  wallet: () => [...currentWalletKeys.all, 'wallet'] as const,
  transactions: (filters: TransactionFilters) => [...currentWalletKeys.all, 'transactions', filters] as const,
}

export const useWallet = () => {
  const { user } = useAuth()
  const username = user?.username || ''

  return useQuery({
    queryKey: currentWalletKeys.wallet(),
    queryFn: () => walletsApi.getWallet(username),
    enabled: !!username,
  })
}

export const useTransactions = (filters: TransactionFilters) => {
  const { user } = useAuth()
  const username = user?.username || ''

  return useQuery({
    queryKey: currentWalletKeys.transactions(filters),
    queryFn: () => walletsApi.getTransactions(username, filters.page || 1, filters.pageSize || 20),
    enabled: !!username,
  })
}

export const useDeposit = () => {
  const { user } = useAuth()
  const username = user?.username || ''
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: DepositRequest) => walletsApi.deposit(username, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currentWalletKeys.all })
    },
  })
}

export const useWithdraw = () => {
  const { user } = useAuth()
  const username = user?.username || ''
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: WithdrawRequest) => walletsApi.withdraw(username, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currentWalletKeys.all })
    },
  })
}

export const paymentMethodKeys = {
  all: ['payment-methods'] as const,
}

export const usePaymentMethods = () => {
  return useQuery({
    queryKey: paymentMethodKeys.all,
    queryFn: async (): Promise<PaymentMethod[]> => [],
    enabled: false,
  })
}

export const useAddPaymentMethod = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (token: string) => {
      console.log('Adding payment method:', token)
      throw new Error('Not implemented')
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentMethodKeys.all })
    },
  })
}

export const useRemovePaymentMethod = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      console.log('Removing payment method:', id)
      throw new Error('Not implemented')
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentMethodKeys.all })
    },
  })
}

export const useSetDefaultPaymentMethod = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      console.log('Setting default payment method:', id)
      throw new Error('Not implemented')
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: paymentMethodKeys.all })
    },
  })
}
