import { createContext } from 'react'
import type {
  AuthUser,
  AuthStatus,
  LoginRequest,
  RegisterRequest,
  TwoFactorLoginRequest,
} from '@/modules/auth/types'

export interface AuthContextType {
  user: AuthUser | null
  status: AuthStatus
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null

  login: (data: LoginRequest) => Promise<{ requiresTwoFactor?: boolean; twoFactorToken?: string }>
  loginWith2FA: (data: TwoFactorLoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  logoutAll: () => Promise<void>
  refreshUser: () => Promise<void>
  silentRefresh: () => Promise<boolean>
  clearError: () => void
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)
