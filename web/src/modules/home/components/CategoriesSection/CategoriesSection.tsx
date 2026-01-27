import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { KeyboardArrowRight } from '@mui/icons-material'
import { FEATURED_CATEGORIES } from '../../data'
import { colors, gradients, typography, transitions } from '@/shared/theme/tokens'

export const CategoriesSection = () => {
  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: colors.background.primary, position: 'relative' }}>
      <Box
        sx={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          height: 1,
          background: gradients.horizontalDivider,
        }}
      />
      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: transitions.duration.normal }}
        >
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'flex-end',
              mb: 6,
              flexWrap: 'wrap',
              gap: 2,
            }}
          >
            <Box>
              <Typography
                variant="overline"
                sx={{
                  color: colors.gold.primary,
                  letterSpacing: 4,
                  display: 'block',
                  mb: 1,
                  fontFamily: typography.fontFamily.body,
                }}
              >
                EXPLORE
              </Typography>
              <Typography
                variant="h2"
                sx={{
                  fontFamily: typography.fontFamily.display,
                  color: colors.text.primary,
                  fontWeight: typography.fontWeight.regular,
                  fontSize: { xs: '2rem', md: '3rem' },
                }}
              >
                Categories
              </Typography>
            </Box>
            <Button
              endIcon={<KeyboardArrowRight />}
              component={Link}
              to="/categories"
              sx={{
                color: colors.text.secondary,
                textTransform: 'none',
                fontWeight: typography.fontWeight.medium,
                fontFamily: typography.fontFamily.body,
                '&:hover': { color: colors.gold.primary },
              }}
            >
              View All Categories
            </Button>
          </Box>
        </motion.div>

        <Grid container spacing={2}>
          {FEATURED_CATEGORIES.map((category, index) => (
            <Grid size={{ xs: 6, sm: 4, md: 2 }} key={category.id}>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1, duration: transitions.duration.normal }}
              >
                <Box
                  component={Link}
                  to={`/auctions?category=${category.id}`}
                  sx={{
                    display: 'block',
                    position: 'relative',
                    aspectRatio: '1',
                    overflow: 'hidden',
                    borderRadius: 3,
                    cursor: 'pointer',
                    textDecoration: 'none',
                    '&:hover img': { transform: 'scale(1.1)' },
                    '&:hover .category-overlay': { opacity: 1 },
                  }}
                >
                  <Box
                    component="img"
                    src={category.image || `https://picsum.photos/400/400?random=${index}`}
                    alt={category.name}
                    sx={{
                      width: '100%',
                      height: '100%',
                      objectFit: 'cover',
                      transition: `transform ${transitions.duration.slow}s ease`,
                    }}
                  />
                  <Box
                    sx={{
                      position: 'absolute',
                      inset: 0,
                      background: `linear-gradient(to top, rgba(0,0,0,0.8) 0%, rgba(0,0,0,0.2) 50%, transparent 100%)`,
                    }}
                  />
                  <Box
                    className="category-overlay"
                    sx={{
                      position: 'absolute',
                      inset: 0,
                      background: category.gradient,
                      opacity: 0,
                      transition: `opacity ${transitions.duration.normal}s ease`,
                      mixBlendMode: 'overlay',
                    }}
                  />
                  <Box
                    sx={{
                      position: 'absolute',
                      bottom: 0,
                      left: 0,
                      right: 0,
                      p: 2,
                    }}
                  >
                    <Typography
                      variant="subtitle1"
                      sx={{
                        color: colors.text.primary,
                        fontWeight: typography.fontWeight.semibold,
                        fontFamily: typography.fontFamily.body,
                      }}
                    >
                      {category.name}
                    </Typography>
                    <Typography variant="caption" sx={{ color: colors.text.subtle }}>
                      {category.count} items
                    </Typography>
                  </Box>
                </Box>
              </motion.div>
            </Grid>
          ))}
        </Grid>
      </Container>
    </Box>
  )
}
