import { Box, CircularProgress, Typography } from '@mui/material'
import { palette } from '@/shared/theme/tokens'

interface LoadingScreenProps {
  message?: string
  fullScreen?: boolean
}

export const LoadingScreen = ({
  message = 'Loading...',
  fullScreen = true,
}: LoadingScreenProps) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: fullScreen ? '100vh' : '200px',
        gap: 3,
        bgcolor: fullScreen ? palette.neutral[50] : 'transparent',
      }}
    >
      <Box sx={{ position: 'relative' }}>
        <CircularProgress
          size={56}
          thickness={2}
          sx={{
            color: palette.neutral[200],
          }}
        />
        <CircularProgress
          size={56}
          thickness={2}
          sx={{
            color: palette.brand.primary,
            position: 'absolute',
            left: 0,
            animationDuration: '1.2s',
          }}
        />
      </Box>
      <Typography
        sx={{
          color: palette.neutral[500],
          fontSize: '0.9375rem',
          fontWeight: 500,
        }}
      >
        {message}
      </Typography>
    </Box>
  )
}
