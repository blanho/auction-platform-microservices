import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { QueryProvider, ThemeProvider, AuthProvider, PermissionProvider } from '@/app/providers'
import { router } from '@/app/router'
import './i18n'
import './index.css'

const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('Root element not found')
}

createRoot(rootElement).render(
  <StrictMode>
    <QueryProvider>
      <ThemeProvider>
        <AuthProvider>
          <PermissionProvider>
            <RouterProvider router={router} />
          </PermissionProvider>
        </AuthProvider>
      </ThemeProvider>
    </QueryProvider>
  </StrictMode>
)
