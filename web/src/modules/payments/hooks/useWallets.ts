import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { walletsApi } from '../api'

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

export const useWalletTransactions = (username: string, page = 1, pageSize = 20) => {
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
