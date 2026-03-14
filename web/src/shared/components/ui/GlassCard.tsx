import { Box } from '@mui/material'
import type { BoxProps } from '@mui/material'
import { colors } from '../../theme/tokens'

interface GlassCardProps extends BoxProps {
  children: React.ReactNode
}

export const GlassCard = ({ children, sx = {}, ...props }: GlassCardProps) => (
  <Box
    sx={{
      background: colors.glass.background,
      border: `1px solid ${colors.glass.border}`,
      borderRadius: 0,
      ...sx,
    }}
    {...props}
  >
    {children}
  </Box>
)
