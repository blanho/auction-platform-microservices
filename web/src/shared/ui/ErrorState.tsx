import { Box, Typography, Button, Stack } from '@mui/material'
import { useTranslation } from 'react-i18next'
import { Warning, Refresh, Home } from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { palette } from '@/shared/theme/tokens'

interface ErrorStateProps {
  title?: string
  message?: string
  onRetry?: () => void
  showHomeButton?: boolean
}

export function ErrorState({
  title,
  message,
  onRetry,
  showHomeButton = false,
}: ErrorStateProps) {
  const { t } = useTranslation()
  const displayTitle = title || t('errors.defaultTitle')
  const displayMessage = message || t('errors.defaultMessage')

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
          bgcolor: palette.semantic.errorLight,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 3,
        }}
      >
        <Warning sx={{ fontSize: 40, color: palette.semantic.error }} />
      </Box>

      <Typography
        variant="h6"
        sx={{
          fontWeight: 600,
          color: palette.neutral[900],
          mb: 1,
        }}
      >
        {displayTitle}
      </Typography>

      <Typography
        sx={{
          color: palette.neutral[500],
          maxWidth: 360,
          mb: 3,
        }}
      >
        {displayMessage}
      </Typography>

      <Stack direction="row" spacing={2}>
        {onRetry && (
          <Button
            variant="contained"
            onClick={onRetry}
            startIcon={<Refresh />}
            sx={{
              bgcolor: palette.neutral[900],
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { bgcolor: palette.neutral[700] },
            }}
          >
            {t('tryAgain')}
          </Button>
        )}
        {showHomeButton && (
          <Button
            variant="outlined"
            component={Link}
            to="/"
            startIcon={<Home />}
            sx={{
              borderColor: palette.neutral[200],
              color: palette.neutral[700],
              textTransform: 'none',
              fontWeight: 600,
              '&:hover': { borderColor: palette.neutral[900] },
            }}
          >
            {t('goHome')}
          </Button>
        )}
      </Stack>
    </Box>
  )
}
