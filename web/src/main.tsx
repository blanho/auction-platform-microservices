import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { QueryProvider, ThemeProvider, AuthProvider, PermissionProvider } from '@/app/providers'
import { router } from '@/app/router'
import './i18n'
import './index.css'

createRoot(document.getElementById('root')!).render(
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
  </StrictMode>,
)
