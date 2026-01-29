export type TransactionType =
  | 'deposit'
  | 'withdrawal'
  | 'payment'
  | 'refund'
  | 'hold'
  | 'release'
  | 'escrow_hold'
  | 'escrow_release'
  | 'fee'

export type TransactionStatus = 'pending' | 'completed' | 'failed' | 'cancelled'

export interface Wallet {
  id: string
  username: string
  balance: number
  heldAmount: number
  availableBalance: number
  currency: string
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface WalletTransaction {
  id: string
  username: string
  type: TransactionType
  amount: number
  balance: number
  status: TransactionStatus
  description: string
  reference?: string
  referenceId?: string
  referenceType?: string
  relatedOrderId?: string
  paymentMethod?: string
  externalTransactionId?: string
  createdAt: string
  processedAt?: string
}

export interface DepositRequest {
  amount: number
  paymentMethod: string
  description?: string
}

export interface WithdrawRequest {
  amount: number
  paymentMethod: string
  description?: string
}

export interface TransactionFilters {
  type?: TransactionType
  status?: TransactionStatus
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}
