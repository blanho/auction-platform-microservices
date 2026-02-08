import { Outlet, useLocation } from 'react-router-dom'
import { Box } from '@mui/material'
import { LandingHeader } from './LandingHeader'
import { Footer } from './Footer'
import { PromoBanner } from '../navigation/PromoBanner'
import { palette } from '@/shared/theme/tokens'

export const LandingLayout = () => {
  const location = useLocation()
  const isLandingPage = location.pathname === '/'

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
