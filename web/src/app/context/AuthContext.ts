import type {
  AuthStatus,
  AuthUser,
  LoginRequest,
  RegisterRequest,
  TwoFactorLoginRequest,
} from '@/modules/auth/types'
import { createContext } from 'react'

export interface AuthContextType {
  user: AuthUser | null
  status: AuthStatus
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null

  login: (
    data: LoginRequest
  ) => Promise<{ requiresTwoFactor?: boolean; twoFactorStateToken?: string }>
  loginWith2FA: (data: TwoFactorLoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  logoutAll: () => Promise<void>
  refreshUser: () => Promise<void>
  silentRefresh: () => Promise<boolean>
  clearError: () => void
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)
