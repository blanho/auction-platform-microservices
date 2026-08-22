import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi } from '../api'
import type {
  ConfirmEmailRequest,
  ForgotPasswordRequest,
  RegisterRequest,
  ResetPasswordRequest,
} from '../types'
import { setAccessToken, setStoredUser } from '../utils'

const authKeys = {
  all: ['auth'] as const,
  currentUser: () => [...authKeys.all, 'me'] as const,
  usernameCheck: (username: string) => [...authKeys.all, 'username', username] as const,
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

export function useCheckUsername(username: string) {
  return useQuery({
    queryKey: authKeys.usernameCheck(username),
    queryFn: () => authApi.checkUsernameAvailable(username),
    enabled: username.length >= 3,
    staleTime: 30 * 1000,
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
