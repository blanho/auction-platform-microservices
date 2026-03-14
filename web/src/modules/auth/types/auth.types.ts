export type AuthStatus = 'idle' | 'loading' | 'authenticated' | 'unauthenticated'

export interface AuthState {
  status: AuthStatus
  user: AuthUser | null
  accessToken: string | null
  error: AuthError | null
}

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

export interface AuthError {
  code: string
  message: string
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
