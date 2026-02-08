import { motion } from 'framer-motion'
import { Box, Container, Typography, Grid, Avatar } from '@mui/material'
import { FormatQuote } from '@mui/icons-material'
import { typography } from '@/shared/theme/tokens'

const testimonials = [
  {
    id: 'testimonial-1',
    name: 'Alexandra Chen',
    role: 'Art Collector',
    avatar: '',
    quote: 'The authentication process gave me complete peace of mind. I acquired a piece I had been searching for over two years — flawlessly.',
  },
  {
    id: 'testimonial-2',
    name: 'Marcus Rothwell',
    role: 'Vintage Watch Dealer',
    avatar: '',
    quote: 'As a seller, the platform exposure is unmatched. My listings consistently attract serious, qualified bidders from around the world.',
  },
  {
    id: 'testimonial-3',
    name: 'Sofia Bergström',
    role: 'Interior Designer',
    avatar: '',
    quote: 'I source unique pieces for my clients here regularly. The curation quality and condition reports save me hours of due diligence.',
  },
]

export const TestimonialsSection = () => {
  return (
    <Box sx={{ py: { xs: 10, md: 16 }, bgcolor: '#FAFAF9', position: 'relative' }}>
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
              Testimonials
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
              Trusted by Collectors Worldwide
            </Typography>
          </Box>
        </motion.div>

        <Grid container spacing={4}>
          {testimonials.map((testimonial, index) => (
            <Grid size={{ xs: 12, md: 4 }} key={testimonial.id}>
              <motion.div
                initial={{ opacity: 0, y: 25 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: index * 0.12, duration: 0.5 }}
              >
                <Box
                  sx={{
                    p: 4,
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    borderTop: '1px solid #E7E5E4',
                    transition: 'border-color 0.3s ease',
                    '&:hover': {
                      borderTopColor: '#1C1917',
                    },
                  }}
                >
                  <FormatQuote
                    sx={{
                      color: '#D6D3D1',
                      fontSize: 32,
                      mb: 2,
                      transform: 'rotate(180deg)',
                    }}
                  />
                  <Typography
                    sx={{
                      color: '#57534E',
                      lineHeight: 1.8,
                      fontSize: '0.875rem',
                      fontStyle: 'italic',
                      flex: 1,
                      mb: 4,
                    }}
                  >
                    {testimonial.quote}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar
                      src={testimonial.avatar}
                      sx={{
                        width: 40,
                        height: 40,
                        bgcolor: '#F5F5F4',
                        color: '#78716C',
                        fontFamily: typography.fontFamily.display,
                        fontWeight: typography.fontWeight.medium,
                        fontSize: '0.875rem',
                      }}
                    >
                      {testimonial.name.charAt(0)}
                    </Avatar>
                    <Box>
                      <Typography
                        sx={{
                          color: '#1C1917',
                          fontWeight: typography.fontWeight.medium,
                          fontSize: '0.8125rem',
                        }}
                      >
                        {testimonial.name}
                      </Typography>
                      <Typography
                        sx={{
                          color: '#A8A29E',
                          fontSize: '0.75rem',
                          letterSpacing: '0.05em',
                        }}
                      >
                        {testimonial.role}
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
