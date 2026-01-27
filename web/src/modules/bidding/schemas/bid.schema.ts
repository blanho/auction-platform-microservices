import { z } from 'zod'
import { BID_CONSTANTS } from '../constants'

export const placeBidSchema = z.object({
  auctionId: z.string().min(1, 'Auction ID is required').uuid('Invalid auction ID'),
  amount: z
    .number()
    .min(BID_CONSTANTS.MIN_BID_AMOUNT, `Bid amount must be at least $${BID_CONSTANTS.MIN_BID_AMOUNT}`)
    .finite('Bid amount must be a valid number'),
})

export const createAutoBidSchema = z
  .object({
    auctionId: z.string().min(1, 'Auction ID is required').uuid('Invalid auction ID'),
    maxAmount: z
      .number()
      .min(BID_CONSTANTS.MIN_BID_AMOUNT, `Maximum amount must be at least $${BID_CONSTANTS.MIN_BID_AMOUNT}`)
      .finite('Maximum amount must be a valid number'),
    bidIncrement: z
      .number()
      .min(BID_CONSTANTS.MIN_BID_AMOUNT, `Increment must be at least $${BID_CONSTANTS.MIN_BID_AMOUNT}`)
      .finite('Increment must be a valid number')
      .optional(),
  })
  .refine(
    (data) => !data.bidIncrement || data.bidIncrement < data.maxAmount,
    {
      message: 'Increment must be less than maximum amount',
      path: ['bidIncrement'],
    }
  )

export const updateAutoBidSchema = z
  .object({
    maxAmount: z
      .number()
      .min(BID_CONSTANTS.MIN_BID_AMOUNT, `Maximum amount must be at least $${BID_CONSTANTS.MIN_BID_AMOUNT}`)
      .finite('Maximum amount must be a valid number')
      .optional(),
    bidIncrement: z
      .number()
      .min(BID_CONSTANTS.MIN_BID_AMOUNT, `Increment must be at least $${BID_CONSTANTS.MIN_BID_AMOUNT}`)
      .finite('Increment must be a valid number')
      .optional(),
  })
  .refine(
    (data) => {
      if (data.maxAmount && data.bidIncrement) {
        return data.bidIncrement < data.maxAmount
      }
      return true
    },
    {
      message: 'Increment must be less than maximum amount',
      path: ['bidIncrement'],
    }
  )

export const retractBidSchema = z.object({
  reason: z
    .string()
    .min(10, 'Reason must be at least 10 characters')
    .max(500, 'Reason must not exceed 500 characters'),
})

export const setAutoBidSchema = z
  .object({
    auctionId: z.string().uuid('Invalid auction'),
    maxAmount: z.number().min(0.01, 'Maximum amount must be greater than 0'),
    incrementAmount: z.number().min(0.01, 'Increment must be greater than 0'),
  })
  .refine((data) => data.incrementAmount < data.maxAmount, {
    message: 'Increment must be less than maximum amount',
    path: ['incrementAmount'],
  })

export type PlaceBidFormData = z.infer<typeof placeBidSchema>
export type CreateAutoBidFormData = z.infer<typeof createAutoBidSchema>
export type UpdateAutoBidFormData = z.infer<typeof updateAutoBidSchema>
export type RetractBidFormData = z.infer<typeof retractBidSchema>
export type SetAutoBidFormData = z.infer<typeof setAutoBidSchema>
