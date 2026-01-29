import { Link } from 'react-router-dom'
import { Box, Container, Typography, Grid, Stack, Divider } from '@mui/material'
import { Gavel } from '@mui/icons-material'
import { colors, gradients, typography, transitions } from '@/shared/theme/tokens'

const footerLinks = {
  explore: ['All Auctions', 'Categories', 'Ending Soon', 'New Arrivals'],
  sell: ['Start Selling', 'Seller Guide', 'Fees & Pricing', 'Success Stories'],
  support: ['Help Center', 'Contact Us', 'Shipping Info', 'Returns'],
  company: ['About Us', 'Careers', 'Press', 'Blog'],
}

const footerRoutes: Record<string, string> = {
  explore: '/auctions',
  sell: '/sell',
  support: '/help',
  company: '/about',
}

export const Footer = () => {
  return (
    <Box
      component="footer"
      sx={{
        py: 8,
        bgcolor: colors.background.tertiary,
        borderTop: `1px solid ${colors.border.subtle}`,
      }}
    >
      <Container maxWidth="xl">
        <Grid container spacing={6}>
          <Grid size={{ xs: 12, md: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 3 }}>
              <Box
                sx={{
                  width: 40,
                  height: 40,
                  borderRadius: 1.5,
                  background: gradients.goldButton,
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                }}
              >
                <Gavel sx={{ fontSize: 24, color: colors.background.primary }} />
              </Box>
              <Typography
                variant="h6"
                sx={{
                  fontFamily: typography.fontFamily.display,
                  fontWeight: typography.fontWeight.semibold,
                  color: colors.text.primary,
                  letterSpacing: 1,
                }}
              >
                AUCTION
              </Typography>
            </Box>
            <Typography
              variant="body2"
              sx={{ color: colors.text.disabled, mb: 3, maxWidth: 300, lineHeight: 1.7 }}
            >
              The premier destination for collectors and connoisseurs. Discover authenticated
              treasures from trusted sellers worldwide.
            </Typography>
          </Grid>

          {Object.entries(footerLinks).map(([category, items]) => (
            <Grid size={{ xs: 6, sm: 3, md: 2 }} key={category}>
              <Typography
                variant="subtitle2"
                sx={{
                  fontWeight: typography.fontWeight.semibold,
                  mb: 2.5,
                  color: colors.text.primary,
                  textTransform: 'capitalize',
                }}
              >
                {category}
              </Typography>
              <Stack spacing={1.5}>
                {items.map((item) => (
                  <Typography
                    key={item}
                    component={Link}
                    to={footerRoutes[category]}
                    variant="body2"
                    sx={{
                      color: colors.text.disabled,
                      textDecoration: 'none',
                      cursor: 'pointer',
                      transition: `color ${transitions.duration.fast}s`,
                      '&:hover': { color: colors.gold.primary },
                    }}
                  >
                    {item}
                  </Typography>
                ))}
              </Stack>
            </Grid>
          ))}
        </Grid>

        <Divider sx={{ borderColor: colors.border.subtle, my: 6 }} />

        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 2,
          }}
        >
          <Typography variant="caption" sx={{ color: colors.text.faint }}>
            © {new Date().getFullYear()} Auction Platform. All rights reserved.
          </Typography>
          <Box sx={{ display: 'flex', gap: 3 }}>
            {['Privacy Policy', 'Terms of Service', 'Cookie Policy'].map((item) => (
              <Typography
                key={item}
                variant="caption"
                sx={{
                  color: colors.text.faint,
                  cursor: 'pointer',
                  transition: `color ${transitions.duration.fast}s`,
                  '&:hover': { color: colors.text.primary },
                }}
              >
                {item}
              </Typography>
            ))}
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
