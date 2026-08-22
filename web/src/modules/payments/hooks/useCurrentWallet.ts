import { useAuth } from '@/app/hooks/useAuth'
import { useQuery } from '@tanstack/react-query'
import { walletsApi } from '../api'
import type { TransactionFilters } from '../types'

export const currentWalletKeys = {
  all: ['current-wallet'] as const,
  wallet: () => [...currentWalletKeys.all, 'wallet'] as const,
  transactions: (filters: TransactionFilters) =>
    [...currentWalletKeys.all, 'transactions', filters] as const,
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
