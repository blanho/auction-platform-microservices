import { AUTH_STATUS } from '@/modules/auth/constants/auth-status'
import { getErrorMessage } from '@/services/http'
import { authApi } from '@/modules/auth/api'
import type {
  AuthResponse,
  AuthStatus,
  AuthUser,
  LoginRequest,
  RegisterRequest,
  TwoFactorLoginRequest,
} from '@/modules/auth/types'
import {
  clearAuthStorage,
  getStoredUser,
  removeStoredUser,
  setAccessToken,
  setStoredUser,
  shouldRefreshToken,
} from '@/modules/auth/utils/token.utils'
import { signalRService } from '@/services/signalr'
import type { ReactNode } from 'react'
import { useCallback, useEffect, useRef, useState } from 'react'
import { AuthContext } from '../context/AuthContext'

interface AuthProviderProps {
  children: ReactNode
}

const REFRESH_INTERVAL_MS = 4 * 60 * 1000

function extractUserFromResponse(response: AuthResponse): AuthUser {
  return {
    id: response.userId,
    userId: response.userId,
    email: response.email,
    username: response.username,
    displayName: response.username,
    roles: response.roles,
  }
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser())
  const [status, setStatus] = useState<AuthStatus>(AUTH_STATUS.IDLE)
  const [error, setError] = useState<string | null>(null)

  const refreshTimerRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const refreshPromiseRef = useRef<Promise<boolean> | null>(null)
  const isMountedRef = useRef(true)

  const clearError = useCallback(() => setError(null), [])

  const handleLogout = useCallback(() => {
    if (refreshTimerRef.current) {
      clearInterval(refreshTimerRef.current)
      refreshTimerRef.current = null
    }
    void signalRService.disconnect()
    clearAuthStorage()
    setUser(null)
    setStatus(AUTH_STATUS.UNAUTHENTICATED)
  }, [])

  const silentRefresh = useCallback(async (): Promise<boolean> => {
    if (refreshPromiseRef.current) {
      return refreshPromiseRef.current
    }

    const refreshPromise = (async (): Promise<boolean> => {
      try {
        const response = await authApi.refreshToken()
        if (!isMountedRef.current) {
          return false
        }
        setAccessToken(response.accessToken, response.expiresIn)
        return true
      } catch {
        if (isMountedRef.current) {
          handleLogout()
        }
        return false
      }
    })()

    refreshPromiseRef.current = refreshPromise
    try {
      return await refreshPromise
    } finally {
      if (refreshPromiseRef.current === refreshPromise) {
        refreshPromiseRef.current = null
      }
    }
  }, [handleLogout])

  const startRefreshTimer = useCallback(() => {
    if (refreshTimerRef.current) {
      clearInterval(refreshTimerRef.current)
    }

    refreshTimerRef.current = setInterval(() => {
      if (shouldRefreshToken()) {
        void silentRefresh()
      }
    }, REFRESH_INTERVAL_MS)
  }, [silentRefresh])

  const handleAuthSuccess = useCallback(
    async (response: AuthResponse): Promise<void> => {
      const authUser = extractUserFromResponse(response)
      setAccessToken(response.accessToken, response.expiresIn)
      setUser(authUser)
      setStoredUser(authUser)
      setStatus(AUTH_STATUS.AUTHENTICATED)
      startRefreshTimer()
      await signalRService.connect()
    },
    [startRefreshTimer]
  )

  const refreshUser = useCallback(async () => {
    try {
      const userData = await authApi.getCurrentUser()
      if (!isMountedRef.current) {
        return
      }

      setUser(userData)
      setStoredUser(userData)
      setStatus(AUTH_STATUS.AUTHENTICATED)
      startRefreshTimer()

      await signalRService.connect()
    } catch {
      if (isMountedRef.current) {
        handleLogout()
      }
    }
  }, [startRefreshTimer, handleLogout])

  const initializeAuth = useCallback(async () => {
    setStatus(AUTH_STATUS.LOADING)

    try {
      const refreshed = await silentRefresh()
      if (!isMountedRef.current) {
        return
      }

      if (refreshed) {
        await refreshUser()
      } else {
        setStatus(AUTH_STATUS.UNAUTHENTICATED)
        removeStoredUser()
      }
    } catch {
      if (isMountedRef.current) {
        setStatus(AUTH_STATUS.UNAUTHENTICATED)
        removeStoredUser()
      }
    }
  }, [silentRefresh, refreshUser])

  useEffect(() => {
    isMountedRef.current = true
    void initializeAuth()

    return () => {
      isMountedRef.current = false
      if (refreshTimerRef.current) {
        clearInterval(refreshTimerRef.current)
      }
    }
  }, [initializeAuth])

  const login = async (data: LoginRequest) => {
    setError(null)
    setStatus(AUTH_STATUS.LOADING)

    try {
      const response = await authApi.login(data)

      if (response.requiresTwoFactor) {
        setStatus(AUTH_STATUS.UNAUTHENTICATED)
        return { requiresTwoFactor: true, twoFactorStateToken: response.twoFactorStateToken }
      }

      await handleAuthSuccess(response)
      return {}
    } catch (err) {
      setStatus(AUTH_STATUS.UNAUTHENTICATED)
      const message = getErrorMessage(err)
      setError(message)
      throw err
    }
  }

  const loginWith2FA = async (data: TwoFactorLoginRequest) => {
    setError(null)
    setStatus(AUTH_STATUS.LOADING)

    try {
      const response = await authApi.loginWith2FA(data)
      await handleAuthSuccess(response)
    } catch (err) {
      setStatus(AUTH_STATUS.UNAUTHENTICATED)
      const message = getErrorMessage(err)
      setError(message)
      throw err
    }
  }

  const register = async (data: RegisterRequest) => {
    setError(null)
    setStatus(AUTH_STATUS.LOADING)

    try {
      const response = await authApi.register(data)
      await handleAuthSuccess(response)
    } catch (err) {
      setStatus(AUTH_STATUS.UNAUTHENTICATED)
      const message = getErrorMessage(err)
      setError(message)
      throw err
    }
  }

  const logout = async () => {
    try {
      await authApi.logout()
    } finally {
      handleLogout()
    }
  }

  const logoutAll = async () => {
    try {
      await authApi.logoutAll()
    } finally {
      handleLogout()
    }
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        status,
        isAuthenticated: status === AUTH_STATUS.AUTHENTICATED && !!user,
        isLoading: status === AUTH_STATUS.LOADING || status === AUTH_STATUS.IDLE,
        error,
        login,
        loginWith2FA,
        register,
        logout,
        logoutAll,
        refreshUser,
        silentRefresh,
        clearError,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
