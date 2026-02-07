import axios from 'axios'
import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios'
import { getAccessToken, setAccessToken, clearAuthStorage } from '@/modules/auth/utils/token.utils'
import { getCsrfToken } from '@/modules/auth/utils/csrf.utils'

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api'
const MAX_RETRIES = 3
const RETRY_DELAY_MS = 1000

let isRefreshing = false
let refreshSubscribers: ((token: string | null) => void)[] = []

function subscribeTokenRefresh(callback: (token: string | null) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(token: string | null) {
  refreshSubscribers.forEach((callback) => callback(token))
  refreshSubscribers = []
}

function isRetryableError(error: AxiosError): boolean {
  if (!error.response) {return true}
  const status = error.response.status
  return status >= 500 || status === 408 || status === 429
}

function shouldRetry(config: InternalAxiosRequestConfig & { _retryCount?: number }): boolean {
  const method = config.method?.toUpperCase()
  const isIdempotent = ['GET', 'HEAD', 'OPTIONS', 'PUT', 'DELETE'].includes(method || '')
  const retryCount = config._retryCount || 0
  return isIdempotent && retryCount < MAX_RETRIES
}

async function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

class HttpService {
  private client: AxiosInstance

  constructor() {
    this.client = axios.create({
      baseURL: API_BASE_URL,
      headers: {
        'Content-Type': 'application/json',
      },
      timeout: 30000,
      withCredentials: true,
    })

    this.setupInterceptors()
  }

  private setupInterceptors() {
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = getAccessToken()
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`
        }

        const method = config.method?.toUpperCase()
        if (method && !['GET', 'HEAD', 'OPTIONS'].includes(method)) {
          const csrfToken = getCsrfToken()
          if (csrfToken && config.headers) {
            config.headers['X-XSRF-TOKEN'] = csrfToken
          }
        }

        return config
      },
      (error) => Promise.reject(error)
    )

    this.client.interceptors.response.use(
      (response) => response,
      async (error: AxiosError) => {
        const originalRequest = error.config as InternalAxiosRequestConfig & {
          _retry?: boolean
          _retryCount?: number
        }

        if (!originalRequest) {
          return Promise.reject(error)
        }

        if (isRetryableError(error) && shouldRetry(originalRequest)) {
          originalRequest._retryCount = (originalRequest._retryCount || 0) + 1
          const backoffDelay = RETRY_DELAY_MS * Math.pow(2, originalRequest._retryCount - 1)
          await delay(backoffDelay)
          return this.client(originalRequest)
        }

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (originalRequest.url?.includes('/auth/refresh')) {
            clearAuthStorage()
            return Promise.reject(error)
          }

          if (isRefreshing) {
            return new Promise((resolve) => {
              subscribeTokenRefresh((token) => {
                if (token) {
                  originalRequest.headers.Authorization = `Bearer ${token}`
                  resolve(this.client(originalRequest))
                } else {
                  resolve(Promise.reject(error))
                }
              })
            })
          }

          originalRequest._retry = true
          isRefreshing = true

          try {
            const response = await axios.post<{ accessToken: string; expiresIn: number }>(
              `${API_BASE_URL}/auth/refresh`,
              {},
              { withCredentials: true }
            )
            const { accessToken, expiresIn } = response.data
            setAccessToken(accessToken, expiresIn)
            onTokenRefreshed(accessToken)
            originalRequest.headers.Authorization = `Bearer ${accessToken}`
            return this.client(originalRequest)
          } catch (refreshError) {
            onTokenRefreshed(null)
            clearAuthStorage()
            return Promise.reject(error)
          } finally {
            isRefreshing = false
          }
        }

        return Promise.reject(error)
      }
    )
  }

  get<T>(url: string, config?: { params?: Record<string, unknown> }) {
    return this.client.get<T>(url, config)
  }

  post<T>(url: string, data?: unknown, config?: { headers?: Record<string, string> }) {
    return this.client.post<T>(url, data, config)
  }

  put<T>(url: string, data?: unknown) {
    return this.client.put<T>(url, data)
  }

  patch<T>(url: string, data?: unknown) {
    return this.client.patch<T>(url, data)
  }

  delete<T>(url: string) {
    return this.client.delete<T>(url)
  }
}

export const http = new HttpService()

export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

export interface ApiError {
  message: string
  code: string
  details?: Record<string, string[]>
}

export function isApiError(error: unknown): error is AxiosError<ApiError | ProblemDetails> {
  return axios.isAxiosError(error)
}

export function getErrorMessage(error: unknown): string {
  if (isApiError(error) && error.response?.data) {
    const data = error.response.data as ApiError | ProblemDetails
    if ('message' in data && data.message) {
      return data.message
    }
    if ('detail' in data && data.detail) {
      return data.detail
    }
    if ('title' in data && data.title) {
      return data.title
    }
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'An unexpected error occurred'
}

export function getValidationErrors(error: unknown): Record<string, string[]> | null {
  if (isApiError(error) && error.response?.data) {
    const data = error.response.data as ApiError | ProblemDetails
    if ('details' in data && data.details) {
      return data.details
    }
    if ('errors' in data && data.errors) {
      return data.errors
    }
  }
  return null
}
