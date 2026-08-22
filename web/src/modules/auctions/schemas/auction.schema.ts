import type { TFunction } from 'i18next'
import { z } from 'zod'

const createBaseAuctionSchema = (t: TFunction<'auctions'>) =>
  z.object({
    title: z.string().min(3, t('validation.titleMin')).max(200, t('validation.titleMax')),
    description: z
      .string()
      .min(10, t('validation.descriptionMinCreate'))
      .max(2000, t('validation.descriptionMax')),
    categoryId: z.string().min(1, t('validation.categoryRequired')),
    brandId: z.string().optional(),
    condition: z.string().max(50, t('validation.conditionMax')).optional(),
    yearManufactured: z
      .number()
      .min(1900)
      .max(new Date().getFullYear() + 1)
      .optional(),
    reservePrice: z.number().min(0, t('validation.reservePriceMin')),
    buyNowPrice: z.number().optional(),
    auctionEnd: z.string().min(1, t('validation.auctionEndRequired')),
    currency: z.string().max(3, t('validation.currencyCode')).optional().default('USD'),
    isFeatured: z.boolean().optional().default(false),
  })

export const createAuctionSchema = (t: TFunction<'auctions'>) =>
  createBaseAuctionSchema(t)
    .refine(
      (data) => {
        if (data.buyNowPrice && data.buyNowPrice <= data.reservePrice) {
          return false
        }
        return true
      },
      {
        message: t('validation.buyNowGreater'),
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
        message: t('validation.auctionEndFuture'),
        path: ['auctionEnd'],
      }
    )

export const createUpdateAuctionSchema = (t: TFunction<'auctions'>) =>
  z.object({
    title: z.string().min(10, t('validation.titleMinUpdate')).max(200, t('validation.titleMax')),
    description: z
      .string()
      .min(50, t('validation.descriptionMinUpdate'))
      .max(2000, t('validation.descriptionMax')),
    categoryId: z.string().min(1, t('validation.categoryRequired')),
    condition: z.string().optional(),
    yearManufactured: z.number().min(1900).max(2100).optional(),
  })

export type CreateAuctionFormData = z.input<ReturnType<typeof createBaseAuctionSchema>>
export type UpdateAuctionFormData = z.infer<ReturnType<typeof createUpdateAuctionSchema>>
