import { Box, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { AnimatedCounter } from '@/shared/components/motion'
import { typography } from '@/shared/theme/tokens'

interface HeroStat {
  label: string
  value: number
  prefix?: string
  suffix?: string
}

interface HeroStatsProps {
  stats: readonly HeroStat[]
}

export const HeroStats = ({ stats }: HeroStatsProps) => {
  return (
    <Box
      sx={{
        display: 'flex',
        gap: { xs: 4, md: 8 },
        flexWrap: 'wrap',
      }}
    >
      {stats.map((stat, index) => (
        <motion.div
          key={stat.label}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 + index * 0.1, duration: 0.5 }}
        >
          <Box
            sx={{
              position: 'relative',
              pl: index > 0 ? { md: 6 } : 0,
              '&::before': index > 0
                ? {
                    content: '""',
                    position: 'absolute',
                    left: 0,
                    top: '10%',
                    height: '80%',
                    width: 1,
                    background: 'rgba(255,255,255,0.2)',
                    display: { xs: 'none', md: 'block' },
                  }
                : undefined,
            }}
          >
            <Typography
              sx={{
                fontFamily: typography.fontFamily.display,
                color: '#FFFFFF',
                fontWeight: typography.fontWeight.light,
                mb: 0.5,
                fontSize: { xs: '2rem', md: '2.75rem' },
                lineHeight: 1,
              }}
            >
              <AnimatedCounter value={stat.value} prefix={stat.prefix} suffix={stat.suffix} />
            </Typography>
            <Typography
              sx={{
                color: 'rgba(255,255,255,0.6)',
                textTransform: 'uppercase',
                letterSpacing: '0.15em',
                fontFamily: typography.fontFamily.body,
                fontSize: '0.625rem',
                fontWeight: typography.fontWeight.medium,
              }}
            >
              {stat.label}
            </Typography>
          </Box>
        </motion.div>
      ))}
    </Box>
  )
}
