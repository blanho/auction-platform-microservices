import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { East } from '@mui/icons-material'
import { useMemo } from 'react'
import { useActiveCategories } from '@/modules/auctions/hooks/useCategories'
import { typography } from '@/shared/theme/tokens'

export const CategoriesSection = () => {
  const { data: categoriesData } = useActiveCategories()
  const categories = useMemo(() => (categoriesData ?? []).slice(0, 6), [categoriesData])

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FAFAF9', position: 'relative' }}>
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, height: '1px', bgcolor: '#E7E5E4' }} />
      <Container maxWidth="xl">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
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
                sx={{
                  color: '#78716C',
                  letterSpacing: '0.2em',
                  display: 'block',
                  mb: 2,
                  fontFamily: typography.fontFamily.body,
                  fontSize: '0.6875rem',
                  fontWeight: typography.fontWeight.medium,
                  textTransform: 'uppercase',
                }}
              >
                Explore
              </Typography>
              <Typography
                variant="h2"
                sx={{
                  fontFamily: typography.fontFamily.display,
                  color: '#1C1917',
                  fontWeight: typography.fontWeight.regular,
                  fontSize: { xs: '1.75rem', md: '2.5rem' },
                }}
              >
                Browse by Category
              </Typography>
            </Box>
            <Button
              endIcon={<East />}
              component={Link}
              to="/categories"
              sx={{
                color: '#1C1917',
                textTransform: 'uppercase',
                fontWeight: typography.fontWeight.medium,
                fontFamily: typography.fontFamily.body,
                fontSize: '0.75rem',
                letterSpacing: '0.1em',
                '&:hover': { color: '#78716C' },
              }}
            >
              All Categories
            </Button>
          </Box>
        </motion.div>

        {categories.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography variant="body2" sx={{ color: '#A8A29E' }}>
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
                  transition={{ delay: index * 0.08, duration: 0.5 }}
                >
                  <Box
                    component={Link}
                    to={`/auctions?categoryId=${category.id}`}
                    sx={{
                      display: 'block',
                      position: 'relative',
                      aspectRatio: '1',
                      overflow: 'hidden',
                      cursor: 'pointer',
                      textDecoration: 'none',
                      '&:hover img, &:hover .category-placeholder': {
                        transform: 'scale(1.05)',
                      },
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
                          transition: 'transform 0.6s ease',
                        }}
                      />
                    ) : (
                      <Box
                        className="category-placeholder"
                        sx={{
                          width: '100%',
                          height: '100%',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          bgcolor: '#F5F5F4',
                          color: '#A8A29E',
                          fontFamily: typography.fontFamily.display,
                          fontSize: '2.5rem',
                          fontWeight: typography.fontWeight.light,
                          transition: 'transform 0.6s ease',
                        }}
                      >
                        {category.name.charAt(0)}
                      </Box>
                    )}
                    <Box
                      sx={{
                        position: 'absolute',
                        inset: 0,
                        background: 'linear-gradient(to top, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0.1) 50%, transparent 100%)',
                      }}
                    />
                    <Box sx={{ position: 'absolute', bottom: 0, left: 0, right: 0, p: 2 }}>
                      <Typography
                        sx={{
                          color: '#FFFFFF',
                          fontWeight: typography.fontWeight.medium,
                          fontFamily: typography.fontFamily.body,
                          fontSize: '0.8125rem',
                          lineHeight: 1.3,
                        }}
                      >
                        {category.name}
                      </Typography>
                      <Typography sx={{ color: 'rgba(255,255,255,0.7)', fontSize: '0.6875rem', mt: 0.5 }}>
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
