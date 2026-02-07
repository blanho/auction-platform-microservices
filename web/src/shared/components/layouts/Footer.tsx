import { useState } from 'react'
import {
  Box,
  Container,
  Grid,
  Typography,
  TextField,
  Button,
  Stack,
  IconButton,
  Divider,
  Collapse,
} from '@mui/material'
import {
  Facebook,
  Twitter,
  Instagram,
  YouTube,
  Pinterest,
  ArrowForward,
  KeyboardArrowUp,
} from '@mui/icons-material'
import { Link } from 'react-router-dom'
import { palette } from '@/shared/theme/tokens'
import { InlineAlert } from '@/shared/ui'

const footerLinks = {
  shop: {
    title: 'Shop',
    links: [
      { label: 'All Auctions', href: '/auctions' },
      { label: 'Categories', href: '/categories' },
      { label: 'Ending Soon', href: '/auctions?sort=ending-soon' },
      { label: 'New Arrivals', href: '/auctions?sort=newest' },
      { label: 'Buy Now', href: '/auctions?buyNow=true' },
    ],
  },
  sell: {
    title: 'Sell',
    links: [
      { label: 'Start Selling', href: '/sell' },
      { label: 'Seller Center', href: '/seller-center' },
      { label: 'Seller Guidelines', href: '/seller-guidelines' },
      { label: 'Fees & Pricing', href: '/fees' },
      { label: 'Success Stories', href: '/success-stories' },
    ],
  },
  help: {
    title: 'Help & Support',
    links: [
      { label: 'Help Center', href: '/help' },
      { label: 'Contact Us', href: '/contact' },
      { label: 'Bidding Guide', href: '/bidding-guide' },
      { label: 'Shipping Info', href: '/shipping' },
      { label: 'Returns & Refunds', href: '/returns' },
    ],
  },
  company: {
    title: 'Company',
    links: [
      { label: 'About Us', href: '/about' },
      { label: 'Careers', href: '/careers' },
      { label: 'Press', href: '/press' },
      { label: 'Sustainability', href: '/sustainability' },
      { label: 'Affiliate Program', href: '/affiliates' },
    ],
  },
}

const socialLinks = [
  { icon: <Facebook />, href: 'https://facebook.com', label: 'Facebook' },
  { icon: <Twitter />, href: 'https://twitter.com', label: 'Twitter' },
  { icon: <Instagram />, href: 'https://instagram.com', label: 'Instagram' },
  { icon: <YouTube />, href: 'https://youtube.com', label: 'YouTube' },
  { icon: <Pinterest />, href: 'https://pinterest.com', label: 'Pinterest' },
]

