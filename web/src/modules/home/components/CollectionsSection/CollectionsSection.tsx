import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid } from '@mui/material'
import { FEATURED_COLLECTIONS } from '../../data'
import { colors, typography, transitions } from '@/shared/theme/tokens'

export const CollectionsSection = () => {
  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: colors.background.secondary }}>
      <Container maxWidth="lg">
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: transitions.duration.normal }}
        >
          <Box sx={{ textAlign: 'center', mb: 8 }}>
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
              CURATED COLLECTIONS
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
              Featured Galleries
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={3}>
          {FEATURED_COLLECTIONS.map((collection, index) => (
            <Grid size={{ xs: 12, sm: 6, md: 3 }} key={collection.id}>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.1, duration: transitions.duration.normal }}
              >
                <Box
                  component={Link}
                  to={`/collections/${collection.id}`}
                  sx={{
                    display: 'block',
                    position: 'relative',
                    borderRadius: 3,
                    overflow: 'hidden',
                    cursor: 'pointer',
                    textDecoration: 'none',
                    '&:hover .collection-image': {
                      transform: 'scale(1.05)',
                    },
                    '&:hover .collection-overlay': {
                      background:
                        'linear-gradient(to top, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.3) 50%, transparent 100%)',
                    },
                  }}
                >
                  <Box
                    className="collection-image"
                    sx={{
                      width: '100%',
                      aspectRatio: '3/4',
                      backgroundImage: `url(${collection.image})`,
                      backgroundSize: 'cover',
                      backgroundPosition: 'center',
                      transition: `transform ${transitions.duration.slow}s ease`,
                    }}
                  />
                  <Box
                    className="collection-overlay"
                    sx={{
                      position: 'absolute',
                      inset: 0,
                      background:
                        'linear-gradient(to top, rgba(0,0,0,0.8) 0%, rgba(0,0,0,0.2) 50%, transparent 100%)',
                      transition: `background ${transitions.duration.normal}s ease`,
                    }}
                  />
                  <Box
                    sx={{
                      position: 'absolute',
                      bottom: 0,
                      left: 0,
                      right: 0,
                      p: 3,
                    }}
                  >
                    <Typography
                      variant="h6"
                      sx={{
                        fontFamily: typography.fontFamily.display,
                        color: colors.text.primary,
                        fontWeight: typography.fontWeight.semibold,
                        fontSize: '1.25rem',
                        mb: 1,
                      }}
                    >
                      {collection.name}
                    </Typography>
                    <Box
                      sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
                    >
                      <Typography variant="caption" sx={{ color: colors.text.subtle }}>
                        {collection.itemCount} items
                      </Typography>
                      <Typography variant="caption" sx={{ color: colors.gold.primary, fontWeight: typography.fontWeight.semibold }}>
                        {collection.totalValue}
                      </Typography>
                    </Box>
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
