import { useSignalRNotifications } from '@/modules/notifications/hooks'
import { palette } from '@/shared/theme/tokens'
import { Box } from '@mui/material'
import { Outlet, useLocation } from 'react-router-dom'
import { PromoBanner } from '../navigation/PromoBanner'
import { Footer } from './Footer'
import { LandingHeader } from './LandingHeader'

export const LandingLayout = () => {
  const location = useLocation()
  const isLandingPage = location.pathname === '/'
  useSignalRNotifications()

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <PromoBanner />
      <LandingHeader />
      <Box
        component="main"
        sx={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          bgcolor: isLandingPage ? 'transparent' : palette.neutral[50],
          pb: isLandingPage ? '60px' : 0,
          minHeight: isLandingPage ? 'auto' : 'calc(100vh - 200px)',
        }}
      >
        <Outlet />
      </Box>
      <Footer />
    </Box>
  )
}
