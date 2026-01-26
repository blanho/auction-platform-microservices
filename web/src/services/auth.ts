import { http } from './http'
import { Permission } from '@/shared/permissions'

export interface User {
  id: string
  email: string
  username: string
  displayName: string
  avatarUrl?: string
  permissions: Permission[]
  roles: string[]
  createdAt: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  username: string
  password: string
  confirmPassword: string
}

export interface AuthResponse {
  user: User
  accessToken: string
  refreshToken: string
}

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await http.post<AuthResponse>('/auth/login', data)
    return response.data
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await http.post<AuthResponse>('/auth/register', data)
    return response.data
  },

  async logout(): Promise<void> {
    await http.post('/auth/logout')
  },

  async getCurrentUser(): Promise<User> {
    const response = await http.get<User>('/auth/me')
    return response.data
  },

  async refreshToken(refreshToken: string): Promise<AuthResponse> {
    const response = await http.post<AuthResponse>('/auth/refresh', { refreshToken })
    return response.data
  },

  async forgotPassword(email: string): Promise<void> {
    await http.post('/auth/forgot-password', { email })
  },

  async resetPassword(token: string, password: string): Promise<void> {
    await http.post('/auth/reset-password', { token, password })
  },

  async verifyEmail(token: string): Promise<void> {
    await http.post('/auth/verify-email', { token })
  },

  async resendVerificationEmail(): Promise<void> {
    await http.post('/auth/resend-verification')
  },
}
