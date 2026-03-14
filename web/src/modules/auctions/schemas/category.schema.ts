import { z } from 'zod'

export const categorySchema = z.object({
  name: z.string().min(2, 'Name is required'),
  slug: z.string().min(2, 'Slug is required'),
  description: z.string().optional(),
  parentId: z.string().optional(),
})

export type CategoryFormData = z.infer<typeof categorySchema>
