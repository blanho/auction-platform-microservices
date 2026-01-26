import { Box, Typography, Button, Stack } from '@mui/material'
import { Warning, Refresh, Home } from '@mui/icons-material'
import { Link } from 'react-router-dom'

interface ErrorStateProps {
  title?: string
  message?: string
  onRetry?: () => void
  showHomeButton?: boolean
}

export function ErrorState({
  title = 'Something went wrong',
  message = 'We encountered an unexpected error. Please try again.',
  onRetry,
  showHomeButton = false,
}: ErrorStateProps) {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        textAlign: 'center',
        py: 8,
        px: 4,
      }}
    >
      <Box
        sx={{
          width: 80,
          height: 80,
          borderRadius: '50%',
          bgcolor: '#FEE2E2',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
        }}
      >
        <Warning sx={{ fontSize: 40, color: '#DC2626' }} />
      </Box>

      <Typography
        variant="h6"
        sx={{
          fontWeight: 600,
          color: '#1C1917',
          mb: 1,
        }}
      >
        {title}
      </Typography>

      <Typography
        sx={{
          color: '#78716C',
          maxWidth: 360,
          mb: 3,
        }}
      >
        {message}
      </Typography>

      <Stack direction="row" spacing={2}>
        {onRetry && (
          <Button
            variant="contained"
            onClick={onRetry}
            startIcon={<Refresh />}
            sx={{
              bgcolor: '#1C1917',
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: '#44403C' },
            }}
          >
            Try Again
          </Button>
        )}
        {showHomeButton && (
          <Button
            variant="outlined"
            component={Link}
            to="/"
            startIcon={<Home />}
            sx={{
              borderColor: '#E5E5E5',
              color: '#44403C',
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { borderColor: '#1C1917' },
            }}
          >
            Go Home
          </Button>
        )}
      </Stack>
    </Box>
  )
}
