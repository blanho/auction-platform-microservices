import { useEffect, useState, useRef, useMemo } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Box, CircularProgress, Typography, alpha } from '@mui/material'
import { useOAuthExchange } from '../hooks'
import { colors, palette } from '@/shared/theme/tokens'
import { getErrorMessage } from '@/services/http'

export function OAuthCallbackPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [mutationError, setMutationError] = useState<string | null>(null)
  const exchangeMutation = useOAuthExchange()
  const hasProcessedRef = useRef(false)

  const urlError = useMemo(() => {
    const error = searchParams.get('error')
    if (error) {
      return decodeURIComponent(error)
    }
    const code = searchParams.get('code')
    if (!code) {
      return 'No authorization code received'
    }
    return null
  }, [searchParams])

  const errorMessage = mutationError ?? urlError

  useEffect(() => {
    if (hasProcessedRef.current) {
      return
    }

    if (urlError) {
      hasProcessedRef.current = true
      setTimeout(() => navigate('/login', { replace: true }), 3000)
      return
    }

    const code = searchParams.get('code')
    if (!code) {
      return
    }

    hasProcessedRef.current = true
    exchangeMutation.mutate(code, {
      onSuccess: () => {
        navigate('/', { replace: true })
      },
      onError: (err) => {
        setMutationError(getErrorMessage(err))
        setTimeout(() => navigate('/login', { replace: true }), 3000)
      },
    })
  }, [searchParams, navigate, exchangeMutation, urlError])

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        bgcolor: colors.background.primary,
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          top: '30%',
          left: '50%',
          transform: 'translateX(-50%)',
          width: '500px',
          height: '500px',
          background: errorMessage
            ? `radial-gradient(circle, ${alpha(palette.semantic.error, 0.15)} 0%, transparent 70%)`
            : `radial-gradient(circle, ${alpha(palette.brand.primary, 0.15)} 0%, transparent 70%)`,
          pointerEvents: 'none',
        }}
      />

      <Box
        sx={{
          position: 'relative',
          zIndex: 1,
          textAlign: 'center',
          p: 4,
        }}
      >
        {errorMessage ? (
          <>
            <Typography
              variant="h5"
              sx={{
                color: palette.semantic.error,
                fontWeight: 600,
                mb: 2,
              }}
            >
              Authentication Failed
            </Typography>
            <Typography
              sx={{
                color: 'rgba(255, 255, 255, 0.6)',
                mb: 3,
              }}
            >
              {errorMessage}
            </Typography>
            <Typography
              sx={{
                color: 'rgba(255, 255, 255, 0.4)',
                fontSize: '0.875rem',
              }}
            >
              Redirecting to login...
            </Typography>
          </>
        ) : (
          <>
            <CircularProgress
              size={48}
              sx={{
                color: palette.brand.primary,
                mb: 3,
              }}
            />
            <Typography
              variant="h6"
              sx={{
                color: 'rgba(255, 255, 255, 0.8)',
                fontWeight: 500,
                mb: 1,
              }}
            >
              Completing sign in...
            </Typography>
            <Typography
              sx={{
                color: 'rgba(255, 255, 255, 0.5)',
                fontSize: '0.875rem',
              }}
            >
              Please wait while we verify your account
            </Typography>
          </>
        )}
      </Box>
    </Box>
  )
}
