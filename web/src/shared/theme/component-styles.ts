import { palette } from './tokens'

/**
 * Shared MUI `sx` style objects for commonly repeated patterns.
 * Import these instead of writing inline sx objects to keep components DRY.
 *
 * Usage: <Typography sx={componentStyles.sectionLabel}>...</Typography>
 */
export const componentStyles = {
  /** Uppercase section label (e.g. "Filter", "Category") */
  sectionLabel: {
    fontWeight: 600,
    color: palette.neutral[900],
    mb: 2,
    textTransform: 'uppercase' as const,
    letterSpacing: 1,
    fontSize: '0.7rem',
  },

  /** Standard brand-colored checkbox */
  brandCheckbox: {
    color: palette.neutral[500],
    '&.Mui-checked': { color: palette.brand.primary },
  },

  /** Chip styled with brand muted background and primary text */
  brandChip: {
    bgcolor: palette.brand.muted,
    color: palette.brand.primary,
    '& .MuiChip-deleteIcon': { color: palette.brand.primary },
  },

  /** Dark filled primary action button */
  darkButton: {
    bgcolor: palette.neutral[900],
    color: palette.neutral[50],
    textTransform: 'none' as const,
    fontWeight: 600,
    borderRadius: 0,
    '&:hover': { bgcolor: palette.neutral[700] },
    '&.Mui-disabled': { bgcolor: palette.neutral[100], color: palette.neutral[500] },
  },

  /** Ghost/outlined button with dark border */
  outlinedDarkButton: {
    borderColor: palette.neutral[900],
    color: palette.neutral[900],
    textTransform: 'none' as const,
    borderRadius: 0,
    fontWeight: 500,
    '&:hover': {
      borderColor: palette.neutral[900],
      bgcolor: 'rgba(28,25,23,0.05)',
    },
  },

  /** Text-only link button in brand color */
  textBrandButton: {
    color: palette.brand.primary,
    textTransform: 'none' as const,
    p: 0,
    '&:hover': { bgcolor: 'transparent', textDecoration: 'underline' },
  },
} as const
