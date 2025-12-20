import apiClient from '@/lib/api/axios';
import {
    Order,
    OrderStatus,
    PaymentStatus,
    OrderSummary,
    UpdateOrderStatusDto,
    ShipOrderDto,
} from '@/types/order';

export type { Order, OrderSummary, UpdateOrderStatusDto, ShipOrderDto } from '@/types/order';
export { OrderStatus, PaymentStatus } from '@/types/order';

export const orderService = {
    async getOrder(id: string): Promise<Order> {
        const response = await apiClient.get<Order>(`/orders/${id}`);
        return response.data;
    },

    async getOrderByAuction(auctionId: string): Promise<Order | null> {
        try {
            const response = await apiClient.get<Order>(`/orders/auction/${auctionId}`);
            return response.data;
        } catch {
            return null;
        }
    },

    async getMyPurchases(username: string): Promise<Order[]> {
        const response = await apiClient.get<Order[]>(`/orders/buyer/${username}`);
        return response.data;
    },

    async getMySales(username: string): Promise<Order[]> {
        const response = await apiClient.get<Order[]>(`/orders/seller/${username}`);
        return response.data;
    },

    async getOrderSummary(): Promise<OrderSummary> {
        const response = await apiClient.get<OrderSummary>('/orders/summary');
        return response.data;
    },

    async updateOrder(id: string, dto: UpdateOrderStatusDto): Promise<Order> {
        const response = await apiClient.put<Order>(`/orders/${id}`, dto);
        return response.data;
    },

    async markAsShipped(id: string, dto: ShipOrderDto): Promise<Order> {
        const response = await apiClient.put<Order>(`/orders/${id}`, {
            status: OrderStatus.Shipped,
            trackingNumber: dto.trackingNumber,
            shippingCarrier: dto.carrier,
        });
        return response.data;
    },

    async markAsDelivered(id: string): Promise<Order> {
        const response = await apiClient.put<Order>(`/orders/${id}`, {
            status: OrderStatus.Delivered,
        });
        return response.data;
    },

    async confirmDelivery(id: string): Promise<Order> {
        const response = await apiClient.put<Order>(`/orders/${id}`, {
            status: OrderStatus.Delivered,
        });
        return response.data;
    },
};

export const getOrderStatusColor = (status: OrderStatus) => {
    const colors: Record<OrderStatus, { bg: string; text: string }> = {
        [OrderStatus.PendingPayment]: { bg: 'bg-yellow-100 dark:bg-yellow-900/20', text: 'text-yellow-800 dark:text-yellow-200' },
        [OrderStatus.PaymentReceived]: { bg: 'bg-blue-100 dark:bg-blue-900/20', text: 'text-blue-800 dark:text-blue-200' },
        [OrderStatus.Processing]: { bg: 'bg-indigo-100 dark:bg-indigo-900/20', text: 'text-indigo-800 dark:text-indigo-200' },
        [OrderStatus.Shipped]: { bg: 'bg-purple-100 dark:bg-purple-900/20', text: 'text-purple-800 dark:text-purple-200' },
        [OrderStatus.Delivered]: { bg: 'bg-green-100 dark:bg-green-900/20', text: 'text-green-800 dark:text-green-200' },
        [OrderStatus.Completed]: { bg: 'bg-green-100 dark:bg-green-900/20', text: 'text-green-800 dark:text-green-200' },
        [OrderStatus.Cancelled]: { bg: 'bg-gray-100 dark:bg-gray-900/20', text: 'text-gray-800 dark:text-gray-200' },
        [OrderStatus.Disputed]: { bg: 'bg-red-100 dark:bg-red-900/20', text: 'text-red-800 dark:text-red-200' },
        [OrderStatus.Refunded]: { bg: 'bg-orange-100 dark:bg-orange-900/20', text: 'text-orange-800 dark:text-orange-200' },
    };
    return colors[status] || colors[OrderStatus.PendingPayment];
};

export const getStatusColor = (status: OrderStatus): string => {
    const color = getOrderStatusColor(status);
    return `${color.bg} ${color.text}`;
};

export const getOrderStatusLabel = (status: OrderStatus): string => {
    const labels: Record<OrderStatus, string> = {
        [OrderStatus.PendingPayment]: 'Pending Payment',
        [OrderStatus.PaymentReceived]: 'Payment Received',
        [OrderStatus.Processing]: 'Processing',
        [OrderStatus.Shipped]: 'Shipped',
        [OrderStatus.Delivered]: 'Delivered',
        [OrderStatus.Completed]: 'Completed',
        [OrderStatus.Cancelled]: 'Cancelled',
        [OrderStatus.Disputed]: 'Disputed',
        [OrderStatus.Refunded]: 'Refunded',
    };
    return labels[status] || status;
};

export const getStatusLabel = getOrderStatusLabel;

export const getPaymentStatusColor = (status: PaymentStatus): string => {
    const colors: Record<PaymentStatus, { bg: string; text: string }> = {
        [PaymentStatus.Pending]: { bg: 'bg-yellow-100 dark:bg-yellow-900/20', text: 'text-yellow-800 dark:text-yellow-200' },
        [PaymentStatus.Processing]: { bg: 'bg-blue-100 dark:bg-blue-900/20', text: 'text-blue-800 dark:text-blue-200' },
        [PaymentStatus.Completed]: { bg: 'bg-green-100 dark:bg-green-900/20', text: 'text-green-800 dark:text-green-200' },
        [PaymentStatus.Failed]: { bg: 'bg-red-100 dark:bg-red-900/20', text: 'text-red-800 dark:text-red-200' },
        [PaymentStatus.Refunded]: { bg: 'bg-orange-100 dark:bg-orange-900/20', text: 'text-orange-800 dark:text-orange-200' },
    };
    const color = colors[status] || colors[PaymentStatus.Pending];
    return `${color.bg} ${color.text}`;
};

export const getPaymentStatusLabel = (status: PaymentStatus): string => {
    const labels: Record<PaymentStatus, string> = {
        [PaymentStatus.Pending]: 'Pending',
        [PaymentStatus.Processing]: 'Processing',
        [PaymentStatus.Completed]: 'Completed',
        [PaymentStatus.Failed]: 'Failed',
        [PaymentStatus.Refunded]: 'Refunded',
    };
    return labels[status] || status;
};
