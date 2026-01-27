import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/app/providers'
import { usePermissions } from '@/app/providers'
import type { Permission } from '@/shared/permissions'
import { LoadingScreen } from '@/shared/ui'
import { saveRedirectUrl } from './redirectUrl'

interface ProtectedRouteProps {
  children: ReactNode
  permissions?: Permission[]
  requireAll?: boolean
  redirectTo?: string
  fallback?: ReactNode
}

export const ProtectedRoute = ({
  children,
  permissions = [],
  requireAll = false,
  redirectTo = '/login',
  fallback,
}: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading, status } = useAuth()
  const { canAny, canAll } = usePermissions()
  const location = useLocation()

  const hasCheckedAuth = status !== 'idle' && status !== 'loading'

  if (isLoading && !hasCheckedAuth) {
    return fallback ?? <LoadingScreen message="Verifying authentication..." />
  }

  if (status === 'loading' && hasCheckedAuth) {
    return <>{children}</>
  }

  if (!isAuthenticated) {
    saveRedirectUrl(location.pathname + location.search)
    return <Navigate to={redirectTo} state={{ from: location }} replace />
  }

  if (permissions.length > 0) {
    const hasRequiredPermissions = requireAll
      ? canAll(permissions)
      : canAny(permissions)

    if (!hasRequiredPermissions) {
      return <Navigate to="/unauthorized" replace />
    }
  }

  return <>{children}</>
}
