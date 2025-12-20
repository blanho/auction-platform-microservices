export enum OrderStatus {
    PendingPayment = 'PendingPayment',
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
    Processing = 'Processing',
    Completed = 'Completed',
    Failed = 'Failed',
    Refunded = 'Refunded',
}

export interface Order {
    id: string;
    auctionId: string;
    buyerId: string;
    buyerUsername: string;
    sellerId: string;
    sellerUsername: string;
    itemTitle: string;
    itemImageUrl?: string;
    winningBid: number;
    totalAmount: number;
    shippingCost?: number;
    platformFee?: number;
    status: OrderStatus;
    paymentStatus: PaymentStatus;
    paymentTransactionId?: string;
    shippingAddress?: string;
    trackingNumber?: string;
    shippingCarrier?: string;
    paidAt?: string;
    shippedAt?: string;
    deliveredAt?: string;
    buyerNotes?: string;
    sellerNotes?: string;
    createdAt: string;
    updatedAt?: string;
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
