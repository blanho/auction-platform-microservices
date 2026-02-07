import { motion } from 'framer-motion'
import { Box, Container, Typography } from '@mui/material'
import { useMemo } from 'react'
import { useBrands } from '@/modules/auctions/hooks/useBrands'
import { colors, typography, transitions } from '@/shared/theme/tokens'

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
    <Box
      sx={{
        py: { xs: 8, md: 12 },
        bgcolor: colors.background.primary,
        borderTop: `1px solid ${colors.border.subtle}`,
        borderBottom: `1px solid ${colors.border.subtle}`,
      }}
    >
      <Container maxWidth="lg">
        <motion.div
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ duration: transitions.duration.normal }}
        >
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography
              variant="overline"
              sx={{
                color: colors.text.disabled,
                letterSpacing: 4,
                fontFamily: typography.fontFamily.body,
              }}
            >
              TRUSTED BY LEADING AUCTION HOUSES
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
                transition={{ delay: index * 0.05, duration: transitions.duration.normal }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    opacity: 0.5,
                    transition: `opacity ${transitions.duration.normal}s ease`,
                    cursor: 'pointer',
                    '&:hover': {
                      opacity: 1,
                    },
                  }}
                >
                  {brand.logoUrl ? (
                    <Box
                      component="img"
                      src={brand.logoUrl}
                      alt={brand.name}
                      sx={{
                        width: 40,
                        height: 40,
                        borderRadius: 1,
                        border: `1px solid ${colors.border.light}`,
                        objectFit: 'contain',
                        bgcolor: colors.background.primary,
                      }}
                    />
                  ) : (
                    <Box
                      sx={{
                        width: 40,
                        height: 40,
                        borderRadius: 1,
                        border: `1px solid ${colors.border.light}`,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        fontFamily: typography.fontFamily.display,
                        fontWeight: typography.fontWeight.bold,
                        fontSize: '1.25rem',
                        color: colors.text.primary,
                      }}
                    >
                      {brand.name.charAt(0)}
                    </Box>
                  )}
                  <Typography
                    sx={{
                      fontFamily: typography.fontFamily.body,
                      fontSize: '0.875rem',
                      fontWeight: typography.fontWeight.medium,
                      color: colors.text.primary,
                      letterSpacing: 1,
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
