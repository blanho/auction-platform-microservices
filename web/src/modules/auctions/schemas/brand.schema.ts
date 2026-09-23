import type { TFunction } from 'i18next'
import { z } from 'zod'

export const createBrandSchema = (t: TFunction<'auctions'>) =>
  z.object({
    name: z.string().min(2, t('validation.nameRequired')),
    description: z.string().optional(),
  })

export type BrandFormData = z.infer<ReturnType<typeof createBrandSchema>>
