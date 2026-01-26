import { z } from 'zod'

export const analyticsQuerySchema = z.object({
  startDate: z.string().datetime().optional(),
  endDate: z.string().datetime().optional(),
  days: z.number().min(1).max(365).optional(),
  period: z.enum(['day', 'week', 'month', 'year']).optional(),
  granularity: z.enum(['hour', 'day', 'week', 'month']).optional(),
  limit: z.number().min(1).max(100).optional(),
  categoryId: z.string().uuid().optional(),
}).refine(
  (data) => {
    if (data.startDate && data.endDate) {
      const start = new Date(data.startDate)
      const end = new Date(data.endDate)
      return start < end
    }
    return true
  },
  {
    message: 'Start date must be before end date',
    path: ['endDate'],
  }
)

export const userAnalyticsQuerySchema = z.object({
  timeRange: z.enum(['7d', '30d', '90d', '1y']).optional(),
  period: z.enum(['day', 'week', 'month']).optional(),
})

export const dateRangeSchema = z.object({
  startDate: z.string().datetime(),
  endDate: z.string().datetime(),
}).refine(
  (data) => {
    const start = new Date(data.startDate)
    const end = new Date(data.endDate)
    return start < end
  },
  {
    message: 'Start date must be before end date',
    path: ['endDate'],
  }
)

export type AnalyticsQueryFormData = z.infer<typeof analyticsQuerySchema>
export type UserAnalyticsQueryFormData = z.infer<typeof userAnalyticsQuerySchema>
export type DateRangeFormData = z.infer<typeof dateRangeSchema>
