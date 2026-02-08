import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Grid } from '@mui/material'
import { East } from '@mui/icons-material'
import { useFeaturedAuctions } from '@/modules/auctions/hooks/useAuctions'
import { typography } from '@/shared/theme/tokens'
import { formatCurrency } from '@/shared/utils/formatters'

export const FreshArrivalsSection = () => {
  const { data: auctionsData } = useFeaturedAuctions(6)
  const auctions = auctionsData?.items ?? []

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
              So Much New
            </Typography>
            <Typography
              sx={{
                fontFamily: typography.fontFamily.body,
                fontSize: { xs: '0.9375rem', md: '1rem' },
                color: '#57534E',
              }}
            >
              10,000+ fresh finds are in (and they're really, really good).
            </Typography>
          </Box>
        </motion.div>

        {auctions.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <Typography sx={{ color: '#A8A29E', fontSize: '0.9375rem' }}>
              New arrivals coming soon.
            </Typography>
          </Box>
        ) : (
          <>
            <Grid container spacing={{ xs: 2, md: 3 }}>
              {auctions.map((auction, index) => (
                <Grid size={{ xs: 6, sm: 4, md: 2 }} key={auction.id}>
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    whileInView={{ opacity: 1, y: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.05, duration: 0.4 }}
                  >
                    <Box
                      component={Link}
                      to={`/auctions/${auction.id}`}
                      sx={{
                        display: 'block',
                        textDecoration: 'none',
                        cursor: 'pointer',
                        '&:hover .auction-image': {
                          transform: 'scale(1.03)',
                        },
                        '&:hover .auction-title': {
                          color: '#78716C',
                        },
                      }}
                    >
                      <Box
                        sx={{
                          aspectRatio: '3/4',
                          overflow: 'hidden',
                          bgcolor: '#F5F5F4',
                          mb: 2,
                        }}
                      >
                        {auction.imageUrl ? (
                          <Box
                            component="img"
                            className="auction-image"
                            src={auction.imageUrl}
                            alt={auction.title}
                            sx={{
                              width: '100%',
                              height: '100%',
                              objectFit: 'cover',
                              transition: 'transform 0.5s ease',
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
                              color: '#A8A29E',
                              fontFamily: typography.fontFamily.display,
                              fontSize: '2rem',
                            }}
                          >
                            {auction.title.charAt(0)}
                          </Box>
                        )}
                      </Box>
                      <Typography
                        className="auction-title"
                        sx={{
                          fontFamily: typography.fontFamily.body,
                          fontSize: '0.8125rem',
                          fontWeight: typography.fontWeight.medium,
                          color: '#1C1917',
                          mb: 0.5,
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap',
                          transition: 'color 0.2s ease',
                        }}
                      >
                        {auction.title}
                      </Typography>
                      <Typography
                        sx={{
                          fontFamily: typography.fontFamily.body,
                          fontSize: '0.75rem',
                          color: '#78716C',
                        }}
                      >
                        {formatCurrency(auction.currentPrice || auction.startingPrice)}
                      </Typography>
                    </Box>
                  </motion.div>
                </Grid>
              ))}
            </Grid>

            <Box sx={{ textAlign: 'center', mt: { xs: 6, md: 8 } }}>
              <Button
                component={Link}
                to="/auctions?sort=newest"
                endIcon={<East />}
                sx={{
                  color: '#1C1917',
                  textTransform: 'uppercase',
                  fontWeight: typography.fontWeight.medium,
                  fontFamily: typography.fontFamily.body,
                  fontSize: '0.75rem',
                  letterSpacing: '0.1em',
                  '&:hover': { color: '#78716C', bgcolor: 'transparent' },
                }}
              >
                Shop All New Arrivals
              </Button>
            </Box>
          </>
        )}
      </Container>
    </Box>
  )
}
