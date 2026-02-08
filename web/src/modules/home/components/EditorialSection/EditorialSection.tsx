import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button } from '@mui/material'
import { typography } from '@/shared/theme/tokens'

const editorialImages = [
  'https://images.unsplash.com/photo-1509631179647-0177331693ae?w=600&q=80',
  'https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&q=80',
  'https://images.unsplash.com/photo-1469334031218-e382a71b716b?w=600&q=80',
  'https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=600&q=80',
]

interface EditorialSectionProps {
  title?: string
  subtitle?: string
  buttonText?: string
  buttonLink?: string
}

export const EditorialSection = ({
  title = 'Timeless Elegance',
  subtitle = 'Discover pieces that transcend seasons. The key is investing in quality that lasts generations.',
  buttonText = 'Shop The Edit',
  buttonLink = '/auctions?featured=true',
}: EditorialSectionProps) => {
  return (
    <Box sx={{ py: { xs: 10, md: 14 }, bgcolor: '#F5F5F4' }}>
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
              Offbeat Elements
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
              One unexpected texture or detail can transform an entire look.
            </Typography>
          </Box>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.8, delay: 0.2 }}
        >
          <Box
            sx={{
              position: 'relative',
              display: 'grid',
              gridTemplateColumns: { xs: '1fr 1fr', md: 'repeat(4, 1fr)' },
              gap: 0,
              overflow: 'hidden',
            }}
          >
            {editorialImages.map((image, index) => (
              <Box
                key={index}
                sx={{
                  aspectRatio: { xs: '3/4', md: '3/4' },
                  overflow: 'hidden',
                  cursor: 'pointer',
                  '&:hover img': {
                    transform: 'scale(1.03)',
                  },
                }}
              >
                <Box
                  component="img"
                  src={image}
                  alt={`Editorial ${index + 1}`}
                  sx={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover',
                    transition: 'transform 0.6s ease',
                  }}
                />
              </Box>
            ))}

            <Box
              sx={{
                position: 'absolute',
                top: '50%',
                right: { xs: '5%', md: '10%' },
                transform: 'translateY(-50%)',
                bgcolor: '#FFFFFF',
                p: { xs: 3, md: 5 },
                maxWidth: { xs: 200, md: 320 },
                textAlign: 'center',
                zIndex: 10,
              }}
            >
              <Typography
                sx={{
                  fontFamily: typography.fontFamily.display,
                  fontWeight: typography.fontWeight.regular,
                  fontSize: { xs: '1.25rem', md: '1.75rem' },
                  color: '#1C1917',
                  mb: 2,
                }}
              >
                {title}
              </Typography>
              <Typography
                sx={{
                  fontFamily: typography.fontFamily.body,
                  fontSize: { xs: '0.8125rem', md: '0.9375rem' },
                  color: '#57534E',
                  mb: 3,
                  lineHeight: 1.6,
                }}
              >
                {subtitle}
              </Typography>
              <Button
                component={Link}
                to={buttonLink}
                variant="outlined"
                sx={{
                  borderColor: '#1C1917',
                  color: '#1C1917',
                  borderRadius: 0,
                  px: { xs: 3, md: 4 },
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
                {buttonText}
              </Button>
            </Box>
          </Box>
        </motion.div>
      </Container>
    </Box>
  )
}
