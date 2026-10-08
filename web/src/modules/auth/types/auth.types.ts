import type { AUTH_STATUS } from '@/modules/auth/constants/auth-status'
export type AuthStatus = (typeof AUTH_STATUS)[keyof typeof AUTH_STATUS]

export interface AuthUser {
  id: string
  userId: string
  email: string
  username: string
  displayName: string
  fullName?: string
  avatarUrl?: string
  roles: string[]
}

export interface AuthResponse {
  userId: string
  username: string
  email: string
  roles: string[]
  accessToken: string
  refreshToken?: string
  expiresIn: number
  requiresTwoFactor: boolean
  twoFactorStateToken?: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken?: string
  expiresIn: number
}

export interface TwoFactorStatusResponse {
  isEnabled: boolean
  hasAuthenticator: boolean
  recoveryCodesLeft: number
}

export interface TwoFactorSetupResponse {
  sharedKey: string
  authenticatorUri: string
  qrCodeBase64: string
}
