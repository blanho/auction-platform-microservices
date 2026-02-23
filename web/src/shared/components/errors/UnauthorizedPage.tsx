import { Link } from 'react-router-dom'
import { Box, Container, Typography, Button, Stack } from '@mui/material'
import { Home, ArrowBack } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'

export const UnauthorizedPage = () => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: palette.neutral[50],
        display: 'flex',
        alignItems: 'center',
      }}
    >
      <Container maxWidth="sm">
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <Typography
            sx={{
              fontSize: { xs: '8rem', md: '12rem' },
              fontFamily: '"Playfair Display", serif',
              fontWeight: 700,
              color: palette.neutral[900],
              lineHeight: 1,
              mb: 2,
            }}
          >
            403
          </Typography>

          <Box
            sx={{
              width: 120,
              height: 4,
              bgcolor: palette.semantic.error,
              mx: 'auto',
              mb: 4,
              borderRadius: 2,
            }}
          />

          <Typography
            variant="h4"
            sx={{
              fontFamily: '"Playfair Display", serif',
              fontWeight: 600,
              color: palette.neutral[900],
              mb: 2,
            }}
          >
            Access Denied
          </Typography>

          <Typography
            sx={{
              color: palette.neutral[500],
              fontSize: '1.125rem',
              mb: 4,
              maxWidth: 400,
              mx: 'auto',
            }}
          >
            You do not have permission to access this page. Please contact an administrator if you
            believe this is a mistake.
          </Typography>

          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
            <Button
              variant="contained"
              component={Link}
              to="/"
              startIcon={<Home />}
              sx={{
                bgcolor: palette.neutral[900],
                textTransform: 'none',
                fontWeight: 600,
                px: 4,
                py: 1.5,
                '&:hover': { bgcolor: palette.neutral[700] },
              }}
            >
              Go Home
            </Button>
            <Button
              variant="outlined"
              onClick={() => globalThis.history.back()}
              startIcon={<ArrowBack />}
              sx={{
                borderColor: palette.neutral[300],
                color: palette.neutral[700],
                textTransform: 'none',
                fontWeight: 600,
                px: 4,
                py: 1.5,
                '&:hover': {
                  borderColor: palette.neutral[900],
                  bgcolor: 'transparent',
                },
              }}
            >
              Go Back
            </Button>
          </Stack>
        </Box>
      </Container>
    </Box>
  )
}
