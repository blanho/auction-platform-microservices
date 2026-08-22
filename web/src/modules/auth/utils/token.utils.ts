import type { AuthUser } from '../types'

let inMemoryAccessToken: string | null = null
let tokenExpiresAt: number | null = null

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const encodedPayload = token.split('.')[1]
  if (!encodedPayload) {
    return null
  }

  try {
    const normalizedPayload = encodedPayload.replace(/-/g, '+').replace(/_/g, '/')
    const paddedPayload = normalizedPayload.padEnd(Math.ceil(normalizedPayload.length / 4) * 4, '=')
    const payload: unknown = JSON.parse(atob(paddedPayload))
    return isRecord(payload) ? payload : null
  } catch {
    return null
  }
}

function isAuthUser(value: unknown): value is AuthUser {
  if (!isRecord(value)) {
    return false
  }

  return (
    typeof value.id === 'string' &&
    typeof value.userId === 'string' &&
    typeof value.email === 'string' &&
    typeof value.username === 'string' &&
    typeof value.displayName === 'string' &&
    (value.fullName === undefined || typeof value.fullName === 'string') &&
    (value.avatarUrl === undefined || typeof value.avatarUrl === 'string') &&
    Array.isArray(value.roles) &&
    value.roles.every((role) => typeof role === 'string')
  )
}

export function getAccessToken(): string | null {
  return inMemoryAccessToken
}

export function setAccessToken(token: string, expiresIn?: number): void {
  inMemoryAccessToken = token
  if (expiresIn) {
    tokenExpiresAt = Date.now() + expiresIn * 1000
  } else {
    const exp = getTokenExpirationTime(token)
    tokenExpiresAt = exp
  }
}

export function clearAccessToken(): void {
  inMemoryAccessToken = null
  tokenExpiresAt = null
}

export function shouldRefreshToken(thresholdMs = 60 * 1000): boolean {
  if (!inMemoryAccessToken || !tokenExpiresAt) {
    return true
  }
  return Date.now() >= tokenExpiresAt - thresholdMs
}

export function getTokenExpirationTime(token: string): number | null {
  const payload = decodeJwtPayload(token)
  return typeof payload?.exp === 'number' && Number.isFinite(payload.exp)
    ? payload.exp * 1000
    : null
}

const AUTH_USER_KEY = 'auction_user'

export function getStoredUser(): AuthUser | null {
  if (typeof window === 'undefined') {
    return null
  }
  const userJson = sessionStorage.getItem(AUTH_USER_KEY)
  if (!userJson) {
    return null
  }
  try {
    const storedUser: unknown = JSON.parse(userJson)
    return isAuthUser(storedUser) ? storedUser : null
  } catch {
    return null
  }
}

export function setStoredUser(user: AuthUser): void {
  if (typeof window === 'undefined') {
    return
  }
  sessionStorage.setItem(AUTH_USER_KEY, JSON.stringify(user))
}

export function removeStoredUser(): void {
  if (typeof window === 'undefined') {
    return
  }
  sessionStorage.removeItem(AUTH_USER_KEY)
}

export function clearAuthStorage(): void {
  clearAccessToken()
  removeStoredUser()
}
