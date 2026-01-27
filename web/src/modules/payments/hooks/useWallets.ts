import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { walletsApi } from '../api'
import type { DepositRequest, WithdrawRequest } from '../types'

export const walletKeys = {
  all: ['wallets'] as const,
  byUsername: (username: string) => [...walletKeys.all, 'by-username', username] as const,
  transactions: (username: string, page: number, pageSize: number) =>
    [...walletKeys.all, 'transactions', username, { page, pageSize }] as const,
  transactionById: (id: string) => [...walletKeys.all, 'transaction', id] as const,
}

export const useWallet = (username: string) => {
  return useQuery({
    queryKey: walletKeys.byUsername(username),
    queryFn: () => walletsApi.getWallet(username),
    enabled: !!username,
  })
}

export const useCreateWallet = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (username: string) => walletsApi.createWallet(username),
    onSuccess: (_, username) => {
      queryClient.invalidateQueries({ queryKey: walletKeys.byUsername(username) })
    },
  })
}

export const useDeposit = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ username, data }: { username: string; data: DepositRequest }) =>
      walletsApi.deposit(username, data),
    onSuccess: (_, { username }) => {
      queryClient.invalidateQueries({ queryKey: walletKeys.byUsername(username) })
      queryClient.invalidateQueries({ queryKey: walletKeys.all })
    },
  })
}

export const useWithdraw = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ username, data }: { username: string; data: WithdrawRequest }) =>
      walletsApi.withdraw(username, data),
    onSuccess: (_, { username }) => {
      queryClient.invalidateQueries({ queryKey: walletKeys.byUsername(username) })
      queryClient.invalidateQueries({ queryKey: walletKeys.all })
    },
  })
}

export const useWalletTransactions = (
  username: string,
  page: number = 1,
  pageSize: number = 20
) => {
  return useQuery({
    queryKey: walletKeys.transactions(username, page, pageSize),
    queryFn: () => walletsApi.getTransactions(username, page, pageSize),
    enabled: !!username,
  })
}

export const useTransactionById = (id: string) => {
  return useQuery({
    queryKey: walletKeys.transactionById(id),
    queryFn: () => walletsApi.getTransactionById(id),
    enabled: !!id,
  })
}
