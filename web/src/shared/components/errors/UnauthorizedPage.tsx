import { Link } from 'react-router-dom'
import { Box, Container, Typography, Button } from '@mui/material'
import { Block as BlockIcon } from '@mui/icons-material'

export const UnauthorizedPage = () => {
  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: '60vh',
          textAlign: 'center',
          gap: 3,
        }}
      >
        <BlockIcon sx={{ fontSize: 120, color: 'error.main' }} />
        <Typography variant="h1" fontWeight={700} color="text.primary">
          403
        </Typography>
        <Typography variant="h5" color="text.secondary">
          Access Denied
        </Typography>
        <Typography variant="body1" color="text.secondary">
          You do not have permission to access this page. Please contact an administrator
          if you believe this is a mistake.
        </Typography>
        <Button variant="contained" size="large" component={Link} to="/">
          Go to Homepage
        </Button>
      </Box>
    </Container>
  )
}
