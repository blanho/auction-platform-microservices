export interface LoginRequest {
  usernameOrEmail: string
  password: string
  rememberMe?: boolean
}

export interface RegisterRequest {
  email: string
  username: string
  password: string
}

export interface TwoFactorLoginRequest {
  twoFactorStateToken: string
  code: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  token: string
  newPassword: string
  confirmPassword: string
}

export interface ConfirmEmailRequest {
  userId: string
  token: string
}