export function Footer() {
  const [email, setEmail] = useState('')
  const [isSubscribed, setIsSubscribed] = useState(false)
  const [error, setError] = useState('')

  const handleSubscribe = (e: React.FormEvent) => {
    e.preventDefault()
    if (!email?.includes('@')) {
      setError('Please enter a valid email address')
      return
    }
    setIsSubscribed(true)
    setEmail('')
    setError('')
  }

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <Box
      component="footer"
      sx={{
        bgcolor: palette.neutral[900],
        color: 'white',
        pt: 8,
        pb: 4,
      }}
    >
      <Container maxWidth="xl">
        <Grid container spacing={6}>
          <Grid size={{ xs: 12, lg: 4 }}>
            <Typography
              variant="h5"
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 700,
                mb: 2,
              }}
            >
              AUCTION
            </Typography>
            <Typography
              variant="body2"
              sx={{
                color: palette.neutral[400],
                lineHeight: 1.8,
                mb: 3,
                maxWidth: 320,
              }}
            >
              Discover unique treasures and rare finds. Join thousands of collectors and enthusiasts
              in the premier destination for online auctions.
            </Typography>

            <Box component="form" onSubmit={handleSubscribe} sx={{ maxWidth: 360 }}>
              <Typography
                variant="subtitle2"
                sx={{
                  color: 'white',
                  fontWeight: 600,
                  mb: 1.5,
                  textTransform: 'uppercase',
                  letterSpacing: 1,
                  fontSize: '0.75rem',
                }}
              >
                Subscribe to Our Newsletter
              </Typography>

              <Collapse in={isSubscribed}>
                <InlineAlert severity="success" sx={{ mb: 2 }} onClose={() => setIsSubscribed(false)}>
                  Thank you for subscribing!
                </InlineAlert>
              </Collapse>

              <Collapse in={!!error}>
                <InlineAlert severity="error" sx={{ mb: 2 }} onClose={() => setError('')}>
                  {error}
                </InlineAlert>
              </Collapse>

              <Stack direction="row" spacing={1}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Enter your email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      bgcolor: 'rgba(255,255,255,0.05)',
                      borderRadius: 1,
                      '& fieldset': {
                        borderColor: 'rgba(255,255,255,0.1)',
                      },
                      '&:hover fieldset': {
                        borderColor: 'rgba(255,255,255,0.2)',
                      },
                      '&.Mui-focused fieldset': {
                        borderColor: palette.brand.primary,
                      },
                    },
                    '& .MuiOutlinedInput-input': {
                      color: 'white',
                      '&::placeholder': {
                        color: palette.neutral[500],
                        opacity: 1,
                      },
                    },
                  }}
                />
                <Button
                  type="submit"
                  variant="contained"
                  sx={{
                    bgcolor: palette.brand.primary,
                    color: 'white',
                    minWidth: 48,
                    px: 2,
                    '&:hover': {
                      bgcolor: palette.brand.secondary,
                    },
                  }}
                >
                  <ArrowForward />
                </Button>
              </Stack>
            </Box>

            <Stack direction="row" spacing={1} sx={{ mt: 4 }}>
              {socialLinks.map((social) => (
                <IconButton
                  key={social.label}
                  component="a"
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={social.label}
                  sx={{
                    color: palette.neutral[500],
                    '&:hover': {
                      color: 'white',
                      bgcolor: 'rgba(255,255,255,0.1)',
                    },
                  }}
                >
                  {social.icon}
                </IconButton>
              ))}
            </Stack>
          </Grid>

          <Grid size={{ xs: 12, lg: 8 }}>
            <Grid container spacing={4}>
              {Object.entries(footerLinks).map(([key, section]) => (
                <Grid key={key} size={{ xs: 6, sm: 3 }}>
                  <Typography
                    variant="subtitle2"
                    sx={{
                      color: 'white',
                      fontWeight: 600,
                      mb: 2,
                      textTransform: 'uppercase',
                      letterSpacing: 1,
                      fontSize: '0.75rem',
                    }}
                  >
                    {section.title}
                  </Typography>
                  <Stack spacing={1.5}>
                    {section.links.map((link) => (
                      <Typography
                        key={link.href}
                        component={Link}
                        to={link.href}
                        sx={{
                          color: palette.neutral[400],
                          fontSize: '0.875rem',
                          textDecoration: 'none',
                          transition: 'color 0.2s ease',
                          '&:hover': {
                            color: 'white',
                          },
                        }}
                      >
                        {link.label}
                      </Typography>
                    ))}
                  </Stack>
                </Grid>
              ))}
            </Grid>
          </Grid>
        </Grid>

        <Divider sx={{ borderColor: 'rgba(255,255,255,0.1)', my: 6 }} />

        <Stack
          direction={{ xs: 'column', md: 'row' }}
          justifyContent="space-between"
          alignItems={{ xs: 'flex-start', md: 'center' }}
          spacing={2}
        >
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={{ xs: 1, sm: 3 }}>
            <Typography variant="body2" sx={{ color: palette.neutral[500] }}>
              © {new Date().getFullYear()} Auction Platform. All rights reserved.
            </Typography>
            <Stack direction="row" spacing={2}>
              <Typography
                component={Link}
                to="/privacy"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.875rem',
                  textDecoration: 'none',
                  '&:hover': { color: 'white' },
                }}
              >
                Privacy Policy
              </Typography>
              <Typography
                component={Link}
                to="/terms"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.875rem',
                  textDecoration: 'none',
                  '&:hover': { color: 'white' },
                }}
              >
                Terms of Service
              </Typography>
              <Typography
                component={Link}
                to="/cookies"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.875rem',
                  textDecoration: 'none',
                  '&:hover': { color: 'white' },
                }}
              >
                Cookie Policy
              </Typography>
            </Stack>
          </Stack>

          <Button
            onClick={scrollToTop}
            endIcon={<KeyboardArrowUp />}
            sx={{
              color: palette.neutral[500],
              textTransform: 'none',
              '&:hover': {
                color: 'white',
                bgcolor: 'transparent',
              },
            }}
          >
            Back to Top
          </Button>
        </Stack>
      </Container>
    </Box>
  )
}
