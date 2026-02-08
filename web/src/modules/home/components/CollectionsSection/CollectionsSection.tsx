import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { useMemo } from 'react'
import { useActiveCategories } from '@/modules/auctions/hooks/useCategories'
import { typography } from '@/shared/theme/tokens'

export const CollectionsSection = () => {
  const { data: categoriesData } = useActiveCategories()
  const collections = useMemo(() => {
    const source = categoriesData ?? []
    const withImages = source.filter((category) => category.imageUrl)
    const selected = (withImages.length ? withImages : source).slice(0, 4)
    return selected
  }, [categoriesData])

  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FFFFFF', position: 'relative' }}>
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, height: '1px', bgcolor: '#E7E5E4' }} />

      <Container maxWidth="lg" sx={{ position: 'relative' }}>
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.6 }}
        >
          <Box sx={{ textAlign: 'center', mb: 10 }}>
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
              Curated Collections
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
              Featured Galleries
            </Typography>
          </Box>
        </motion.div>

        {collections.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography sx={{ color: '#A8A29E', fontSize: '0.9rem' }}>
              No collections available right now.
            </Typography>
          </Box>
        ) : (
          <Grid container spacing={3}>
            {collections.map((collection, index) => (
              <Grid size={{ xs: 12, sm: 6, md: 3 }} key={collection.id}>
                <motion.div
                  initial={{ opacity: 0, y: 25 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  transition={{ delay: index * 0.1, duration: 0.5 }}
                >
                  <Box
                    component={Link}
                    to={`/auctions?categoryId=${collection.id}`}
                    sx={{
                      display: 'block',
                      position: 'relative',
                      overflow: 'hidden',
                      cursor: 'pointer',
                      textDecoration: 'none',
                      '&:hover .collection-image': { transform: 'scale(1.04)' },
                    }}
                  >
                    {collection.imageUrl ? (
                      <Box
                        className="collection-image"
                        sx={{
                          width: '100%',
                          aspectRatio: '3/4',
                          backgroundImage: `url(${collection.imageUrl})`,
                          backgroundSize: 'cover',
                          backgroundPosition: 'center',
                          transition: 'transform 0.6s ease',
                        }}
                      />
                    ) : (
                      <Box
                        className="collection-image"
                        sx={{
                          width: '100%',
                          aspectRatio: '3/4',
                          bgcolor: '#F5F5F4',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          fontFamily: typography.fontFamily.display,
                          fontSize: '3rem',
                          fontWeight: typography.fontWeight.light,
                          color: '#A8A29E',
                          transition: 'transform 0.6s ease',
                        }}
                      >
                        {collection.name.charAt(0)}
                      </Box>
                    )}
                    <Box
                      sx={{
                        position: 'absolute',
                        inset: 0,
                        background: 'linear-gradient(to top, rgba(0,0,0,0.7) 0%, rgba(0,0,0,0.1) 50%, transparent 100%)',
                      }}
                    />
                    <Box sx={{ position: 'absolute', bottom: 0, left: 0, right: 0, p: 3 }}>
                      <Typography
                        sx={{
                          fontFamily: typography.fontFamily.display,
                          color: '#FFFFFF',
                          fontWeight: typography.fontWeight.medium,
                          fontSize: '1.125rem',
                          mb: 0.5,
                        }}
                      >
                        {collection.name}
                      </Typography>
                      <Typography sx={{ color: 'rgba(255,255,255,0.7)', fontSize: '0.75rem' }}>
                        {collection.auctionCount ?? 0} items
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
