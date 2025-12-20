import apiClient from '@/lib/api/axios';
import { API_ENDPOINTS } from '@/constants/api';

export interface WalletTransaction {
    id: string;
    type: string;
    amount: number;
    status: string;
    description?: string;
    reference?: string;
    createdAt: string;
    processedAt?: string;
}

export interface WalletBalance {
    totalBalance: number;
    availableBalance: number;
    pendingHolds: number;
    pendingWithdrawals: number;
}

export interface PagedTransactionsResponse {
    transactions: WalletTransaction[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
}

export interface GetTransactionsParams {
    type?: string;
    status?: string;
    pageNumber?: number;
    pageSize?: number;
}

export interface CreateDepositDto {
    amount: number;
    paymentMethod?: string;
    description?: string;
}

export interface CreateWithdrawalDto {
    amount: number;
    paymentMethod?: string;
    description?: string;
}

export interface AdminWithdrawal {
    id: string;
    username: string;
    amount: number;
    paymentMethod?: string;
    status: string;
    requestedAt: string;
}

export interface PaymentStatistics {
    pendingWithdrawalsCount: number;
    pendingWithdrawalsTotal: number;
}

export const walletService = {
    getWallet: async (username: string): Promise<WalletBalance> => {
        const { data } = await apiClient.get<WalletBalance>(
            API_ENDPOINTS.WALLETS.BY_USERNAME(username)
        );
        return data;
    },

    createWallet: async (username: string): Promise<WalletBalance> => {
        const { data } = await apiClient.post<WalletBalance>(
            API_ENDPOINTS.WALLETS.CREATE(username)
        );
        return data;
    },

    getTransactions: async (username: string, params: GetTransactionsParams = {}): Promise<WalletTransaction[]> => {
        const { data } = await apiClient.get<WalletTransaction[]>(
            API_ENDPOINTS.WALLETS.TRANSACTIONS(username),
            {
                params: {
                    page: params.pageNumber || 1,
                    pageSize: params.pageSize || 20,
                },
            }
        );
        return data;
    },

    getTransaction: async (id: string): Promise<WalletTransaction> => {
        const { data } = await apiClient.get<WalletTransaction>(
            API_ENDPOINTS.WALLETS.TRANSACTION_BY_ID(id)
        );
        return data;
    },

    createDeposit: async (username: string, dto: CreateDepositDto): Promise<WalletTransaction> => {
        const { data } = await apiClient.post<WalletTransaction>(
            API_ENDPOINTS.WALLETS.DEPOSIT(username),
            dto
        );
        return data;
    },

    createWithdrawal: async (username: string, dto: CreateWithdrawalDto): Promise<WalletTransaction> => {
        const { data } = await apiClient.post<WalletTransaction>(
            API_ENDPOINTS.WALLETS.WITHDRAW(username),
            dto
        );
        return data;
    },
} as const;

export const adminPaymentService = {
    getPendingWithdrawals: async (): Promise<AdminWithdrawal[]> => {
        const { data } = await apiClient.get<AdminWithdrawal[]>(
            API_ENDPOINTS.ADMIN.PAYMENTS_PENDING
        );
        return data;
    },

    approveWithdrawal: async (id: string, externalTransactionId?: string): Promise<void> => {
        await apiClient.post(API_ENDPOINTS.ADMIN.PAYMENT_APPROVE(id), {
            externalTransactionId,
        });
    },

    rejectWithdrawal: async (id: string, reason: string): Promise<void> => {
        await apiClient.post(API_ENDPOINTS.ADMIN.PAYMENT_REJECT(id), { reason });
    },

    getStatistics: async (): Promise<PaymentStatistics> => {
        const { data } = await apiClient.get<PaymentStatistics>(
            API_ENDPOINTS.ADMIN.PAYMENTS_STATS
        );
        return data;
    },
} as const;
