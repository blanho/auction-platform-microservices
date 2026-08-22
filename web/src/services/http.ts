import i18n, { getBackendCulture } from '@/i18n'
import { getCsrfToken } from '@/modules/auth/utils/csrf.utils'
import { clearAuthStorage, getAccessToken, setAccessToken } from '@/modules/auth/utils/token.utils'
import type { AxiosError, AxiosInstance, InternalAxiosRequestConfig } from 'axios'
import axios from 'axios'

const API_BASE_URL = (import.meta.env.VITE_API_URL || '/api').replace(/\/+$/, '')
const MAX_RETRIES = 3
const RETRY_DELAY_MS = 1000

let isRefreshing = false
let refreshSubscribers: ((token: string | null) => void)[] = []

export function getApiUrl(path: string): string {
  return `${API_BASE_URL}/${path.replace(/^\/+/, '')}`
}

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
  _retryCount?: number
}

function subscribeTokenRefresh(callback: (token: string | null) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(token: string | null) {
  refreshSubscribers.forEach((callback) => callback(token))
  refreshSubscribers = []
}

function isRetryableError(error: AxiosError): boolean {
  if (!error.response) {
    return true
  }
  const status = error.response.status
  return status >= 500 || status === 408 || status === 429
}

function shouldRetry(config: RetryableRequestConfig): boolean {
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

  private setupInterceptors(): void {
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        const token = getAccessToken()
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`
        }

        if (config.headers) {
          config.headers['Accept-Language'] = getBackendCulture()
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
        const originalRequest: RetryableRequestConfig | undefined = error.config

        if (!originalRequest) {
          return Promise.reject(error)
        }

        if (isRetryableError(error) && shouldRetry(originalRequest)) {
          originalRequest._retryCount = (originalRequest._retryCount || 0) + 1
          const baseDelay = RETRY_DELAY_MS * Math.pow(2, originalRequest._retryCount - 1)
          const jitter = baseDelay * 0.2 * Math.random()
          const backoffDelay = baseDelay + jitter
          await delay(backoffDelay)
          return this.client(originalRequest)
        }

        if (error.response?.status === 401 && !originalRequest._retry) {
          if (originalRequest.url?.includes('/auth/refresh')) {
            clearAuthStorage()
            return Promise.reject(error)
          }

          if (isRefreshing) {
            return new Promise((resolve, reject) => {
              subscribeTokenRefresh((token) => {
                if (token) {
                  originalRequest.headers.Authorization = `Bearer ${token}`
                  resolve(this.client(originalRequest))
                } else {
                  reject(error)
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
              {
                withCredentials: true,
                headers: { 'Accept-Language': getBackendCulture() },
              }
            )
            const { accessToken, expiresIn } = response.data
            setAccessToken(accessToken, expiresIn)
            onTokenRefreshed(accessToken)
            originalRequest.headers.Authorization = `Bearer ${accessToken}`
            return this.client(originalRequest)
          } catch (_refreshError) {
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

  get<T>(url: string, config?: { params?: object }) {
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

  postForm<T>(
    url: string,
    formData: FormData,
    config?: { onUploadProgress?: (event: { loaded: number; total?: number }) => void }
  ) {
    return this.client.post<T>(url, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
      ...config,
    })
  }
}

export const http = new HttpService()

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

export function isApiError(error: unknown): error is AxiosError<Record<string, unknown>> {
  return axios.isAxiosError(error) && isRecord(error.response?.data)
}

export function getErrorMessage(error: unknown): string {
  if (isApiError(error) && error.response?.data) {
    const data = error.response.data
    if (typeof data.message === 'string' && data.message) {
      return data.message
    }
    if (typeof data.detail === 'string' && data.detail) {
      return data.detail
    }
    if (typeof data.title === 'string' && data.title) {
      return data.title
    }
  }
  if (error instanceof Error) {
    return error.message
  }
  return i18n.t('errors.unexpected')
}
