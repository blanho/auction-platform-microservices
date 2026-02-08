import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Box, Container, Typography, Button, Stack } from '@mui/material'
import { East } from '@mui/icons-material'
import { typography } from '@/shared/theme/tokens'

export const CTASection = () => {
  return (
    <Box sx={{ py: { xs: 12, md: 20 }, bgcolor: '#1C1917', position: 'relative' }}>
      <Container maxWidth="md" sx={{ position: 'relative' }}>
        <motion.div
          initial={{ opacity: 0, y: 40 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: '-100px' }}
          transition={{ duration: 0.7 }}
        >
          <Box sx={{ textAlign: 'center' }}>
            <Typography
              sx={{
                color: 'rgba(255,255,255,0.5)',
                letterSpacing: '0.2em',
                fontSize: '0.6875rem',
                fontWeight: typography.fontWeight.medium,
                textTransform: 'uppercase',
                mb: 4,
              }}
            >
              Start Your Collection
            </Typography>
            <Typography
              variant="h3"
              sx={{
                fontFamily: typography.fontFamily.display,
                color: '#FFFFFF',
                fontWeight: typography.fontWeight.regular,
                mb: 3,
                fontSize: { xs: '1.75rem', md: '2.5rem' },
                lineHeight: 1.2,
              }}
            >
              Ready to Discover
              <br />
              Your Next Treasure?
            </Typography>
            <Typography
              sx={{
                color: 'rgba(255,255,255,0.6)',
                mb: 6,
                maxWidth: 440,
                mx: 'auto',
                fontSize: '0.9rem',
                lineHeight: 1.7,
              }}
            >
              Join thousands of collectors worldwide. Start bidding on authenticated,
              extraordinary pieces today.
            </Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} justifyContent="center">
              <Button
                variant="contained"
                size="large"
                endIcon={<East />}
                component={Link}
                to="/register"
                sx={{
                  bgcolor: '#FFFFFF',
                  color: '#1C1917',
                  px: 5,
                  py: 1.75,
                  fontSize: '0.8125rem',
                  fontWeight: typography.fontWeight.medium,
                  fontFamily: typography.fontFamily.body,
                  textTransform: 'uppercase',
                  letterSpacing: '0.1em',
                  borderRadius: 0,
                  boxShadow: 'none',
                  '&:hover': {
                    bgcolor: '#F5F5F4',
                    boxShadow: 'none',
                  },
                }}
              >
                Create Free Account
              </Button>
              <Button
                variant="outlined"
                size="large"
                component={Link}
                to="/auctions"
                sx={{
                  borderColor: 'rgba(255,255,255,0.25)',
                  color: '#FFFFFF',
                  px: 5,
                  py: 1.75,
                  fontSize: '0.8125rem',
                  fontWeight: typography.fontWeight.medium,
                  fontFamily: typography.fontFamily.body,
                  textTransform: 'uppercase',
                  letterSpacing: '0.1em',
                  borderRadius: 0,
                  '&:hover': {
                    borderColor: '#FFFFFF',
                    bgcolor: 'rgba(255,255,255,0.05)',
                  },
                }}
              >
                Browse Auctions
              </Button>
            </Stack>
          </Box>
        </motion.div>
      </Container>
    </Box>
  )
}
