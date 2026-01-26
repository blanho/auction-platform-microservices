import { Container, Typography, Box, Grid, Card, CardContent, CardMedia, Button } from '@mui/material'
import { Link } from 'react-router-dom'
import { Gavel, TrendingUp, Security, Speed } from '@mui/icons-material'

const features = [
  {
    icon: <Gavel sx={{ fontSize: 48 }} />,
    title: 'Live Auctions',
    description: 'Participate in real-time auctions with instant bid updates and notifications.',
  },
  {
    icon: <TrendingUp sx={{ fontSize: 48 }} />,
    title: 'Auto Bidding',
    description: 'Set your maximum bid and let our system bid automatically for you.',
  },
  {
    icon: <Security sx={{ fontSize: 48 }} />,
    title: 'Secure Payments',
    description: 'Safe and secure payment processing with multiple payment options.',
  },
  {
    icon: <Speed sx={{ fontSize: 48 }} />,
    title: 'Fast & Reliable',
    description: 'High-performance platform built for speed and reliability.',
  },
]

export const HomePage = () => {
  return (
    <Box>
      <Box
        sx={{
          background: 'linear-gradient(135deg, #1e3a8a 0%, #3730a3 100%)',
          color: 'white',
          py: 12,
          mb: 6,
        }}
      >
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', maxWidth: 800, mx: 'auto' }}>
            <Typography variant="h2" fontWeight={700} gutterBottom>
              Discover Unique Items at Auction
            </Typography>
            <Typography variant="h5" sx={{ mb: 4, opacity: 0.9 }}>
              Join thousands of buyers and sellers in our trusted marketplace.
              Bid on exclusive items or sell your treasures.
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button
                variant="contained"
                size="large"
                component={Link}
                to="/auctions"
                sx={{
                  bgcolor: 'white',
                  color: 'primary.main',
                  '&:hover': { bgcolor: 'grey.100' },
                }}
              >
                Browse Auctions
              </Button>
              <Button
                variant="outlined"
                size="large"
                component={Link}
                to="/register"
                sx={{
                  borderColor: 'white',
                  color: 'white',
                  '&:hover': { borderColor: 'white', bgcolor: 'rgba(255,255,255,0.1)' },
                }}
              >
                Start Selling
              </Button>
            </Box>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="lg" sx={{ mb: 8 }}>
        <Typography variant="h4" fontWeight={700} textAlign="center" gutterBottom>
          Why Choose Our Platform
        </Typography>
        <Typography variant="body1" color="text.secondary" textAlign="center" sx={{ mb: 6 }}>
          Experience the best auction platform with features designed for you
        </Typography>

        <Grid container spacing={4}>
          {features.map((feature, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={index}>
              <Card
                elevation={0}
                sx={{
                  height: '100%',
                  textAlign: 'center',
                  border: 1,
                  borderColor: 'divider',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  '&:hover': {
                    transform: 'translateY(-4px)',
                    boxShadow: 4,
                  },
                }}
              >
                <CardContent sx={{ p: 4 }}>
                  <Box sx={{ color: 'primary.main', mb: 2 }}>{feature.icon}</Box>
                  <Typography variant="h6" fontWeight={600} gutterBottom>
                    {feature.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {feature.description}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box sx={{ bgcolor: 'grey.100', py: 8 }}>
        <Container maxWidth="lg">
          <Typography variant="h4" fontWeight={700} textAlign="center" gutterBottom>
            Featured Auctions
          </Typography>
          <Typography variant="body1" color="text.secondary" textAlign="center" sx={{ mb: 6 }}>
            Check out our most popular ongoing auctions
          </Typography>

          <Grid container spacing={4}>
            {[1, 2, 3, 4].map((item) => (
              <Grid size={{ xs: 12, sm: 6, md: 3 }} key={item}>
                <Card
                  sx={{
                    height: '100%',
                    transition: 'transform 0.2s, box-shadow 0.2s',
                    '&:hover': {
                      transform: 'translateY(-4px)',
                      boxShadow: 4,
                    },
                  }}
                >
                  <CardMedia
                    component="div"
                    sx={{
                      height: 200,
                      bgcolor: 'grey.300',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                    }}
                  >
                    <Gavel sx={{ fontSize: 64, color: 'grey.500' }} />
                  </CardMedia>
                  <CardContent>
                    <Typography variant="subtitle1" fontWeight={600} noWrap>
                      Sample Auction Item {item}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Current Bid: $0.00
                    </Typography>
                    <Button
                      variant="contained"
                      size="small"
                      fullWidth
                      component={Link}
                      to={`/auctions/${item}`}
                    >
                      View Details
                    </Button>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          <Box sx={{ textAlign: 'center', mt: 4 }}>
            <Button variant="outlined" size="large" component={Link} to="/auctions">
              View All Auctions
            </Button>
          </Box>
        </Container>
      </Box>
    </Box>
  )
}
