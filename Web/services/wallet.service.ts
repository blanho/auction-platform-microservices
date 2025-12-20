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
    async getWallet(username: string): Promise<WalletBalance> {
        const response = await apiClient.get<WalletBalance>(`/wallets/${username}`);
        return response.data;
    },

    async createWallet(username: string): Promise<WalletBalance> {
        const response = await apiClient.post<WalletBalance>(`/wallets/${username}/create`);
        return response.data;
    },

    async getTransactions(username: string, params: GetTransactionsParams = {}): Promise<WalletTransaction[]> {
        const response = await apiClient.get<WalletTransaction[]>(`/wallets/${username}/transactions`, {
            params: {
                page: params.pageNumber || 1,
                pageSize: params.pageSize || 20,
            },
        });
        return response.data;
    },

    async getTransaction(id: string): Promise<WalletTransaction> {
        const response = await apiClient.get<WalletTransaction>(`/wallets/transactions/${id}`);
        return response.data;
    },

    async createDeposit(username: string, dto: CreateDepositDto): Promise<WalletTransaction> {
        const response = await apiClient.post<WalletTransaction>(`/wallets/${username}/deposit`, dto);
        return response.data;
    },

    async createWithdrawal(username: string, dto: CreateWithdrawalDto): Promise<WalletTransaction> {
        const response = await apiClient.post<WalletTransaction>(`/wallets/${username}/withdraw`, dto);
        return response.data;
    },
};

export const adminPaymentService = {
    async getPendingWithdrawals(): Promise<AdminWithdrawal[]> {
        const response = await apiClient.get<AdminWithdrawal[]>('/admin/payments/withdrawals/pending');
        return response.data;
    },

    async approveWithdrawal(id: string, externalTransactionId?: string): Promise<void> {
        await apiClient.post(`/admin/payments/withdrawals/${id}/approve`, {
            externalTransactionId,
        });
    },

    async rejectWithdrawal(id: string, reason: string): Promise<void> {
        await apiClient.post(`/admin/payments/withdrawals/${id}/reject`, { reason });
    },

    async getStatistics(): Promise<PaymentStatistics> {
        const response = await apiClient.get<PaymentStatistics>('/admin/payments/statistics');
        return response.data;
    },
};
