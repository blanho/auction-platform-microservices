export enum OrderStatus {
    PendingPayment = 'PendingPayment',
    Paid = 'Paid',
    PaymentReceived = 'PaymentReceived',
    Processing = 'Processing',
    Shipped = 'Shipped',
    Delivered = 'Delivered',
    Completed = 'Completed',
    Cancelled = 'Cancelled',
    Disputed = 'Disputed',
    Refunded = 'Refunded',
}

export enum PaymentStatus {
    Pending = 'Pending',
    Paid = 'Paid',
    Failed = 'Failed',
    Refunded = 'Refunded',
    Cancelled = 'Cancelled',
    Completed = 'Completed',
}

export interface Order {
    id: string;
    auctionId: string;
    buyerUsername: string;
    sellerUsername: string;
    itemTitle: string;
    itemImageUrl?: string;
    winningBid: number;
    totalAmount: number;
    shippingCost?: number;
    platformFee?: number;
    status: OrderStatus;
    paymentStatus: PaymentStatus;
    trackingNumber?: string;
    shippingCarrier?: string;
    carrier?: string;
    paidAt?: string;
    shippedAt?: string;
    deliveredAt?: string;
    createdAt: string;
    hasReview?: boolean;
}

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
