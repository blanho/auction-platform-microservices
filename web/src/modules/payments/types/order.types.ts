import type { QueryParameters } from '@/shared/types'

export type OrderStatus =
  | 'pending'
  | 'payment_pending'
  | 'paid'
  | 'processing'
  | 'shipped'
  | 'delivered'
  | 'completed'
  | 'cancelled'
  | 'disputed'
  | 'refunded'

export type PaymentStatus =
  | 'pending'
  | 'processing'
  | 'completed'
  | 'failed'
  | 'refunded'
  | 'cancelled'

export interface OrderStats {
  totalOrders: number
  pendingOrders: number
  paidOrders: number
  processingOrders: number
  shippedOrders: number
  deliveredOrders: number
  completedOrders: number
  cancelledOrders: number
  disputedOrders: number
  refundedOrders: number
  totalRevenue: number
  averageOrderValue: number
}

export interface ShippingAddress {
  fullName: string
  addressLine1: string
  addressLine2?: string
  city: string
  state: string
  postalCode: string
  country: string
  phone?: string
}

export interface Order {
  id: string
  auctionId: string
  auctionTitle?: string
  auctionImageUrl?: string
  buyerId: string
  buyerUsername: string
  buyerName?: string
  sellerId: string
  sellerUsername: string
  sellerName?: string
  itemTitle: string
  winningBid: number
  winningBidAmount?: number
  totalAmount: number
  shippingCost?: number
  platformFee?: number
  status: OrderStatus
  paymentStatus: PaymentStatus
  paymentTransactionId?: string
  shippingAddress?: string | ShippingAddress
  trackingNumber?: string
  trackingUrl?: string
  shippingCarrier?: string
  paidAt?: string
  shippedAt?: string
  deliveredAt?: string
  completedAt?: string
  buyerNotes?: string
  sellerNotes?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateOrderRequest {
  auctionId: string
  shippingAddress: ShippingAddress
  buyerId?: string
  buyerUsername?: string
  sellerId?: string
  sellerUsername?: string
  itemTitle?: string
  winningBid?: number
  shippingCost?: number
  platformFee?: number
  buyerNotes?: string
}

export interface UpdateOrderRequest {
  status?: OrderStatus
  paymentStatus?: PaymentStatus
  paymentTransactionId?: string
  shippingAddress?: string
  trackingNumber?: string
  shippingCarrier?: string
  buyerNotes?: string
  sellerNotes?: string
}

export interface ProcessPaymentRequest {
  paymentMethod: string
  externalTransactionId?: string
}

export interface ShipOrderRequest {
  trackingNumber: string
  shippingCarrier: string
  sellerNotes?: string
}

export interface CancelOrderRequest {
  reason?: string
}

export interface OrderFilter {
  status?: OrderStatus
  paymentStatus?: PaymentStatus
  searchTerm?: string
  fromDate?: string
  toDate?: string
  buyerUsername?: string
  sellerUsername?: string
}

export interface OrderFilters extends QueryParameters {
  status?: OrderStatus
  paymentStatus?: PaymentStatus
  role?: 'buyer' | 'seller'
  searchTerm?: string
  fromDate?: string
  toDate?: string
}
