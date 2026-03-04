import { Box, Container, Typography, Grid, Button, Stack } from '@mui/material'
import { Link } from 'react-router-dom'
import {
  PersonAdd,
  Search,
  Gavel,
  EmojiEvents,
  Payment,
  LocalShipping,
  ArrowForward,
} from '@mui/icons-material'
import { palette } from '@/shared/theme/tokens'
import { useTranslation } from 'react-i18next'

const steps = [
  { icon: <PersonAdd sx={{ fontSize: 48 }} />, step: '01', key: '1' },
  { icon: <Search sx={{ fontSize: 48 }} />, step: '02', key: '2' },
  { icon: <Gavel sx={{ fontSize: 48 }} />, step: '03', key: '3' },
  { icon: <EmojiEvents sx={{ fontSize: 48 }} />, step: '04', key: '4' },
  { icon: <Payment sx={{ fontSize: 48 }} />, step: '05', key: '5' },
  { icon: <LocalShipping sx={{ fontSize: 48 }} />, step: '06', key: '6' },
]

const sellerSteps = [
  { number: '01', key: '1' },
  { number: '02', key: '2' },
  { number: '03', key: '3' },
  { number: '04', key: '4' },
]

export const HowItWorksPage = () => {
  const { t } = useTranslation('home')

  return (
    <Box sx={{ bgcolor: palette.neutral[50], minHeight: '100vh' }}>
      <Box sx={{ bgcolor: palette.neutral[900], py: { xs: 12, md: 16 }, mb: { xs: 8, md: 12 } }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', maxWidth: 700, mx: 'auto' }}>
            <Typography
              variant="overline"
              sx={{ color: palette.brand.primary, letterSpacing: 4, mb: 2, display: 'block' }}
            >
              {t('howItWorksPage.badge')}
            </Typography>
            <Typography
              variant="h2"
              sx={{
                color: palette.neutral[50],
                fontWeight: 300,
                mb: 3,
                fontSize: { xs: '2.5rem', md: '3.5rem' },
              }}
            >
              {t('howItWorksPage.title')}
            </Typography>
            <Typography variant="h6" sx={{ color: 'rgba(250,250,249,0.7)', fontWeight: 400 }}>
              {t('howItWorksPage.subtitle')}
            </Typography>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="lg" sx={{ mb: { xs: 10, md: 14 } }}>
        <Box sx={{ mb: 6 }}>
          <Typography
            variant="overline"
            sx={{ color: palette.neutral[700], letterSpacing: 3, display: 'block', mb: 1 }}
          >
            {t('howItWorksPage.buyersLabel')}
          </Typography>
          <Typography variant="h3" sx={{ color: palette.neutral[900], fontWeight: 400, mb: 2 }}>
            {t('howItWorksPage.buyersTitle')}
          </Typography>
          <Typography variant="body1" sx={{ color: palette.neutral[700], maxWidth: 600 }}>
            {t('howItWorksPage.buyersSubtitle')}
          </Typography>
        </Box>

        <Grid container spacing={4}>
          {steps.map((step, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={index}>
              <Box
                sx={{
                  p: 4,
                  height: '100%',
                  bgcolor: index % 2 === 0 ? '#FFFFFF' : 'transparent',
                  border: '1px solid rgba(68,64,60,0.1)',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    boxShadow: '0 8px 30px rgba(0,0,0,0.08)',
                    transform: 'translateY(-4px)',
                  },
                }}
              >
                <Typography
                  variant="h4"
                  sx={{ color: 'rgba(202,138,4,0.3)', fontWeight: 700, mb: 2 }}
                >
                  {step.step}
                </Typography>
                <Box sx={{ color: palette.brand.primary, mb: 2 }}>{step.icon}</Box>
                <Typography
                  variant="h6"
                  sx={{ color: palette.neutral[900], fontWeight: 500, mb: 1.5 }}
                >
                  {t(`howItWorksPage.steps.${step.key}.title`)}
                </Typography>
                <Typography variant="body2" sx={{ color: palette.neutral[700], lineHeight: 1.7 }}>
                  {t(`howItWorksPage.steps.${step.key}.description`)}
                </Typography>
              </Box>
            </Grid>
          ))}
        </Grid>
      </Container>

      <Box sx={{ bgcolor: palette.neutral[900], py: { xs: 10, md: 14 } }}>
        <Container maxWidth="lg">
          <Grid container spacing={8} alignItems="center">
            <Grid size={{ xs: 12, md: 5 }}>
              <Typography
                variant="overline"
                sx={{ color: palette.brand.primary, letterSpacing: 3, display: 'block', mb: 2 }}
              >
                {t('howItWorksPage.sellersLabel')}
              </Typography>
              <Typography variant="h3" sx={{ color: palette.neutral[50], fontWeight: 400, mb: 3 }}>
                {t('howItWorksPage.sellersTitle')}
              </Typography>
              <Typography
                variant="body1"
                sx={{ color: 'rgba(250,250,249,0.7)', mb: 4, lineHeight: 1.8 }}
              >
                {t('howItWorksPage.sellersSubtitle')}
              </Typography>
              <Button
                variant="outlined"
                endIcon={<ArrowForward />}
                component={Link}
                to="/sell"
                sx={{
                  borderColor: palette.brand.primary,
                  color: palette.brand.primary,
                  px: 4,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  '&:hover': {
                    borderColor: palette.brand.primary,
                    bgcolor: 'rgba(202,138,4,0.1)',
                  },
                }}
              >
                {t('howItWorksPage.startSelling')}
              </Button>
            </Grid>
            <Grid size={{ xs: 12, md: 7 }}>
              <Stack spacing={0}>
                {sellerSteps.map((step, index) => (
                  <Box
                    key={index}
                    sx={{
                      display: 'flex',
                      gap: 3,
                      py: 3,
                      borderBottom:
                        index < sellerSteps.length - 1 ? '1px solid rgba(250,250,249,0.1)' : 'none',
                    }}
                  >
                    <Typography
                      variant="h5"
                      sx={{ color: palette.brand.primary, fontWeight: 600, minWidth: 40 }}
                    >
                      {step.number}
                    </Typography>
                    <Box>
                      <Typography
                        variant="h6"
                        sx={{ color: palette.neutral[50], fontWeight: 500, mb: 0.5 }}
                      >
                        {t(`howItWorksPage.sellerSteps.${step.key}.title`)}
                      </Typography>
                      <Typography variant="body2" sx={{ color: 'rgba(250,250,249,0.6)' }}>
                        {t(`howItWorksPage.sellerSteps.${step.key}.description`)}
                      </Typography>
                    </Box>
                  </Box>
                ))}
              </Stack>
            </Grid>
          </Grid>
        </Container>
      </Box>

      <Box sx={{ py: { xs: 10, md: 14 } }}>
        <Container maxWidth="md">
          <Box sx={{ textAlign: 'center', p: { xs: 6, md: 10 }, bgcolor: '#FFFFFF' }}>
            <Typography variant="h4" sx={{ color: palette.neutral[900], fontWeight: 400, mb: 2 }}>
              {t('howItWorksPage.ctaTitle')}
            </Typography>
            <Typography
              variant="body1"
              sx={{ color: palette.neutral[700], mb: 4, maxWidth: 500, mx: 'auto' }}
            >
              {t('howItWorksPage.ctaSubtitle')}
            </Typography>
            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
              <Button
                variant="contained"
                size="large"
                endIcon={<ArrowForward />}
                component={Link}
                to="/register"
                sx={{
                  bgcolor: palette.brand.primary,
                  color: palette.neutral[50],
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': { bgcolor: '#A16207' },
                }}
              >
                {t('howItWorksPage.ctaButton')}
              </Button>
              <Button
                variant="outlined"
                size="large"
                component={Link}
                to="/auctions"
                sx={{
                  borderColor: palette.neutral[900],
                  color: palette.neutral[900],
                  px: 5,
                  py: 1.5,
                  textTransform: 'none',
                  borderRadius: 0,
                  fontWeight: 500,
                  '&:hover': {
                    borderColor: palette.neutral[900],
                    bgcolor: 'rgba(28,25,23,0.05)',
                  },
                }}
              >
                {t('howItWorksPage.ctaSecondary')}
              </Button>
            </Box>
          </Box>
        </Container>
      </Box>
    </Box>
  )
}
