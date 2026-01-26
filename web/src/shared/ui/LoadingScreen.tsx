import { Box, CircularProgress, Typography } from '@mui/material'

interface LoadingScreenProps {
  message?: string
  fullScreen?: boolean
}

export const LoadingScreen = ({ message = 'Loading...', fullScreen = true }: LoadingScreenProps) => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: fullScreen ? '100vh' : '200px',
        gap: 3,
        bgcolor: fullScreen ? '#FAFAF9' : 'transparent',
      }}
    >
      <Box sx={{ position: 'relative' }}>
        <CircularProgress
          size={56}
          thickness={2}
          sx={{
            color: '#E5E5E5',
          }}
        />
        <CircularProgress
          size={56}
          thickness={2}
          sx={{
            color: '#CA8A04',
            position: 'absolute',
            left: 0,
            animationDuration: '1.2s',
          }}
        />
      </Box>
      <Typography
        sx={{
          color: '#78716C',
          fontSize: '0.9375rem',
          fontWeight: 500,
        }}
      >
        {message}
      </Typography>
    </Box>
  )
}
