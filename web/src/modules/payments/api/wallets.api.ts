import { http } from '@/services/http'
import type {
  Wallet,
  WalletTransaction,
  DepositRequest,
  WithdrawRequest,
} from '../types'

export const walletsApi = {
  async getWallet(username: string): Promise<Wallet> {
    const response = await http.get<Wallet>(`/wallets/${username}`)
    return response.data
  },

  async createWallet(username: string): Promise<Wallet> {
    const response = await http.post<Wallet>(`/wallets/${username}/create`)
    return response.data
  },

  async deposit(username: string, data: DepositRequest): Promise<WalletTransaction> {
    const response = await http.post<WalletTransaction>(`/wallets/${username}/deposit`, data)
    return response.data
  },

  async withdraw(username: string, data: WithdrawRequest): Promise<WalletTransaction> {
    const response = await http.post<WalletTransaction>(`/wallets/${username}/withdraw`, data)
    return response.data
  },

  async getTransactions(
    username: string,
    page: number = 1,
    pageSize: number = 20
  ): Promise<{ items: WalletTransaction[]; totalCount: number; totalPages: number }> {
    const response = await http.get<WalletTransaction[]>(`/wallets/${username}/transactions`, {
      params: { page, pageSize },
    })
    const totalCount = parseInt(response.headers['x-total-count'] || '0', 10)
    return { items: response.data, totalCount, totalPages: Math.ceil(totalCount / pageSize) }
  },

  async getTransactionById(id: string): Promise<WalletTransaction> {
    const response = await http.get<WalletTransaction>(`/wallets/transactions/${id}`)
    return response.data
  },
}
