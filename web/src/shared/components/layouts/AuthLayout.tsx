import { Link, Outlet } from 'react-router-dom'
import { Box, Container, Paper, Typography, IconButton } from '@mui/material'
import {
  Gavel as GavelIcon,
  DarkMode as DarkModeIcon,
  LightMode as LightModeIcon,
} from '@mui/icons-material'
import { useThemeMode } from '@/app/providers'

export const AuthLayout = () => {
  const { mode, toggleTheme } = useThemeMode()

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        flexDirection: 'column',
        backgroundColor: 'background.default',
      }}
    >
      <Box
        component="header"
        sx={{
          py: 2,
          px: 3,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Box
          component={Link}
          to="/"
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            textDecoration: 'none',
            color: 'text.primary',
          }}
        >
          <GavelIcon sx={{ fontSize: 32 }} />
          <Typography variant="h6" fontWeight={700}>
            Auction
          </Typography>
        </Box>

        <IconButton onClick={toggleTheme} color="inherit">
          {mode === 'dark' ? <LightModeIcon /> : <DarkModeIcon />}
        </IconButton>
      </Box>

      <Container
        maxWidth="sm"
        sx={{
          flex: 1,
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          py: 4,
        }}
      >
        <Paper
          elevation={0}
          sx={{
            p: 4,
            borderRadius: 3,
            border: 1,
            borderColor: 'divider',
          }}
        >
          <Outlet />
        </Paper>
      </Container>

      <Box
        component="footer"
        sx={{
          py: 3,
          textAlign: 'center',
        }}
      >
        <Typography variant="body2" color="text.secondary">
          © {new Date().getFullYear()} Auction Platform. All rights reserved.
        </Typography>
      </Box>
    </Box>
  )
}
