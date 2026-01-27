import { z } from 'zod'

export const depositSchema = z.object({
  amount: z.number().min(1, 'Minimum deposit is $1').max(10000, 'Maximum deposit is $10,000'),
  paymentMethod: z.string().min(1, 'Payment method is required'),
  description: z.string().optional(),
})

export const withdrawSchema = z.object({
  amount: z.number().min(10, 'Minimum withdrawal is $10'),
  paymentMethod: z.string().min(1, 'Payment method is required'),
  description: z.string().optional(),
})

export type DepositFormData = z.infer<typeof depositSchema>
export type WithdrawFormData = z.infer<typeof withdrawSchema>
