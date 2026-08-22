import type { TFunction } from 'i18next'
import { z } from 'zod'

export const createUpdateProfileSchema = (t: TFunction<'users'>) =>
  z.object({
    fullName: z.string().min(2, t('validation.fullNameMin')).max(100).optional(),
    phoneNumber: z
      .string()
      .regex(/^\+?[1-9]\d{1,14}$/, t('validation.phoneInvalid'))
      .optional()
      .or(z.literal('')),
    bio: z.string().max(500, t('validation.bioMax')).optional(),
    location: z.string().max(100, t('validation.locationMax')).optional(),
  })

export const createChangePasswordSchema = (t: TFunction<'users'>) =>
  z
    .object({
      currentPassword: z.string().min(1, t('validation.currentPasswordRequired')),
      newPassword: z
        .string()
        .min(12, t('validation.passwordMin'))
        .max(100, t('validation.passwordMax'))
        .regex(/[A-Z]/, t('validation.passwordUppercase'))
        .regex(/[a-z]/, t('validation.passwordLowercase'))
        .regex(/\d/, t('validation.passwordNumber'))
        .regex(/[!@#$%^&*(),.?":{}|<>]/, t('validation.passwordSpecial')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: t('validation.passwordMatch'),
      path: ['confirmPassword'],
    })
