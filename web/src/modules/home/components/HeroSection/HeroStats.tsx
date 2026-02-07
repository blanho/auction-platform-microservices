import { Box, Typography } from '@mui/material'
import { motion } from 'framer-motion'
import { AnimatedCounter } from '@/shared/components/motion'
import { colors, typography, transitions } from '@/shared/theme/tokens'

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
    <Box sx={{ display: 'flex', gap: 6, flexWrap: 'wrap' }}>
      {stats.map((stat, index) => (
        <motion.div
          key={stat.label}
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 + index * 0.1, duration: transitions.duration.normal }}
        >
          <Typography
            variant="h4"
            sx={{
              fontFamily: typography.fontFamily.body,
              color: colors.gold.primary,
              fontWeight: typography.fontWeight.bold,
              mb: 0.5,
              fontSize: { xs: '1.75rem', md: '2.25rem' },
            }}
          >
            <AnimatedCounter value={stat.value} prefix={stat.prefix} suffix={stat.suffix} />
          </Typography>
          <Typography
            variant="caption"
            sx={{
              color: colors.text.disabled,
              textTransform: 'uppercase',
              letterSpacing: 2,
              fontFamily: typography.fontFamily.body,
            }}
          >
            {stat.label}
          </Typography>
        </motion.div>
      ))}
    </Box>
  )
}
