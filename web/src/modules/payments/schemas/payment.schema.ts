import { z } from 'zod'

export const depositSchema = z.object({
  amount: z.number().min(1, 'Minimum deposit is $1').max(10000, 'Maximum deposit is $10,000'),
  paymentMethod: z.string().min(1, 'Payment method is required'),
  description: z.string().max(500, 'Description must be 500 characters or less').optional(),
})

export const withdrawSchema = z.object({
  amount: z.number().min(10, 'Minimum withdrawal is $10'),
  paymentMethod: z.string().min(1, 'Payment method is required'),
  description: z.string().max(500, 'Description must be 500 characters or less').optional(),
})

export const shippingAddressSchema = z.object({
  fullName: z.string().min(1, 'Full name is required').max(100, 'Name too long'),
  addressLine1: z.string().min(1, 'Address is required').max(200, 'Address too long'),
  addressLine2: z.string().max(200, 'Address too long').optional(),
  city: z.string().min(1, 'City is required').max(100, 'City name too long'),
  state: z.string().min(1, 'State is required').max(100, 'State name too long'),
  postalCode: z.string().min(1, 'Postal code is required').max(20, 'Postal code too long'),
  country: z.string().min(1, 'Country is required').max(100, 'Country name too long'),
  phone: z.string().max(20, 'Phone number too long').optional(),
})

export const createOrderSchema = z.object({
  auctionId: z.string().uuid('Invalid auction ID'),
  shippingAddress: shippingAddressSchema,
  buyerNotes: z.string().max(1000, 'Notes must be 1000 characters or less').optional(),
})

export type DepositFormData = z.infer<typeof depositSchema>
export type WithdrawFormData = z.infer<typeof withdrawSchema>
export type ShippingAddressFormData = z.infer<typeof shippingAddressSchema>
export type CreateOrderFormData = z.infer<typeof createOrderSchema>
