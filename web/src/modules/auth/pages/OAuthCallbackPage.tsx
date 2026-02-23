import { useEffect, useState, useRef, useMemo } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { Box, CircularProgress, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { useOAuthExchange } from '../hooks'
import { palette } from '@/shared/theme/tokens'
import { getErrorMessage } from '@/services/http'
import { fadeInUp } from '@/shared/lib/animations'

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
        bgcolor: palette.neutral[50],
      }}
    >
      <Box
        sx={{
          display: { xs: 'none', lg: 'block' },
          width: '50%',
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            backgroundImage: 'url(https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=1200&q=80)',
            backgroundSize: 'cover',
            backgroundPosition: 'center',
          }}
        />
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            bgcolor: 'rgba(28, 25, 23, 0.4)',
          }}
        />
      </Box>

      <Box
        sx={{
          flex: 1,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          px: { xs: 3, md: 6 },
        }}
      >
        <Box sx={{ width: '100%', maxWidth: 440, textAlign: 'center' }}>
          <motion.div initial="initial" animate="animate" variants={fadeInUp}>
            <Typography
              component={Link}
              to="/"
              sx={{
                display: 'block',
                fontFamily: '"Playfair Display", serif',
                fontStyle: 'italic',
                fontWeight: 500,
                fontSize: '1.75rem',
                color: palette.neutral[900],
                textDecoration: 'none',
                mb: 6,
              }}
            >
              TheAuction
            </Typography>

            {errorMessage ? (
              <>
                <Typography
                  variant="h4"
                  sx={{
                    fontFamily: '"Playfair Display", serif',
                    fontWeight: 500,
                    color: palette.semantic.error,
                    mb: 2,
                  }}
                >
                  Authentication Failed
                </Typography>
                <Typography
                  sx={{
                    color: palette.neutral[600],
                    mb: 3,
                  }}
                >
                  {errorMessage}
                </Typography>
                <Typography
                  sx={{
                    color: palette.neutral[500],
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
                    color: palette.neutral[900],
                    mb: 4,
                  }}
                />
                <Typography
                  variant="h5"
                  sx={{
                    fontFamily: '"Playfair Display", serif',
                    color: palette.neutral[900],
                    fontWeight: 500,
                    mb: 2,
                  }}
                >
                  Completing sign in...
                </Typography>
                <Typography
                  sx={{
                    color: palette.neutral[500],
                    fontSize: '0.9375rem',
                  }}
                >
                  Please wait while we verify your account
                </Typography>
              </>
            )}
          </motion.div>
        </Box>
      </Box>
    </Box>
  )
}
