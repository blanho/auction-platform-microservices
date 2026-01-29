import { RouterProvider } from 'react-router-dom'
import { router } from './router'
import { QueryProvider, ThemeProvider, AuthProvider, PermissionProvider } from './providers'
import { useSignalRNotifications } from '@/modules/notifications/hooks'
import { ErrorBoundary } from '@/shared/components/errors'

function AppContent() {
  useSignalRNotifications()

  return <RouterProvider router={router} />
}

export function App() {
  return (
    <ErrorBoundary>
      <QueryProvider>
        <ThemeProvider>
          <AuthProvider>
            <PermissionProvider>
              <AppContent />
            </PermissionProvider>
          </AuthProvider>
        </ThemeProvider>
      </QueryProvider>
    </ErrorBoundary>
  )
}
