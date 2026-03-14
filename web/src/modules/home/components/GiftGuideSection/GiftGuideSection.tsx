import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { typography } from '@/shared/theme/tokens'
import { useTranslation } from 'react-i18next'

const giftCategories = [
  {
    id: 'for-her',
    titleKey: 'giftGuide.forHer',
    image: 'https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=400&q=80',
    link: '/auctions?category=women',
  },
  {
    id: 'jewelry',
    titleKey: 'giftGuide.jewelry',
    image: 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?w=400&q=80',
    link: '/auctions?category=jewelry',
  },
  {
    id: 'for-him',
    titleKey: 'giftGuide.forHim',
    image: 'https://images.unsplash.com/photo-1511499767150-a48a237f0083?w=400&q=80',
    link: '/auctions?category=men',
  },
  {
    id: 'accessories',
    titleKey: 'giftGuide.accessories',
    image: 'https://images.unsplash.com/photo-1606760227091-3dd870d97f1d?w=400&q=80',
    link: '/auctions?category=accessories',
  },
  {
    id: 'watches',
    titleKey: 'giftGuide.watches',
    image: 'https://images.unsplash.com/photo-1587836374828-4dbafa94cf0e?w=400&q=80',
    link: '/auctions?category=watches',
  },
]

export const GiftGuideSection = () => {
  const { t } = useTranslation('home')

  return (
    <Box sx={{ py: { xs: 10, md: 14 }, bgcolor: '#FFFFFF' }}>
      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
        >
          <Box sx={{ textAlign: 'center', mb: { xs: 6, md: 8 } }}>
            <Typography
              variant="h2"
              sx={{
                fontFamily: typography.fontFamily.display,
                fontWeight: typography.fontWeight.regular,
                fontSize: { xs: '1.75rem', md: '2.5rem' },
                color: '#1C1917',
                mb: 2,
              }}
            >
              {t('giftGuide.title')}
            </Typography>
            <Typography
              sx={{
                fontFamily: typography.fontFamily.body,
                fontSize: { xs: '0.9375rem', md: '1rem' },
                color: '#57534E',
                maxWidth: 600,
                mx: 'auto',
              }}
            >
              {t('giftGuide.subtitle')}
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={{ xs: 2, md: 3 }}>
          {giftCategories.map((category, index) => (
            <Grid size={{ xs: 6, sm: 4, md: 2.4 }} key={category.id}>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.08, duration: 0.5 }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 2,
                  }}
                >
                  <Box
                    component={Link}
                    to={category.link}
                    sx={{
                      display: 'block',
                      width: '100%',
                      aspectRatio: '1',
                      overflow: 'hidden',
                      bgcolor: '#F5F5F4',
                      cursor: 'pointer',
                      '&:hover img': {
                        transform: 'scale(1.05)',
                      },
                    }}
                  >
                    <Box
                      component="img"
                      src={category.image}
                      alt={t(category.titleKey)}
                      sx={{
                        width: '100%',
                        height: '100%',
                        objectFit: 'cover',
                        transition: 'transform 0.5s ease',
                      }}
                    />
                  </Box>
                  <Button
                    component={Link}
                    to={category.link}
                    variant="outlined"
                    fullWidth
                    sx={{
                      borderColor: '#1C1917',
                      color: '#1C1917',
                      borderRadius: 0,
                      py: 1.25,
                      fontSize: '0.6875rem',
                      fontWeight: typography.fontWeight.medium,
                      letterSpacing: '0.15em',
                      textTransform: 'uppercase',
                      fontFamily: typography.fontFamily.body,
                      '&:hover': {
                        borderColor: '#1C1917',
                        bgcolor: '#1C1917',
                        color: '#FFFFFF',
                      },
                    }}
                  >
                    {t(category.titleKey)}
                  </Button>
                </Box>
              </motion.div>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  )
}
