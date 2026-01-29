import type { AuthUser } from '../types'

let inMemoryAccessToken: string | null = null
let tokenExpiresAt: number | null = null

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

export function isAccessTokenExpired(): boolean {
  if (!inMemoryAccessToken || !tokenExpiresAt) {return true}
  return Date.now() >= tokenExpiresAt
}

export function shouldRefreshToken(thresholdMs = 60 * 1000): boolean {
  if (!inMemoryAccessToken || !tokenExpiresAt) {return true}
  return Date.now() >= tokenExpiresAt - thresholdMs
}

export function getTokenExpirationTime(token: string): number | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return payload.exp * 1000
  } catch {
    return null
  }
}

export function isTokenExpired(token: string): boolean {
  const exp = getTokenExpirationTime(token)
  if (!exp) {return true}
  return Date.now() >= exp
}

export function getTokenPayload<T = Record<string, unknown>>(token: string): T | null {
  try {
    return JSON.parse(atob(token.split('.')[1])) as T
  } catch {
    return null
  }
}

const AUTH_USER_KEY = 'auction_user'

export function getStoredUser(): AuthUser | null {
  if (typeof window === 'undefined') {return null}
  const userJson = sessionStorage.getItem(AUTH_USER_KEY)
  if (!userJson) {return null}
  try {
    return JSON.parse(userJson) as AuthUser
  } catch {
    return null
  }
}

export function setStoredUser(user: AuthUser): void {
  if (typeof window === 'undefined') {return}
  sessionStorage.setItem(AUTH_USER_KEY, JSON.stringify(user))
}

export function removeStoredUser(): void {
  if (typeof window === 'undefined') {return}
  sessionStorage.removeItem(AUTH_USER_KEY)
}

export function clearAuthStorage(): void {
  clearAccessToken()
  removeStoredUser()
}
