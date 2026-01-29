import { Link } from 'react-router-dom'
import { Box, Container, Typography, Button, Stack } from '@mui/material'
import { Home, Search, ArrowBack } from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'

export const NotFoundPage = () => {
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
            404
          </Typography>

          <Box
            sx={{
              width: 120,
              height: 4,
              bgcolor: palette.brand.primary,
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
            Page Not Found
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
            The page you're looking for seems to have gone to auction. It might have been sold,
            moved, or never existed.
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
              component={Link}
              to="/auctions"
              startIcon={<Search />}
              sx={{
                borderColor: palette.brand.primary,
                color: palette.brand.primary,
                textTransform: 'none',
                fontWeight: 600,
                px: 4,
                py: 1.5,
                '&:hover': {
                  borderColor: palette.brand.secondary,
                  bgcolor: palette.brand.muted,
                },
              }}
            >
              Browse Auctions
            </Button>
          </Stack>

          <Button
            onClick={() => window.history.back()}
            startIcon={<ArrowBack />}
            sx={{
              mt: 4,
              color: palette.neutral[500],
              textTransform: 'none',
              '&:hover': { bgcolor: 'transparent', color: palette.neutral[900] },
            }}
          >
            Go Back
          </Button>
        </Box>
      </Container>
    </Box>
  )
}

export default NotFoundPage
