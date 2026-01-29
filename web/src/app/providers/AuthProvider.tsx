import { useState, useCallback, useEffect, useRef } from 'react'
import type { ReactNode } from 'react'
import { AuthContext } from '../context/AuthContext'
import type {
  AuthUser,
  AuthStatus,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  TwoFactorLoginRequest,
} from '@/modules/auth/types'
import { authApi } from '@/modules/auth/api'
import {
  setAccessToken,
  clearAuthStorage,
  getStoredUser,
  setStoredUser,
  removeStoredUser,
  shouldRefreshToken,
} from '@/modules/auth/utils/token.utils'
import { signalRService } from '@/services/signalr'

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
  const [status, setStatus] = useState<AuthStatus>('idle')
  const [error, setError] = useState<string | null>(null)

  const refreshTimerRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const isMountedRef = useRef(true)

  const clearError = useCallback(() => setError(null), [])

  const handleLogout = useCallback(() => {
    if (refreshTimerRef.current) {
      clearInterval(refreshTimerRef.current)
      refreshTimerRef.current = null
    }
    signalRService.disconnect()
    clearAuthStorage()
    setUser(null)
    setStatus('unauthenticated')
  }, [])

  const silentRefresh = useCallback(async (): Promise<boolean> => {
    try {
      const response = await authApi.refreshToken()
      if (!isMountedRef.current) {return false}
      setAccessToken(response.accessToken, response.expiresIn)
      return true
    } catch {
      if (isMountedRef.current) {
        handleLogout()
      }
      return false
    }
  }, [handleLogout])

  const startRefreshTimer = useCallback(() => {
    if (refreshTimerRef.current) {
      clearInterval(refreshTimerRef.current)
    }

    refreshTimerRef.current = setInterval(() => {
      if (shouldRefreshToken()) {
        silentRefresh()
      }
    }, REFRESH_INTERVAL_MS)
  }, [silentRefresh])

  const refreshUser = useCallback(async () => {
    try {
      const userData = await authApi.getCurrentUser()
      if (!isMountedRef.current) {return}

      setUser(userData)
      setStoredUser(userData)
      setStatus('authenticated')
      startRefreshTimer()

      await signalRService.connect()
    } catch {
      if (isMountedRef.current) {
        handleLogout()
      }
    }
  }, [startRefreshTimer, handleLogout])

  const initializeAuth = useCallback(async () => {
    setStatus('loading')

    try {
      const refreshed = await silentRefresh()
      if (!isMountedRef.current) {return}

      if (refreshed) {
        await refreshUser()
      } else {
        setStatus('unauthenticated')
        removeStoredUser()
      }
    } catch {
      if (isMountedRef.current) {
        setStatus('unauthenticated')
        removeStoredUser()
      }
    }
  }, [silentRefresh, refreshUser])

  useEffect(() => {
    isMountedRef.current = true
    initializeAuth()

    return () => {
      isMountedRef.current = false
      if (refreshTimerRef.current) {clearInterval(refreshTimerRef.current)}
    }
  }, [initializeAuth])

  const login = async (data: LoginRequest) => {
    setError(null)
    setStatus('loading')

    try {
      const response = await authApi.login(data)

      if (response.requiresTwoFactor) {
        setStatus('unauthenticated')
        return { requiresTwoFactor: true, twoFactorStateToken: response.twoFactorStateToken }
      }

      const authUser = extractUserFromResponse(response)
      setAccessToken(response.accessToken, response.expiresIn)
      setUser(authUser)
      setStoredUser(authUser)
      setStatus('authenticated')
      startRefreshTimer()

      await signalRService.connect()

      return {}
    } catch (err) {
      setStatus('unauthenticated')
      const message = err instanceof Error ? err.message : 'Login failed'
      setError(message)
      throw err
    }
  }

  const loginWith2FA = async (data: TwoFactorLoginRequest) => {
    setError(null)
    setStatus('loading')

    try {
      const response = await authApi.loginWith2FA(data)

      const authUser = extractUserFromResponse(response)
      setAccessToken(response.accessToken, response.expiresIn)
      setUser(authUser)
      setStoredUser(authUser)
      setStatus('authenticated')
      startRefreshTimer()

      await signalRService.connect()
    } catch (err) {
      setStatus('unauthenticated')
      const message = err instanceof Error ? err.message : '2FA verification failed'
      setError(message)
      throw err
    }
  }

  const register = async (data: RegisterRequest) => {
    setError(null)
    setStatus('loading')

    try {
      const response = await authApi.register(data)

      const authUser = extractUserFromResponse(response)
      setAccessToken(response.accessToken, response.expiresIn)
      setUser(authUser)
      setStoredUser(authUser)
      setStatus('authenticated')
      startRefreshTimer()

      await signalRService.connect()
    } catch (err) {
      setStatus('unauthenticated')
      const message = err instanceof Error ? err.message : 'Registration failed'
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
        isAuthenticated: status === 'authenticated' && !!user,
        isLoading: status === 'loading' || status === 'idle',
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
