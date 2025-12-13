import apiClient from '@/lib/api/axios';

export interface Order {
    id: string;
    auctionId: string;
    buyerUsername: string;
    sellerUsername: string;
    itemTitle: string;
    winningBid: number;
    totalAmount: number;
    shippingCost?: number;
    platformFee?: number;
    status: OrderStatus;
    paymentStatus: PaymentStatus;
    trackingNumber?: string;
    shippingCarrier?: string;
    paidAt?: string;
    shippedAt?: string;
    deliveredAt?: string;
    createdAt: string;
}

export type OrderStatus =
    | 'PendingPayment'
    | 'PaymentReceived'
    | 'Processing'
    | 'Shipped'
    | 'Delivered'
    | 'Completed'
    | 'Cancelled'
    | 'Disputed'
    | 'Refunded';

export type PaymentStatus =
    | 'Pending'
    | 'Processing'
    | 'Completed'
    | 'Failed'
    | 'Refunded';

export interface OrderSummary {
    totalOrders: number;
    pendingPayment: number;
    awaitingShipment: number;
    shipped: number;
    completed: number;
    totalRevenue: number;
}

export interface UpdateOrderStatusDto {
    status: string;
    trackingNumber?: string;
    shippingCarrier?: string;
    notes?: string;
}

export interface ShipOrderDto {
    trackingNumber: string;
    carrier: string;
}

export const orderService = {
    async getOrder(id: string): Promise<Order> {
        const response = await apiClient.get<Order>(`/utility/api/v1/orders/${id}`);
        return response.data;
    },

    async getOrderByAuction(auctionId: string): Promise<Order | null> {
        try {
            const response = await apiClient.get<Order>(`/utility/api/v1/orders/auction/${auctionId}`);
            return response.data;
        } catch {
            return null;
        }
    },

    async getMyPurchases(): Promise<Order[]> {
        const response = await apiClient.get<Order[]>('/utility/api/v1/orders/purchases');
        return response.data;
    },

    async getMySales(): Promise<Order[]> {
        const response = await apiClient.get<Order[]>('/utility/api/v1/orders/sales');
        return response.data;
    },

    async getOrderSummary(): Promise<OrderSummary> {
        const response = await apiClient.get<OrderSummary>('/utility/api/v1/orders/summary');
        return response.data;
    },

    async updateOrderStatus(id: string, dto: UpdateOrderStatusDto): Promise<Order> {
        const response = await apiClient.put<Order>(`/utility/api/v1/orders/${id}/status`, dto);
        return response.data;
    },

    async markAsShipped(id: string, dto: ShipOrderDto): Promise<void> {
        await apiClient.post(`/utility/api/v1/orders/${id}/ship`, dto);
    },

    async markAsDelivered(id: string): Promise<void> {
        await apiClient.post(`/utility/api/v1/orders/${id}/delivered`);
    },
};

export const getOrderStatusColor = (status: OrderStatus) => {
    const colors: Record<OrderStatus, { bg: string; text: string }> = {
        PendingPayment: { bg: 'bg-yellow-100 dark:bg-yellow-900/20', text: 'text-yellow-800 dark:text-yellow-200' },
        PaymentReceived: { bg: 'bg-blue-100 dark:bg-blue-900/20', text: 'text-blue-800 dark:text-blue-200' },
        Processing: { bg: 'bg-indigo-100 dark:bg-indigo-900/20', text: 'text-indigo-800 dark:text-indigo-200' },
        Shipped: { bg: 'bg-purple-100 dark:bg-purple-900/20', text: 'text-purple-800 dark:text-purple-200' },
        Delivered: { bg: 'bg-green-100 dark:bg-green-900/20', text: 'text-green-800 dark:text-green-200' },
        Completed: { bg: 'bg-green-100 dark:bg-green-900/20', text: 'text-green-800 dark:text-green-200' },
        Cancelled: { bg: 'bg-gray-100 dark:bg-gray-900/20', text: 'text-gray-800 dark:text-gray-200' },
        Disputed: { bg: 'bg-red-100 dark:bg-red-900/20', text: 'text-red-800 dark:text-red-200' },
        Refunded: { bg: 'bg-orange-100 dark:bg-orange-900/20', text: 'text-orange-800 dark:text-orange-200' },
    };
    return colors[status] || colors.PendingPayment;
};
