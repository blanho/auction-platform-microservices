import { palette } from './tokens'

export const componentStyles = {
  sectionLabel: {
    fontWeight: 600,
    color: palette.neutral[900],
    mb: 2,
    textTransform: 'uppercase' as const,
    letterSpacing: 1,
    fontSize: '0.7rem',
  },

  brandCheckbox: {
    color: palette.neutral[500],
    '&.Mui-checked': { color: palette.brand.primary },
  },

  brandChip: {
    bgcolor: palette.brand.muted,
    color: palette.brand.primary,
    '& .MuiChip-deleteIcon': { color: palette.brand.primary },
  },

  textBrandButton: {
    color: palette.brand.primary,
    textTransform: 'none' as const,
    p: 0,
    '&:hover': { bgcolor: 'transparent', textDecoration: 'underline' },
  },
} as const
