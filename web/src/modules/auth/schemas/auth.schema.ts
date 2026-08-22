import type { TFunction } from 'i18next'
import { z } from 'zod'
import {
  PASSWORD_MAX_LENGTH,
  PASSWORD_MIN_LENGTH,
  PASSWORD_REGEX,
  USERNAME_MAX_LENGTH,
  USERNAME_MIN_LENGTH,
  USERNAME_REGEX,
} from '../constants'

export const createLoginSchema = (t: TFunction<'auth'>) =>
  z.object({
    usernameOrEmail: z.string().min(1, t('validation.usernameOrEmailRequired')),
    password: z.string().min(1, t('validation.passwordRequired')),
    rememberMe: z.boolean().optional(),
  })

export const createRegisterSchema = (t: TFunction<'auth'>) =>
  z
    .object({
      email: z.string().email(t('validation.emailInvalid')),
      username: z
        .string()
        .min(USERNAME_MIN_LENGTH, t('validation.usernameMinLength', { min: USERNAME_MIN_LENGTH }))
        .max(USERNAME_MAX_LENGTH, t('validation.usernameMaxLength', { max: USERNAME_MAX_LENGTH }))
        .regex(USERNAME_REGEX, t('validation.usernameFormat')),
      password: z
        .string()
        .min(PASSWORD_MIN_LENGTH, t('validation.passwordMin', { min: PASSWORD_MIN_LENGTH }))
        .max(PASSWORD_MAX_LENGTH, t('validation.passwordMax', { max: PASSWORD_MAX_LENGTH }))
        .regex(PASSWORD_REGEX.uppercase, t('validation.passwordUppercase'))
        .regex(PASSWORD_REGEX.lowercase, t('validation.passwordLowercase'))
        .regex(PASSWORD_REGEX.number, t('validation.passwordNumber'))
        .regex(PASSWORD_REGEX.specialChar, t('validation.passwordSpecial')),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: t('validation.passwordMatch'),
      path: ['confirmPassword'],
    })
