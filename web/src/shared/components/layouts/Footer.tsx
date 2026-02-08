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
        color: palette.neutral[0],
        pt: 10,
        pb: 5,
      }}
    >
      <Container maxWidth="xl">
        <Grid container spacing={8}>
          <Grid size={{ xs: 12, lg: 4 }}>
            <Typography
              sx={{
                fontFamily: '"Playfair Display", serif',
                fontWeight: 600,
                fontSize: '1.375rem',
                letterSpacing: '0.08em',
                mb: 3,
              }}
            >
              THE AUCTION
            </Typography>
            <Typography
              variant="body2"
              sx={{
                color: palette.neutral[400],
                lineHeight: 1.8,
                mb: 4,
                maxWidth: 320,
                fontSize: '0.8125rem',
              }}
            >
              Authenticated luxury consignment. Discover extraordinary pieces
              from the world's most discerning collectors.
            </Typography>

            <Box component="form" onSubmit={handleSubscribe} sx={{ maxWidth: 360 }}>
              <Typography
                sx={{
                  color: palette.neutral[300],
                  fontWeight: 500,
                  mb: 2,
                  textTransform: 'uppercase',
                  letterSpacing: '0.12em',
                  fontSize: '0.6875rem',
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

              <Stack direction="row" spacing={0}>
                <TextField
                  fullWidth
                  size="small"
                  placeholder="Enter your email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      bgcolor: 'transparent',
                      borderRadius: 0,
                      '& fieldset': {
                        borderColor: palette.neutral[700],
                        borderRight: 'none',
                      },
                      '&:hover fieldset': {
                        borderColor: palette.neutral[500],
                      },
                      '&.Mui-focused fieldset': {
                        borderColor: palette.neutral[400],
                      },
                    },
                    '& .MuiOutlinedInput-input': {
                      color: palette.neutral[0],
                      fontSize: '0.8125rem',
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
                    bgcolor: palette.neutral[0],
                    color: palette.neutral[900],
                    minWidth: 48,
                    px: 2,
                    borderRadius: 0,
                    '&:hover': {
                      bgcolor: palette.neutral[200],
                    },
                  }}
                >
                  <ArrowForward sx={{ fontSize: 18 }} />
                </Button>
              </Stack>
            </Box>

            <Stack direction="row" spacing={1} sx={{ mt: 5 }}>
              {socialLinks.map((social) => (
                <IconButton
                  key={social.label}
                  component="a"
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  aria-label={social.label}
                  size="small"
                  sx={{
                    color: palette.neutral[500],
                    '&:hover': {
                      color: palette.neutral[0],
                      bgcolor: 'transparent',
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
                    sx={{
                      color: palette.neutral[300],
                      fontWeight: 500,
                      mb: 3,
                      textTransform: 'uppercase',
                      letterSpacing: '0.12em',
                      fontSize: '0.6875rem',
                    }}
                  >
                    {section.title}
                  </Typography>
                  <Stack spacing={2}>
                    {section.links.map((link) => (
                      <Typography
                        key={link.href}
                        component={Link}
                        to={link.href}
                        sx={{
                          color: palette.neutral[400],
                          fontSize: '0.8125rem',
                          textDecoration: 'none',
                          transition: 'color 0.2s ease',
                          '&:hover': {
                            color: palette.neutral[0],
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

        <Divider sx={{ borderColor: palette.neutral[800], my: 6 }} />

        <Stack
          direction={{ xs: 'column', md: 'row' }}
          justifyContent="space-between"
          alignItems={{ xs: 'flex-start', md: 'center' }}
          spacing={2}
        >
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={{ xs: 1, sm: 4 }}>
            <Typography variant="body2" sx={{ color: palette.neutral[600], fontSize: '0.75rem' }}>
              © {new Date().getFullYear()} The Auction. All rights reserved.
            </Typography>
            <Stack direction="row" spacing={3}>
              <Typography
                component={Link}
                to="/privacy"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.75rem',
                  textDecoration: 'none',
                  '&:hover': { color: palette.neutral[0] },
                }}
              >
                Privacy
              </Typography>
              <Typography
                component={Link}
                to="/terms"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.75rem',
                  textDecoration: 'none',
                  '&:hover': { color: palette.neutral[0] },
                }}
              >
                Terms
              </Typography>
              <Typography
                component={Link}
                to="/cookies"
                sx={{
                  color: palette.neutral[500],
                  fontSize: '0.75rem',
                  textDecoration: 'none',
                  '&:hover': { color: palette.neutral[0] },
                }}
              >
                Cookies
              </Typography>
            </Stack>
          </Stack>

          <Button
            onClick={scrollToTop}
            endIcon={<KeyboardArrowUp />}
            sx={{
              color: palette.neutral[500],
              textTransform: 'uppercase',
              fontSize: '0.6875rem',
              letterSpacing: '0.1em',
              '&:hover': {
                color: palette.neutral[0],
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
