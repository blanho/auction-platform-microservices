import type { TFunction } from 'i18next'
import { z } from 'zod'

export const createBrandSchema = (t: TFunction<'auctions'>) =>
  z.object({
    name: z.string().min(2, t('validation.nameRequired')),
    slug: z.string().min(2, t('validation.slugRequired')),
    description: z.string().optional(),
    websiteUrl: z.string().url(t('validation.invalidUrl')).optional().or(z.literal('')),
  })

export type BrandFormData = z.infer<ReturnType<typeof createBrandSchema>>
