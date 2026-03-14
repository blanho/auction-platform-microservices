import { useState } from 'react'
import { useTranslation } from 'react-i18next'
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

const socialLinks = [
  { icon: <Facebook />, href: 'https://facebook.com', label: 'Facebook' },
  { icon: <Twitter />, href: 'https://twitter.com', label: 'Twitter' },
  { icon: <Instagram />, href: 'https://instagram.com', label: 'Instagram' },
  { icon: <YouTube />, href: 'https://youtube.com', label: 'YouTube' },
  { icon: <Pinterest />, href: 'https://pinterest.com', label: 'Pinterest' },
]

export function Footer() {
  const { t } = useTranslation()
  const [email, setEmail] = useState('')
  const [isSubscribed, setIsSubscribed] = useState(false)
  const [error, setError] = useState('')

  const footerLinks = {
    shop: {
      title: t('footer.shop'),
      links: [
        { label: t('footer.allAuctions'), href: '/auctions' },
        { label: t('nav.categories'), href: '/categories' },
        { label: t('footer.endingSoon'), href: '/auctions?sort=ending-soon' },
        { label: t('footer.newArrivals'), href: '/auctions?sort=newest' },
        { label: t('footer.buyNow'), href: '/auctions?buyNow=true' },
      ],
    },
    sell: {
      title: t('footer.sell'),
      links: [
        { label: t('footer.startSelling'), href: '/sell' },
        { label: t('footer.sellerCenter'), href: '/seller-center' },
        { label: t('footer.sellerGuidelines'), href: '/seller-guidelines' },
        { label: t('footer.feesPricing'), href: '/fees' },
        { label: t('footer.successStories'), href: '/success-stories' },
      ],
    },
    help: {
      title: t('footer.helpSupport'),
      links: [
        { label: t('footer.help'), href: '/help' },
        { label: t('footer.contact'), href: '/contact' },
        { label: t('footer.biddingGuide'), href: '/bidding-guide' },
        { label: t('footer.shippingInfo'), href: '/shipping' },
        { label: t('footer.returnsRefunds'), href: '/returns' },
      ],
    },
    company: {
      title: t('footer.company'),
      links: [
        { label: t('footer.aboutUs'), href: '/about' },
        { label: t('footer.careers'), href: '/careers' },
        { label: t('footer.press'), href: '/press' },
        { label: t('footer.sustainability'), href: '/sustainability' },
        { label: t('footer.affiliateProgram'), href: '/affiliates' },
      ],
    },
  }

  const handleSubscribe = (e: React.FormEvent) => {
    e.preventDefault()
    if (!email?.includes('@')) {
      setError(t('validation.invalidEmail'))
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
              {t('brandName')}
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
              {t('footer.tagline')}
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
                {t('footer.newsletter')}
              </Typography>

              <Collapse in={isSubscribed}>
                <InlineAlert severity="success" sx={{ mb: 2 }} onClose={() => setIsSubscribed(false)}>
                  {t('footer.newsletterSuccess')}
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
                  placeholder={t('footer.emailPlaceholder')}
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
              {t('footer.copyright', { year: new Date().getFullYear() })}
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
                {t('footer.privacy')}
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
                {t('footer.terms')}
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
                {t('footer.cookies')}
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
            {t('backToTop')}
          </Button>
        </Stack>
      </Container>
    </Box>
  )
}
