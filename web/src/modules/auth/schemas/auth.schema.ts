import { z } from 'zod'
import {
  PASSWORD_MIN_LENGTH,
  PASSWORD_MAX_LENGTH,
  USERNAME_MIN_LENGTH,
  USERNAME_MAX_LENGTH,
  USERNAME_REGEX,
  PASSWORD_REGEX,
  TWO_FACTOR_CODE_LENGTH,
} from '../constants'

export const loginSchema = z.object({
  usernameOrEmail: z.string().min(1, 'Email or username is required'),
  password: z.string().min(1, 'Password is required'),
  rememberMe: z.boolean().optional(),
})

export const registerSchema = z
  .object({
    email: z.string().email('Invalid email address'),
    username: z
      .string()
      .min(USERNAME_MIN_LENGTH, `Username must be at least ${USERNAME_MIN_LENGTH} characters`)
      .max(USERNAME_MAX_LENGTH, `Username must be less than ${USERNAME_MAX_LENGTH} characters`)
      .regex(USERNAME_REGEX, 'Username can only contain letters, numbers, and underscores'),
    password: z
      .string()
      .min(PASSWORD_MIN_LENGTH, `Password must be at least ${PASSWORD_MIN_LENGTH} characters`)
      .max(PASSWORD_MAX_LENGTH, `Password must be less than ${PASSWORD_MAX_LENGTH} characters`)
      .regex(PASSWORD_REGEX.uppercase, 'Password must contain at least one uppercase letter')
      .regex(PASSWORD_REGEX.lowercase, 'Password must contain at least one lowercase letter')
      .regex(PASSWORD_REGEX.number, 'Password must contain at least one number')
      .regex(PASSWORD_REGEX.specialChar, 'Password must contain at least one special character'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

export const twoFactorSchema = z.object({
  code: z
    .string()
    .length(TWO_FACTOR_CODE_LENGTH, `Code must be ${TWO_FACTOR_CODE_LENGTH} digits`)
    .regex(/^\d+$/, 'Code must contain only numbers'),
})

export const forgotPasswordSchema = z.object({
  email: z.string().email('Invalid email address'),
})

export const resetPasswordSchema = z
  .object({
    newPassword: z
      .string()
      .min(PASSWORD_MIN_LENGTH, `Password must be at least ${PASSWORD_MIN_LENGTH} characters`)
      .max(PASSWORD_MAX_LENGTH, `Password must be less than ${PASSWORD_MAX_LENGTH} characters`)
      .regex(PASSWORD_REGEX.uppercase, 'Password must contain at least one uppercase letter')
      .regex(PASSWORD_REGEX.lowercase, 'Password must contain at least one lowercase letter')
      .regex(PASSWORD_REGEX.number, 'Password must contain at least one number')
      .regex(PASSWORD_REGEX.specialChar, 'Password must contain at least one special character'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

export type LoginFormData = z.infer<typeof loginSchema>
export type RegisterFormData = z.infer<typeof registerSchema>
export type TwoFactorFormData = z.infer<typeof twoFactorSchema>
export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>
export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>
