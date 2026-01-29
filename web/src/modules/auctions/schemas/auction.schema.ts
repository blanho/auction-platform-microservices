import { z } from 'zod'

const baseCreateAuctionSchema = z.object({
  title: z.string().min(3, 'Title must be at least 3 characters').max(200, 'Title too long'),
  description: z
    .string()
    .min(10, 'Description must be at least 10 characters')
    .max(4000, 'Description too long'),
  categoryId: z.string().min(1, 'Please select a category'),
  brandId: z.string().optional(),
  condition: z.string().optional(),
  yearManufactured: z
    .number()
    .min(1900)
    .max(new Date().getFullYear() + 1)
    .optional(),
  reservePrice: z.number().min(0, 'Reserve price must be at least $0'),
  buyNowPrice: z.number().optional(),
  auctionEnd: z.string().min(1, 'Auction end time is required'),
  currency: z.string().max(3, 'Currency must be a valid 3-letter code').optional().default('USD'),
  isFeatured: z.boolean().optional().default(false),
})

export const createAuctionSchema = baseCreateAuctionSchema
  .refine(
    (data) => {
      if (data.buyNowPrice && data.buyNowPrice <= data.reservePrice) {
        return false
      }
      return true
    },
    {
      message: 'Buy now price must be greater than reserve price',
      path: ['buyNowPrice'],
    }
  )
  .refine(
    (data) => {
      const end = new Date(data.auctionEnd)
      const now = new Date()
      const oneHourFromNow = new Date(now.getTime() + 60 * 60 * 1000) // Backend requires +1 hour
      return end > oneHourFromNow
    },
    {
      message: 'Auction end time must be at least 1 hour in the future',
      path: ['auctionEnd'],
    }
  )

export const updateAuctionSchema = z.object({
  title: z.string().min(10, 'Title must be at least 10 characters').max(200, 'Title too long'),
  description: z
    .string()
    .min(50, 'Description must be at least 50 characters')
    .max(4000, 'Description too long'),
  categoryId: z.string().min(1, 'Please select a category'),
  condition: z.string().optional(),
  yearManufactured: z.number().min(1900).max(2100).optional(),
})

export type CreateAuctionFormData = z.input<typeof baseCreateAuctionSchema>
export type UpdateAuctionFormData = z.infer<typeof updateAuctionSchema>
