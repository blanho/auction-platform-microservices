import apiClient from '@/lib/api/axios';

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
    paymentMethodId?: string;
}

export interface CreateWithdrawalDto {
    amount: number;
    paymentMethodId?: string;
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
    async getBalance(): Promise<WalletBalance> {
        const response = await apiClient.get<WalletBalance>('/utility/api/v1/wallet/balance');
        return response.data;
    },

    async getTransactions(params: GetTransactionsParams = {}): Promise<PagedTransactionsResponse> {
        const response = await apiClient.get<PagedTransactionsResponse>('/utility/api/v1/wallet/transactions', {
            params: {
                type: params.type && params.type !== 'all' ? params.type : undefined,
                status: params.status && params.status !== 'all' ? params.status : undefined,
                pageNumber: params.pageNumber || 1,
                pageSize: params.pageSize || 20,
            },
        });
        return response.data;
    },

    async getTransaction(id: string): Promise<WalletTransaction> {
        const response = await apiClient.get<WalletTransaction>(`/utility/api/v1/wallet/transactions/${id}`);
        return response.data;
    },

    async createDeposit(dto: CreateDepositDto): Promise<WalletTransaction> {
        const response = await apiClient.post<WalletTransaction>('/utility/api/v1/wallet/deposit', dto);
        return response.data;
    },

    async createWithdrawal(dto: CreateWithdrawalDto): Promise<WalletTransaction> {
        const response = await apiClient.post<WalletTransaction>('/utility/api/v1/wallet/withdraw', dto);
        return response.data;
    },
};

export const adminPaymentService = {
    async getPendingWithdrawals(): Promise<AdminWithdrawal[]> {
        const response = await apiClient.get<AdminWithdrawal[]>('/utility/api/v1/admin/payments/withdrawals/pending');
        return response.data;
    },

    async approveWithdrawal(id: string, externalTransactionId?: string): Promise<void> {
        await apiClient.post(`/utility/api/v1/admin/payments/withdrawals/${id}/approve`, {
            externalTransactionId,
        });
    },

    async rejectWithdrawal(id: string, reason: string): Promise<void> {
        await apiClient.post(`/utility/api/v1/admin/payments/withdrawals/${id}/reject`, { reason });
    },

    async getStatistics(): Promise<PaymentStatistics> {
        const response = await apiClient.get<PaymentStatistics>('/utility/api/v1/admin/payments/statistics');
        return response.data;
    },
};
