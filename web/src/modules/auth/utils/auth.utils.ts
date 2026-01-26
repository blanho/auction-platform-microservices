import { AUTH_ROUTES, PROTECTED_ROUTES, ADMIN_ROUTES, SELLER_ROUTES } from '../constants'
import type { AuthUser } from '../types'

export function isAuthenticated(user: AuthUser | null): boolean {
  return user !== null
}

export function hasRole(user: AuthUser | null, role: string): boolean {
  if (!user) return false
  return user.roles.some((r) => r.toLowerCase() === role.toLowerCase())
}

export function isAdmin(user: AuthUser | null): boolean {
  return hasRole(user, 'admin')
}

export function isSeller(user: AuthUser | null): boolean {
  return hasRole(user, 'seller')
}

export function isProtectedRoute(pathname: string): boolean {
  return PROTECTED_ROUTES.some((route) => pathname.startsWith(route))
}

export function isAdminRoute(pathname: string): boolean {
  return ADMIN_ROUTES.some((route) => pathname.startsWith(route))
}

export function isSellerRoute(pathname: string): boolean {
  return SELLER_ROUTES.some((route) => pathname.startsWith(route))
}

export function isAuthRoute(pathname: string): boolean {
  return Object.values(AUTH_ROUTES).some((route) => pathname.startsWith(route))
}

export function canAccessRoute(user: AuthUser | null, pathname: string): boolean {
  if (isAuthRoute(pathname)) return true
  if (!isAuthenticated(user)) return !isProtectedRoute(pathname)
  if (isAdminRoute(pathname)) return isAdmin(user)
  if (isSellerRoute(pathname)) return isSeller(user)
  return true
}

export function getDefaultRedirect(user: AuthUser | null): string {
  if (!user) return AUTH_ROUTES.LOGIN
  if (isAdmin(user)) return '/admin'
  if (isSeller(user)) return '/seller/dashboard'
  return '/'
}

export function getUserDisplayName(user: AuthUser): string {
  return user.fullName || user.username || user.email.split('@')[0]
}

export function getUserInitials(user: AuthUser): string {
  const name = getUserDisplayName(user)
  const parts = name.split(' ')
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
  }
  return name.slice(0, 2).toUpperCase()
}
