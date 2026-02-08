import { motion } from 'framer-motion'
import { Box, Container, Typography } from '@mui/material'
import { useMemo } from 'react'
import { useBrands } from '@/modules/auctions/hooks/useBrands'
import { typography } from '@/shared/theme/tokens'

export const TrustedBrandsSection = () => {
  const { data: featuredBrandsData } = useBrands({ activeOnly: true, featuredOnly: true, page: 1, pageSize: 8 })
  const { data: activeBrandsData } = useBrands({ activeOnly: true, page: 1, pageSize: 8 })

  const brands = useMemo(() => {
    const featured = featuredBrandsData?.items ?? []
    const active = activeBrandsData?.items ?? []
    return featured.length > 0 ? featured : active
  }, [featuredBrandsData?.items, activeBrandsData?.items])

  if (brands.length === 0) {
    return null
  }

  return (
    <Box sx={{ py: { xs: 8, md: 12 }, bgcolor: '#FFFFFF', position: 'relative' }}>
      <Box sx={{ position: 'absolute', top: 0, left: 0, right: 0, height: '1px', bgcolor: '#E7E5E4' }} />

      <Container maxWidth="lg">
        <motion.div
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography
              sx={{
                color: '#A8A29E',
                letterSpacing: '0.2em',
                fontFamily: typography.fontFamily.body,
                fontSize: '0.625rem',
                fontWeight: typography.fontWeight.medium,
                textTransform: 'uppercase',
              }}
            >
              Trusted by Leading Auction Houses
            </Typography>
          </Box>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'center',
              alignItems: 'center',
              flexWrap: 'wrap',
              gap: { xs: 4, md: 8 },
            }}
          >
            {brands.map((brand, index) => (
              <motion.div
                key={brand.id}
                initial={{ opacity: 0, y: 10 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.05, duration: 0.4 }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    opacity: 0.4,
                    transition: 'opacity 0.3s ease',
                    cursor: 'pointer',
                    '&:hover': { opacity: 1 },
                  }}
                >
                  {brand.logoUrl ? (
                    <Box
                      component="img"
                      src={brand.logoUrl}
                      alt={brand.name}
                      sx={{
                        width: 36,
                        height: 36,
                        objectFit: 'contain',
                        filter: 'grayscale(100%)',
                      }}
                    />
                  ) : (
                    <Box
                      sx={{
                        width: 36,
                        height: 36,
                        border: '1px solid #E7E5E4',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontFamily: typography.fontFamily.display,
                        fontWeight: typography.fontWeight.medium,
                        fontSize: '1.125rem',
                        color: '#1C1917',
                      }}
                    >
                      {brand.name.charAt(0)}
                    </Box>
                  )}
                  <Typography
                    sx={{
                      fontFamily: typography.fontFamily.body,
                      fontSize: '0.8125rem',
                      fontWeight: typography.fontWeight.medium,
                      color: '#1C1917',
                      letterSpacing: '0.05em',
                      display: { xs: 'none', sm: 'block' },
                    }}
                  >
                    {brand.name}
                  </Typography>
                </Box>
              </motion.div>
            ))}
          </Box>
        </motion.div>
      </Container>
    </Box>
  )
}
