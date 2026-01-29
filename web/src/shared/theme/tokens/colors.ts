export const palette = {
  brand: {
    primary: '#CA8A04',
    secondary: '#EAB308',
    accent: '#FCD34D',
    muted: 'rgba(202, 138, 4, 0.15)',
    hover: '#A16207',
    dark: '#92400E',
  },

  purple: {
    primary: '#9333EA',
    light: 'rgba(147, 51, 234, 0.1)',
    muted: 'rgba(147, 51, 234, 0.08)',
  },

  semantic: {
    success: '#22C55E',
    successHover: '#15803D',
    successLight: 'rgba(34, 197, 94, 0.1)',
    successMuted: 'rgba(34, 197, 94, 0.2)',
    error: '#EF4444',
    errorHover: '#B91C1C',
    errorLight: 'rgba(239, 68, 68, 0.1)',
    warning: '#F59E0B',
    warningHover: '#D97706',
    warningLight: 'rgba(245, 158, 11, 0.1)',
    info: '#3B82F6',
    infoHover: '#1D4ED8',
    infoLight: 'rgba(59, 130, 246, 0.1)',
  },

  neutral: {
    0: '#FFFFFF',
    50: '#FAFAF9',
    100: '#F5F5F4',
    200: '#E7E5E4',
    300: '#D6D3D1',
    400: '#A8A29E',
    500: '#78716C',
    600: '#57534E',
    700: '#44403C',
    800: '#292524',
    900: '#1C1917',
    950: '#0A0A0A',
  },
} as const

export const colors = {
  light: {
    bg: {
      primary: palette.neutral[50],
      secondary: palette.neutral[100],
      tertiary: palette.neutral[200],
      elevated: palette.neutral[0],
      glass: 'rgba(0, 0, 0, 0.02)',
      overlay: 'rgba(0, 0, 0, 0.5)',
    },
    text: {
      primary: palette.neutral[900],
      secondary: palette.neutral[600],
      muted: palette.neutral[500],
      disabled: palette.neutral[400],
      inverse: palette.neutral[50],
    },
    border: {
      subtle: palette.neutral[100],
      default: palette.neutral[200],
      strong: palette.neutral[300],
      focus: palette.brand.primary,
    },
    brand: {
      primary: palette.brand.primary,
      secondary: palette.brand.secondary,
      accent: palette.brand.accent,
      muted: palette.brand.muted,
    },
  },
  dark: {
    bg: {
      primary: palette.neutral[950],
      secondary: palette.neutral[900],
      tertiary: palette.neutral[800],
      elevated: palette.neutral[800],
      glass: 'rgba(255, 255, 255, 0.05)',
      overlay: 'rgba(0, 0, 0, 0.7)',
    },
    text: {
      primary: palette.neutral[50],
      secondary: palette.neutral[400],
      muted: palette.neutral[500],
      disabled: palette.neutral[600],
      inverse: palette.neutral[900],
    },
    border: {
      subtle: 'rgba(255, 255, 255, 0.05)',
      default: 'rgba(255, 255, 255, 0.1)',
      strong: 'rgba(255, 255, 255, 0.2)',
      focus: palette.brand.secondary,
    },
    brand: {
      primary: palette.brand.primary,
      secondary: palette.brand.secondary,
      accent: palette.brand.accent,
      muted: palette.brand.muted,
    },
  },
} as const

export const gradients = {
  brand: `linear-gradient(135deg, ${palette.brand.primary} 0%, ${palette.brand.secondary} 100%)`,
  brandHover: `linear-gradient(135deg, ${palette.brand.secondary} 0%, ${palette.brand.accent} 100%)`,
  brandShimmer: `linear-gradient(135deg, ${palette.brand.primary} 0%, ${palette.brand.accent} 50%, ${palette.brand.primary} 100%)`,
  heroGlow: `radial-gradient(ellipse 80% 50% at 50% -20%, ${palette.brand.muted}, transparent)`,
  purpleGlow: `radial-gradient(ellipse 60% 40% at 100% 100%, ${palette.purple.light}, transparent)`,
  cardOverlay: 'linear-gradient(to top, rgba(0,0,0,0.8) 0%, rgba(0,0,0,0.2) 50%, transparent 100%)',
  cardOverlayHover:
    'linear-gradient(to top, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.3) 50%, transparent 100%)',
  divider: `linear-gradient(90deg, transparent, rgba(202, 138, 4, 0.3), transparent)`,
} as const

export type ThemeMode = 'light' | 'dark'
export type ColorTokens = typeof colors.light
export type Palette = typeof palette
export type Gradients = typeof gradients
