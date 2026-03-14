import { z } from 'zod'

export const brandSchema = z.object({
  name: z.string().min(2, 'Name is required'),
  slug: z.string().min(2, 'Slug is required'),
  description: z.string().optional(),
  websiteUrl: z.string().url('Invalid URL').optional().or(z.literal('')),
})

export type BrandFormData = z.infer<typeof brandSchema>
