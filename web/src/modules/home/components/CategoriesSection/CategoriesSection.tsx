import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { KeyboardArrowRight } from '@mui/icons-material'
import { useMemo } from 'react'
import { useActiveCategories } from '@/modules/auctions/hooks/useCategories'
import { colors, gradients, typography, transitions } from '@/shared/theme/tokens'

export const CategoriesSection = () => {
  const { data: categoriesData } = useActiveCategories()
  const categories = useMemo(() => (categoriesData ?? []).slice(0, 6), [categoriesData])

  const overlayGradients = [
    'linear-gradient(135deg, rgba(102,126,234,0.7) 0%, rgba(118,75,162,0.7) 100%)',
    'linear-gradient(135deg, rgba(240,147,251,0.7) 0%, rgba(245,87,108,0.7) 100%)',
    'linear-gradient(135deg, rgba(79,172,254,0.7) 0%, rgba(0,242,254,0.7) 100%)',
    'linear-gradient(135deg, rgba(67,233,123,0.7) 0%, rgba(56,249,215,0.7) 100%)',
    'linear-gradient(135deg, rgba(250,112,154,0.7) 0%, rgba(254,225,64,0.7) 100%)',
    'linear-gradient(135deg, rgba(161,140,209,0.7) 0%, rgba(251,194,235,0.7) 100%)',
  ]

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

        {categories.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              No categories available right now.
            </Typography>
          </Box>
        ) : (
          <Grid container spacing={2}>
            {categories.map((category, index) => (
              <Grid size={{ xs: 6, sm: 4, md: 2 }} key={category.id}>
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1, duration: transitions.duration.normal }}
                >
                  <Box
                    component={Link}
                    to={`/auctions?categoryId=${category.id}`}
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
                    {category.imageUrl ? (
                      <Box
                        component="img"
                        src={category.imageUrl}
                        alt={category.name}
                        sx={{
                          width: '100%',
                          height: '100%',
                          objectFit: 'cover',
                          transition: `transform ${transitions.duration.slow}s ease`,
                        }}
                      />
                    ) : (
                      <Box
                        sx={{
                          width: '100%',
                          height: '100%',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          bgcolor: colors.background.secondary,
                          color: colors.text.primary,
                          fontFamily: typography.fontFamily.display,
                          fontSize: '2rem',
                          fontWeight: typography.fontWeight.semibold,
                          transition: `transform ${transitions.duration.slow}s ease`,
                        }}
                      >
                        {category.name.charAt(0)}
                      </Box>
                    )}
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
                        background: overlayGradients[index % overlayGradients.length],
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
                        {category.auctionCount ?? 0} items
                      </Typography>
                    </Box>
                  </Box>
                </motion.div>
              </Grid>
            ))}
          </Grid>
        )}
      </Container>
    </Box>
  )
}
