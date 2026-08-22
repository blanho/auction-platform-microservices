import type { TFunction } from 'i18next'
import { z } from 'zod'

export const createCategorySchema = (t: TFunction<'auctions'>) =>
  z.object({
    name: z.string().min(2, t('validation.nameRequired')),
    slug: z.string().min(2, t('validation.slugRequired')),
    description: z.string().optional(),
    parentId: z.string().optional(),
  })

export type CategoryFormData = z.infer<ReturnType<typeof createCategorySchema>>
