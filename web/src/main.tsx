import {
  AuthProvider,
  PermissionProvider,
  QueryProvider,
  ThemeProvider,
  ToastProvider,
} from '@/app/providers'
import { router } from '@/app/router'
import { ErrorBoundary } from '@/shared/components/errors'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import './i18n'
import './index.css'

const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('Root element not found')
}

createRoot(rootElement).render(
  <StrictMode>
    <ErrorBoundary>
      <QueryProvider>
        <ThemeProvider>
          <ToastProvider>
            <AuthProvider>
              <PermissionProvider>
                <RouterProvider router={router} />
              </PermissionProvider>
            </AuthProvider>
          </ToastProvider>
        </ThemeProvider>
      </QueryProvider>
    </ErrorBoundary>
  </StrictMode>
)
