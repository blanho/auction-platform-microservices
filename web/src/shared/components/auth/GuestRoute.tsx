import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/app/providers'
import { LoadingScreen } from '@/shared/ui'
import { getRedirectUrl, clearRedirectUrl } from './redirectUrl'

interface GuestRouteProps {
  readonly children: ReactNode
  readonly redirectTo?: string
}

export function GuestRoute({ children, redirectTo = '/' }: GuestRouteProps) {
  const { isAuthenticated, isLoading, status } = useAuth()
  const location = useLocation()

  const hasCheckedAuth = status !== 'idle' && status !== 'loading'

  if (isLoading && !hasCheckedAuth) {
    return <LoadingScreen message="Checking authentication..." />
  }

  if (isAuthenticated) {
    const savedRedirect = getRedirectUrl()
    if (savedRedirect) {
      clearRedirectUrl()
      return <Navigate to={savedRedirect} replace />
    }

    const from = (location.state as { from?: { pathname: string } })?.from?.pathname
    return <Navigate to={from || redirectTo} replace />
  }

  return <>{children}</>
}
