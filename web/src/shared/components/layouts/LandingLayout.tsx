import { Outlet } from 'react-router-dom'
import { Box } from '@mui/material'
import { LandingHeader } from './LandingHeader'
import { Footer } from './Footer'
import { PromoBanner } from '../navigation/PromoBanner'

export const LandingLayout = () => {
  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <PromoBanner />
      <LandingHeader />
      <Box component="main" sx={{ flex: 1 }}>
        <Outlet />
      </Box>
      <Footer />
    </Box>
  )
}
