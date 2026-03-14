import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api'
import { setAccessToken, clearAuthStorage, setStoredUser } from '../utils'
import type {
  LoginRequest,
  RegisterRequest,
  TwoFactorLoginRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ConfirmEmailRequest,
} from '../types'

export const authKeys = {
  all: ['auth'] as const,
  currentUser: () => [...authKeys.all, 'me'] as const,
  twoFactorStatus: () => [...authKeys.all, '2fa-status'] as const,
  usernameCheck: (username: string) => [...authKeys.all, 'username', username] as const,
}

export function useCurrentUser() {
  return useQuery({
    queryKey: authKeys.currentUser(),
    queryFn: authApi.getCurrentUser,
    staleTime: 5 * 60 * 1000,
    retry: false,
  })
}

export function useLogin() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      if (!response.requiresTwoFactor) {
        setAccessToken(response.accessToken, response.expiresIn)
        const user = authApi.mapResponseToUser(response)
        setStoredUser(user)
        queryClient.setQueryData(authKeys.currentUser(), user)
      }
    },
  })
}

export function useLoginWith2FA() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: TwoFactorLoginRequest) => authApi.loginWith2FA(data),
    onSuccess: (response) => {
      setAccessToken(response.accessToken, response.expiresIn)
      const user = authApi.mapResponseToUser(response)
      setStoredUser(user)
      queryClient.setQueryData(authKeys.currentUser(), user)
    },
  })
}

export function useRegister() {
  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
  })
}

export function useConfirmEmail() {
  return useMutation({
    mutationFn: (data: ConfirmEmailRequest) => authApi.confirmEmail(data),
  })
}

export function useResendConfirmation() {
  return useMutation({
    mutationFn: (email: string) => authApi.resendConfirmation(email),
  })
}

export function useForgotPassword() {
  return useMutation({
    mutationFn: (data: ForgotPasswordRequest) => authApi.forgotPassword(data),
  })
}

export function useResetPassword() {
  return useMutation({
    mutationFn: (data: ResetPasswordRequest) => authApi.resetPassword(data),
  })
}

export function useLogout() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => authApi.logout(),
    onSuccess: () => {
      clearAuthStorage()
      queryClient.setQueryData(authKeys.currentUser(), null)
      queryClient.clear()
    },
  })
}

export function useLogoutAll() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => authApi.logoutAll(),
    onSuccess: () => {
      clearAuthStorage()
      queryClient.setQueryData(authKeys.currentUser(), null)
      queryClient.clear()
    },
  })
}

export function useCheckUsername(username: string) {
  return useQuery({
    queryKey: authKeys.usernameCheck(username),
    queryFn: () => authApi.checkUsernameAvailable(username),
    enabled: username.length >= 3,
    staleTime: 30 * 1000,
  })
}

export function use2FAStatus() {
  return useQuery({
    queryKey: authKeys.twoFactorStatus(),
    queryFn: authApi.get2FAStatus,
    staleTime: 60 * 1000,
  })
}

export function useSetup2FA() {
  return useMutation({
    mutationFn: () => authApi.setup2FA(),
  })
}

export function useEnable2FA() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (code: string) => authApi.enable2FA(code),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authKeys.twoFactorStatus() })
    },
  })
}

export function useDisable2FA() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (password: string) => authApi.disable2FA(password),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: authKeys.twoFactorStatus() })
    },
  })
}

export function useGenerateRecoveryCodes() {
  return useMutation({
    mutationFn: () => authApi.generateRecoveryCodes(),
  })
}

export function useOAuthExchange() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (code: string) => authApi.exchangeOAuthCode(code),
    onSuccess: (response) => {
      setAccessToken(response.accessToken, response.expiresIn)
      const user = authApi.mapResponseToUser(response)
      setStoredUser(user)
      queryClient.setQueryData(authKeys.currentUser(), user)
    },
  })
}

export function useForgetBrowser() {
  return useMutation({
    mutationFn: () => authApi.forgetBrowser(),
  })
}
