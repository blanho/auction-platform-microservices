import { Link } from 'react-router-dom'
import { Box, Container, Typography, Button, Stack } from '@mui/material'
import { Home, Search, ArrowBack } from '@mui/icons-material'

export const NotFoundPage = () => {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAF9', display: 'flex', alignItems: 'center' }}>
      <Container maxWidth="sm">
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <Typography
            sx={{
              fontSize: { xs: '8rem', md: '12rem' },
              fontFamily: '"Playfair Display", serif',
              fontWeight: 700,
              color: '#1C1917',
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
              bgcolor: '#CA8A04',
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
              color: '#1C1917',
              mb: 2,
            }}
          >
            Page Not Found
          </Typography>
          
          <Typography
            sx={{
              color: '#78716C',
              fontSize: '1.125rem',
              mb: 4,
              maxWidth: 400,
              mx: 'auto',
            }}
          >
            The page you're looking for seems to have gone to auction. 
            It might have been sold, moved, or never existed.
          </Typography>

          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={2}
            justifyContent="center"
          >
            <Button
              variant="contained"
              component={Link}
              to="/"
              startIcon={<Home />}
              sx={{
                bgcolor: '#1C1917',
                textTransform: 'none',
                fontWeight: 600,
                px: 4,
                py: 1.5,
                '&:hover': { bgcolor: '#44403C' },
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
                borderColor: '#CA8A04',
                color: '#CA8A04',
                textTransform: 'none',
                fontWeight: 600,
                px: 4,
                py: 1.5,
                '&:hover': {
                  borderColor: '#A16207',
                  bgcolor: '#FEF3C7',
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
              color: '#78716C',
              textTransform: 'none',
              '&:hover': { bgcolor: 'transparent', color: '#1C1917' },
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
