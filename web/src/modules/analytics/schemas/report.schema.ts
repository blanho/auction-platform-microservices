import { z } from 'zod'
import { REPORT_STATUS, REPORT_TYPE, REPORT_PRIORITY } from '../constants'

export const createReportSchema = z
  .object({
    reportedUserId: z.string().uuid('Invalid user ID').optional(),
    reportedItemId: z.string().uuid('Invalid item ID').optional(),
    type: z.enum([
      REPORT_TYPE.FRAUD,
      REPORT_TYPE.FAKE_ITEM,
      REPORT_TYPE.NON_PAYMENT,
      REPORT_TYPE.HARASSMENT,
      REPORT_TYPE.INAPPROPRIATE_CONTENT,
      REPORT_TYPE.SUSPICIOUS_ACTIVITY,
      REPORT_TYPE.OTHER,
    ]),
    description: z
      .string()
      .min(20, 'Description must be at least 20 characters')
      .max(2000, 'Description is too long'),
    priority: z
      .enum([
        REPORT_PRIORITY.LOW,
        REPORT_PRIORITY.MEDIUM,
        REPORT_PRIORITY.HIGH,
        REPORT_PRIORITY.CRITICAL,
      ])
      .optional(),
  })
  .refine((data) => data.reportedUserId || data.reportedItemId, {
    message: 'Either reported user or reported item must be specified',
    path: ['reportedUserId'],
  })

export const updateReportStatusSchema = z.object({
  status: z.enum([
    REPORT_STATUS.PENDING,
    REPORT_STATUS.UNDER_REVIEW,
    REPORT_STATUS.RESOLVED,
    REPORT_STATUS.DISMISSED,
  ]),
  notes: z.string().max(1000, 'Notes are too long').optional(),
})

export const reportQuerySchema = z.object({
  status: z
    .enum([
      REPORT_STATUS.PENDING,
      REPORT_STATUS.UNDER_REVIEW,
      REPORT_STATUS.RESOLVED,
      REPORT_STATUS.DISMISSED,
    ])
    .optional(),
  type: z
    .enum([
      REPORT_TYPE.FRAUD,
      REPORT_TYPE.FAKE_ITEM,
      REPORT_TYPE.NON_PAYMENT,
      REPORT_TYPE.HARASSMENT,
      REPORT_TYPE.INAPPROPRIATE_CONTENT,
      REPORT_TYPE.SUSPICIOUS_ACTIVITY,
      REPORT_TYPE.OTHER,
    ])
    .optional(),
  priority: z
    .enum([
      REPORT_PRIORITY.LOW,
      REPORT_PRIORITY.MEDIUM,
      REPORT_PRIORITY.HIGH,
      REPORT_PRIORITY.CRITICAL,
    ])
    .optional(),
  reportedByUsername: z.string().optional(),
  page: z.number().min(1).optional(),
  pageSize: z.number().min(1).max(100).optional(),
})

export type CreateReportFormData = z.infer<typeof createReportSchema>
export type UpdateReportStatusFormData = z.infer<typeof updateReportStatusSchema>
export type ReportQueryFormData = z.infer<typeof reportQuerySchema>
