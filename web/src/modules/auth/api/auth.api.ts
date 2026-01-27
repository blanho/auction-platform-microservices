import { http } from '@/services/http'
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  TwoFactorLoginRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ConfirmEmailRequest,
  TokenResponse,
  AuthUser,
  TwoFactorSetupResponse,
  TwoFactorStatusResponse,
} from '../types'

const BASE_URL = '/api/auth'

function mapResponseToUser(response: AuthResponse): AuthUser {
  return {
    id: response.userId,
    userId: response.userId,
    email: response.email,
    username: response.username,
    displayName: response.username,
    roles: response.roles,
  }
}

export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await http.post<AuthResponse>(`${BASE_URL}/login`, data)
    return response.data
  },

  loginWith2FA: async (data: TwoFactorLoginRequest): Promise<AuthResponse> => {
    const response = await http.post<AuthResponse>(`${BASE_URL}/login-2fa`, data)
    return response.data
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await http.post<AuthResponse>(`${BASE_URL}/register`, data)
    return response.data
  },

  confirmEmail: async (data: ConfirmEmailRequest): Promise<void> => {
    await http.post(`${BASE_URL}/confirm-email`, data)
  },

  resendConfirmation: async (email: string): Promise<void> => {
    await http.post(`${BASE_URL}/resend-confirmation`, { email })
  },

  forgotPassword: async (data: ForgotPasswordRequest): Promise<void> => {
    await http.post(`${BASE_URL}/forgot-password`, data)
  },

  resetPassword: async (data: ResetPasswordRequest): Promise<void> => {
    await http.post(`${BASE_URL}/reset-password`, data)
  },

  refreshToken: async (): Promise<TokenResponse> => {
    const response = await http.post<TokenResponse>(`${BASE_URL}/refresh`)
    return response.data
  },

  logout: async (): Promise<void> => {
    await http.post(`${BASE_URL}/logout`)
  },

  logoutAll: async (): Promise<void> => {
    await http.post(`${BASE_URL}/logout-all`)
  },

  getCurrentUser: async (): Promise<AuthUser> => {
    const response = await http.get<AuthUser>(`${BASE_URL}/me`)
    return response.data
  },

  checkUsernameAvailable: async (username: string): Promise<boolean> => {
    const response = await http.get<boolean>(`${BASE_URL}/check-username/${username}`)
    return response.data
  },

  checkEmailAvailable: async (email: string): Promise<boolean> => {
    const response = await http.get<boolean>(`${BASE_URL}/check-email/${encodeURIComponent(email)}`)
    return response.data
  },

  get2FAStatus: async (): Promise<TwoFactorStatusResponse> => {
    const response = await http.get<TwoFactorStatusResponse>(`${BASE_URL}/2fa/status`)
    return response.data
  },

  setup2FA: async (): Promise<TwoFactorSetupResponse> => {
    const response = await http.post<TwoFactorSetupResponse>(`${BASE_URL}/2fa/setup`)
    return response.data
  },

  enable2FA: async (code: string): Promise<string[]> => {
    const response = await http.post<{ recoveryCodes: string[] }>(`${BASE_URL}/2fa/enable`, { code })
    return response.data.recoveryCodes
  },

  disable2FA: async (password: string): Promise<void> => {
    await http.post(`${BASE_URL}/2fa/disable`, { password })
  },

  generateRecoveryCodes: async (): Promise<string[]> => {
    const response = await http.post<{ recoveryCodes: string[] }>(`${BASE_URL}/2fa/generate-codes`)
    return response.data.recoveryCodes
  },

  forgetBrowser: async (): Promise<void> => {
    await http.post(`${BASE_URL}/2fa/forget-browser`)
  },

  exchangeOAuthCode: async (code: string): Promise<AuthResponse> => {
    const response = await http.post<AuthResponse>(`${BASE_URL}/exchange-code`, { code })
    return response.data
  },

  mapResponseToUser,
}
